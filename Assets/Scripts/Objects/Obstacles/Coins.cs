using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coins : ObstaclesManager
{
    private void Start()
    {
        int hasCoin = SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinNameByLevel +
             GameManager.Instance.GetCurrentGameMode + ScenesManager.Instance.GetCurrentSceneName());

        gameObject.SetActive(hasCoin == default || hasCoin == 0);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player)
        {
            LevelManager.Instance.OnGetCoin();
            gameObject.SetActive(false);
        }
    }
}
