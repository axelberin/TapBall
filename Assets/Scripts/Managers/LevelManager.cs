using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    private int _gameCoins;
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
        GameManager.Instance.SetGetPlayer.GetRigidbody.bodyType = RigidbodyType2D.Static;

        int currentCoins = SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName) + _gameCoins;
        SaveAndLoadManager.SetIntValue(currentCoins, SaveAndLoadManager.CoinsName);

        switch (GameManager.Instance.GetCurrentGameMode)
        {
            case GameModes.Dunk:
                {
                    DunkLevelCanvas.Instance.OnWin();
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
                }
                break;
            default:
                break;
        }

        _gameCoins = 0;
        _coinsObtained.Clear();
    }

    public void OnLose()
    {
        if (!GameManager.Instance.SetGetWorldState)
            return;

        GameManager.Instance.SetGetPlayer.GetRigidbody.bodyType = RigidbodyType2D.Static;
        GameManager.Instance.SetGetPlayer.transform.position = GameManager.Instance.SetGetWorldState.GetInitalPos;
        _coinsObtained.ForEach(coin => coin.gameObject.SetActive(true));
        _coinsObtained.Clear();
        switch (GameManager.Instance.GetCurrentGameMode)
        {
            case GameModes.Dunk:
                {
                    _gameCoins = 0;
                    GameManager.Instance.SetGetTapController.SetGetTapCount = 0;
                    DunkLevelCanvas.Instance.OnLose();
                    GameManager.Instance.SetGetWorldState.SetOnUpdate(GameManager.Instance.SetGetWorldState.StartCount);
                }
                break;
            default:
                break;
        }
    }

    public void OnGetCoin(Coins coinName)
    {
        _gameCoins++;
        _coinsObtained.Add(coinName);
    }

    public int SetCoins
    {
        set => _gameCoins = value;
    }
}
