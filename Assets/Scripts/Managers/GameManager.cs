using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    int _gameCoins;

    PlayerController _playerController;
    TapController _tapController;
    WorldStateController _worldStateController;

    public enum GameModes
    {
        Dunk,               // Obstacle mode. Less touches. A lot of levels.
        Endless,            // Endless. More time. Procedural.
        Time,               // Time mode. Less Time. A lot of levels.
        OneTouch            // Limit touches. Less touches than limit. A lot of levels.
    }

    GameModes _currentGameMode = GameModes.Dunk;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Its called from buttons.
    /// </summary>
    /// <param name="gameModes"></param>
    public void SelectGameMode(int gameModes)
    {
        if (gameModes == 0)
            _currentGameMode = GameModes.Dunk;
    }

    public void OnWin()
    {
        _playerController.GetRigidbody.bodyType = RigidbodyType2D.Static;

        UIManager.Instance.ActivateUI(UIManager.Instance.winTime.gameObject, false);
        UIManager.Instance.ActivateUI(UIManager.Instance.winText.gameObject, true);

        int currentCoins = LoadAndSaveManager.GetIntValue(LoadAndSaveManager.CoinsName) + _gameCoins;
        LoadAndSaveManager.SaveIntValue(currentCoins, LoadAndSaveManager.CoinsName);

        switch (_currentGameMode)
        {
            case GameModes.Dunk:
                {
                    if (JSON.Instance.GetDunkData.S_DunkBest[_worldStateController.GetLevel - 1] == default)
                        JSON.Instance.GetDunkData.S_DunkBest[_worldStateController.GetLevel - 1] = _tapController.SetGetTapCount;
                    else if (JSON.Instance.GetDunkData.S_DunkBest[_worldStateController.GetLevel - 1] > _tapController.SetGetTapCount)
                        JSON.Instance.GetDunkData.S_DunkBest[_worldStateController.GetLevel - 1] = _tapController.SetGetTapCount;

                    if (!JSON.Instance.GetDunkData.S_DunkLevels.Contains(_worldStateController.GetLevel))
                        JSON.Instance.GetDunkData.S_DunkLevels.Add(_worldStateController.GetLevel);

                    if (!JSON.Instance.GetDunkData.S_DunkWithoutDeath[_worldStateController.GetLevel - 1])
                        JSON.Instance.GetDunkData.S_DunkWithoutDeath[_worldStateController.GetLevel - 1] = !_playerController.GetDeath;

                    JSON.Instance.SaveDunkData();
                }
                break;
            default:
                break;
        }

        _gameCoins = 0;
    }

    public void OnLose()
    {
        if (!_worldStateController)
            return;

        _playerController.GetRigidbody.bodyType = RigidbodyType2D.Static;
        _playerController.transform.position = _worldStateController.GetInitalPos;
        _worldStateController.GetBaseController.ResetMovement();
        switch (_currentGameMode)
        {
            case GameModes.Dunk:
                {
                    _tapController.SetGetTapCount = 0;
                    UIManager.Instance.SetText(UIManager.Instance.pointsCount, 0);
                    _worldStateController.OnUpdate = _worldStateController.StartCount;
                }
                break;
            default:
                break;
        }
    }

    public void GetCoin()
    {
        _gameCoins++;
        // ui coins
    }

    public int SetCoins
    {
        set => _gameCoins = value;
    }

    public PlayerController SetGetPlayer
    {
        set => _playerController = value;
        get => _playerController;
    }

    public TapController SetGetTapController
    {
        set => _tapController = value;
        get => _tapController;
    }

    public WorldStateController SetGetWorldState
    {
        set => _worldStateController = value;
        get => _worldStateController;
    }

    public GameModes GetCurrentGameMode => _currentGameMode;
}
