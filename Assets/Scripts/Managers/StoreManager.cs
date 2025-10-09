using System;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance { get; private set; }
    public Action UpdateSkinsState { get; set; }

    int _coins;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);
    }

    private void OnEnable()
    {
        UpdateSkinsState?.Invoke();
        UpdateCoins();
        if (IAPManager.Instance)
            IAPManager.Instance.OnCompletePurchase += UpdateCoins;
    }

    private void OnDisable()
    {
        if (IAPManager.Instance)
            IAPManager.Instance.OnCompletePurchase -= UpdateCoins;
    }

    private void UpdateCoins()
    {
        _coins = SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName);
    }

    public void Buy(int cost)
    {
        _coins -= cost;
        SaveAndLoadManager.SetIntValue(_coins, SaveAndLoadManager.CoinsName, true);
    }

    public bool CanBuy(int cost, bool buy)
    {
        if (_coins - cost < 0)
            return false;

        if (buy)
            Buy(cost);

        return true;
    }
}
