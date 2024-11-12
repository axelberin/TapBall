using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    private int _gameCoins;
    private List<string> _coinsNames = new List<string>();

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

                    foreach (var coinName in _coinsNames)
                        SaveAndLoadManager.SetIntValue(1, coinName);

                    SaveAndLoadManager.SetIntValue(GameManager.Instance.SetGetPlayer.HasDeath ? 0 : 1,
                        SaveAndLoadManager.DunkWithoutDeathName + level, true);
                }
                break;
            default:
                break;
        }

        _gameCoins = 0;
    }

    public void OnLose()
    {
        if (!GameManager.Instance.SetGetWorldState)
            return;

        GameManager.Instance.SetGetPlayer.GetRigidbody.bodyType = RigidbodyType2D.Static;
        GameManager.Instance.SetGetPlayer.transform.position = GameManager.Instance.SetGetWorldState.GetInitalPos;
        GameManager.Instance.SetGetWorldState.GetBaseController.ResetMovement();
        _coinsNames.Clear();
        switch (GameManager.Instance.GetCurrentGameMode)
        {
            case GameModes.Dunk:
                {
                    GameManager.Instance.SetGetTapController.SetGetTapCount = 0;
                    DunkLevelCanvas.Instance.OnLose();
                    GameManager.Instance.SetGetWorldState.SetOnUpdate(GameManager.Instance.SetGetWorldState.StartCount);
                }
                break;
            default:
                break;
        }
    }

    public void OnGetCoin(string coinName)
    {
        _gameCoins++;
        _coinsNames.Add(coinName);
    }

    public void OnResumeGame()
    {

    }

    public void OnPauseGame()
    {

    }

    public int SetCoins
    {
        set => _gameCoins = value;
    }
}
