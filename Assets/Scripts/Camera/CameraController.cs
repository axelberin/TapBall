using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _duration;
    [SerializeField] private float _magnitude;

    void Start()
    {
        LevelManager.Instance.OnLoseLevel += StartShake;
    }

    private void OnDestroy()
    {
        LevelManager.Instance.OnLoseLevel -= StartShake;
    }

    private void StartShake()
    {
        StartCoroutine(Shake(_duration, _magnitude));
    }

    IEnumerator Shake(float duration, float magnitude)
    {
        // Guardo la pos original para volver a setearla cuando termina el efecto
        Vector3 originalPos = transform.localPosition;
        // Creo el vector aca por optimizacion de codigo
        Vector3 newPos = Vector3.zero;
        // Tiempo que va pasando en el efecto = 0
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Genero la nueva posicion teniendo en cuenta la magnitud por parametro
            newPos.x = Random.Range(-0.5f, 0.5f) * magnitude;
            newPos.z = transform.localPosition.z;
            newPos.y = transform.localPosition.y;

            // Seteo la nueva pos
            transform.localPosition = newPos;
            // Agrego el tiempo que paso
            elapsed += Time.deltaTime;
            yield return null;
        }
        // Seteo la posicion original
        transform.localPosition = originalPos;
    }
}
