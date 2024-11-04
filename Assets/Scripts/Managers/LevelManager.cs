using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    private int _gameCoins;

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

        UIManager.Instance.ActivateUI(UIManager.Instance.winTime.gameObject, false);
        UIManager.Instance.ActivateUI(UIManager.Instance.winText.gameObject, true);

        int currentCoins = LoadAndSaveManager.GetIntValue(LoadAndSaveManager.CoinsName) + _gameCoins;
        LoadAndSaveManager.SaveIntValue(currentCoins, LoadAndSaveManager.CoinsName);

        switch (GameManager.Instance.GetCurrentGameMode)
        {
            case GameModes.Dunk:
                {
                    int level = GameManager.Instance.SetGetWorldState.GetLevel;
                    int tapCount = GameManager.Instance.SetGetTapController.SetGetTapCount;

                    if (!LoadAndSaveManager.ContainsKey(LoadAndSaveManager.DunkBestName + level) ||
                        LoadAndSaveManager.GetIntValue(LoadAndSaveManager.DunkBestName + level) > tapCount)
                        LoadAndSaveManager.SaveIntValue(tapCount, LoadAndSaveManager.DunkBestName + level);

                    if (!LoadAndSaveManager.ContainsKey(LoadAndSaveManager.DunkLevelName + level))
                        LoadAndSaveManager.SaveIntValue(level, LoadAndSaveManager.DunkLevelName + level);

                    LoadAndSaveManager.SaveIntValue(GameManager.Instance.SetGetPlayer.HasDeath ? 0 : 1,
                        LoadAndSaveManager.DunkWithoutDeathName + level, true);
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
        switch (GameManager.Instance.GetCurrentGameMode)
        {
            case GameModes.Dunk:
                {
                    GameManager.Instance.SetGetTapController.SetGetTapCount = 0;
                    UIManager.Instance.SetText(UIManager.Instance.pointsCount, 0);
                    GameManager.Instance.SetGetWorldState.SetOnUpdate(GameManager.Instance.SetGetWorldState.StartCount);
                }
                break;
            default:
                break;
        }
    }

    public void OnGetCoin()
    {
        _gameCoins++;
        // ui coins
    }

    public int SetCoins
    {
        set => _gameCoins = value;
    }
}
