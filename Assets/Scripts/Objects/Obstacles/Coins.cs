using System;
using System.Collections;
using UnityEngine;

public class Coins : ObstaclesManager
{
    private string _coinName;

    private Animator _animator;

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _coinName = SaveAndLoadManager.CoinNameByLevel +
             GameManager.Instance.GetCurrentGameMode + ScenesManager.Instance.GetCurrentSceneName();
        int hasCoin = SaveAndLoadManager.GetIntValue(_coinName);

        gameObject.SetActive(hasCoin == default || hasCoin == 0);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerController player))
        {
            LevelManager.Instance.OnGetCoin(this);
            StartCoroutine(WaitForAnimation());
        }
    }

    private IEnumerator WaitForAnimation()
    {
        _animator.SetTrigger("Geted");
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }

    public void OnLose()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        _animator.SetTrigger("Lose");
    }

    public string GetCoinName => _coinName;
}
