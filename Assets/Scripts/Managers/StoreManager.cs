using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    int _coins;

    private void Start()
    {
        if (GameManager.Instance)
        {
            _coins = JSON.Instance.GetPlayerData.coins;
            GameManager.Instance.SetCoins = 0;
        }
    }

    public void Buy(int cost)
    {
        if (CanBuy(cost)) _coins -= cost;
    }

    bool CanBuy(int cost)
    {
        if (_coins - cost < 0) return false;

        return true;
    }
}
