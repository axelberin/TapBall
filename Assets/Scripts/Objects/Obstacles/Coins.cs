using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coins : ObstaclesManager
{
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
