using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coins : ObstaclesManager
{
    private string _coinName;

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
            gameObject.SetActive(false);
        }
    }

    public string GetCoinName => _coinName;
}
