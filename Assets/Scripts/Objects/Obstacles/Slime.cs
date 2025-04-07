using UnityEngine;

public class Slime : MonoBehaviour
{
    private AudioSource _audioSource;

    private void Awake()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController player))
            _audioSource.Play();
    }
}
