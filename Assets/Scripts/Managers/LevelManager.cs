using System;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public Action OnWinLevel = delegate { };
    public Action OnLoseLevel = delegate { };

    private List<Coins> _coinsObtained = new List<Coins>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void OnWin()
    {
        OnWinLevel?.Invoke();

        int savedCoins = 0;
        if (SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CoinsName))
            savedCoins = SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName);

        int currentCoins = savedCoins + _coinsObtained.Count;
        SaveAndLoadManager.SetIntValue(currentCoins, SaveAndLoadManager.CoinsName);

        switch (GameManager.Instance.GetCurrentGameMode)
        {
            case GameModes.Dunk:
                DunkOnWin();
                break;
            default:
                break;
        }

        _coinsObtained.Clear();
    }

    private void DunkOnWin()
    {
        int level = GameManager.Instance.SetGetWorldState.GetLevel - 1;
        int tapCount = GameManager.Instance.SetGetTapController.SetGetTapCount;

        if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.DunkBestName + level) ||
            SaveAndLoadManager.GetIntValue(SaveAndLoadManager.DunkBestName + level) > tapCount)
            SaveAndLoadManager.SetIntValue(tapCount, SaveAndLoadManager.DunkBestName + level);

        if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.DunkLevelName + level))
            SaveAndLoadManager.SetIntValue(level, SaveAndLoadManager.DunkLevelName + level);

        foreach (var coinName in _coinsObtained)
            SaveAndLoadManager.SetIntValue(1, coinName.GetCoinName);

        SaveAndLoadManager.SetIntValue(GameManager.Instance.SetGetPlayer.HasDeath ? 0 : 1,
            SaveAndLoadManager.DunkWithoutDeathName + level, true);

        DunkLevelCanvas.Instance.SetTouchesInLevel(tapCount,
           tapCount > GameManager.Instance.SetGetWorldState.GetLimitTouches);
    }

    public void OnLose()
    {
        OnLoseLevel?.Invoke();

        _coinsObtained.ForEach(coin => coin.OnLose());
        _coinsObtained.Clear();

        switch (GameManager.Instance.GetCurrentGameMode)
        {
            case GameModes.Dunk:
                GameManager.Instance.SetGetTapController.SetGetTapCount = 0;
                break;
            default:
                break;
        }
    }

    public void OnGetCoin(Coins coinName)
    {
        _coinsObtained.Add(coinName);
    }

    public void ResetCoins()
    {
        _coinsObtained.Clear();
    }

    public bool HasGetedCoins => _coinsObtained.Count > 0 ||
        SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CoinNameByLevel +
             GameManager.Instance.GetCurrentGameMode +
             GameManager.Instance.SetGetWorldState.GetLevel);
}
