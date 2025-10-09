using System.Collections;
using UnityEngine;

public class Coins : ObstaclesManager
{
    private string _currentWorld = "Neon"; // Temporal hasta que implementes mundos

    private Animator _animator;
    private AudioSource _audioSource;
    private CircleCollider2D _circleCollider;

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();

        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

        if (_circleCollider == null)
            _circleCollider = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        StartCoroutine(DelayToSet());
    }

    private IEnumerator DelayToSet()
    {
        yield return new WaitForSeconds(0.05f);

        // Verificar si ya se obtuvo la moneda usando el nuevo sistema
        bool hasCoin = SaveAndLoadManager.GetLevelCoinObtained(
            GameManager.Instance.GetCurrentGameMode,
            _currentWorld,
            GameManager.Instance.SetGetWorldState.GetLevel);

        // Activar o desactivar la moneda según si ya se obtuvo
        gameObject.SetActive(!hasCoin);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerController player))
        {
            _circleCollider.enabled = false;
            LevelManager.Instance.OnGetCoin(this);
            StartCoroutine(WaitForAnimation());
        }
    }

    private IEnumerator WaitForAnimation()
    {
        if (_audioSource)
            _audioSource.Play();

        _animator.SetTrigger("Geted");
        yield return new WaitForSeconds(0.75f);
        gameObject.SetActive(false);
    }

    public void OnLose()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        _animator.SetTrigger("Lose");
        _circleCollider.enabled = true;
    }
}