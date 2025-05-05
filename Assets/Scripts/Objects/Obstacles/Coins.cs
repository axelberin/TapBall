using System.Collections;
using UnityEngine;

public class Coins : ObstaclesManager
{
    private string _coinName;

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

        _coinName = SaveAndLoadManager.CoinNameByLevel +
             GameManager.Instance.GetCurrentGameMode +
             GameManager.Instance.SetGetWorldState.GetLevel;

        int hasCoin = SaveAndLoadManager.GetIntValue(_coinName);

        gameObject.SetActive(hasCoin == default || hasCoin == 0);
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

    public string GetCoinName => _coinName;
}
