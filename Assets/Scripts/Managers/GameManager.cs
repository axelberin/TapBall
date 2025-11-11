using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static string UnlokedSkin = "";

    public enum GameModes
    {
        Null,
        Dunk,               // Obstacle mode. Less touches. A lot of levels.
        Time,               // Time mode. Less Time. A lot of levels.
        OneTouch,            // Limit touches. Less touches than limit. A lot of levels.
        Endless,            // Endless. More time. One procedural level.
        Fall,               // Fall mode. More time. One procedural level.
    }

    GameModes _currentGameMode = GameModes.Dunk;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(gameObject);

        Application.targetFrameRate = 60;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Its called from buttons.
    /// </summary>
    /// <param name="gameModeIndex"></param>
    public void SelectGameMode(int gameModeIndex)
    {
        switch (gameModeIndex)
        {
            case 0: _currentGameMode = GameModes.Null; break;
            case 1: _currentGameMode = GameModes.Dunk; break;
            case 2: _currentGameMode = GameModes.Time; break;
            case 3: _currentGameMode = GameModes.OneTouch; break;
            case 4: break;
            case 5:
            default:
                _currentGameMode = GameModes.Null;
                Debug.LogWarning($"Game mode not found: " + gameModeIndex);
                break;
        }
    }

    public void SetCurrentModeByIndex(int index)
    {
        int last = (SaveAndLoadManager.GetHighestLevelReachedByGameModeAndWorld(GameModes.Dunk, "Neon") / 5) + 1;

        int limit = 3; //TODO: Modificar cuando se agreguen modos.
        if (last > limit)
            last = limit;

        if (_currentGameMode == GameModes.Dunk && index < 0)
            SelectGameMode(last);
        else if (((int)_currentGameMode) == last && index > 0)
            SelectGameMode((int)GameModes.Dunk);
        else
            SelectGameMode(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CurrentModeName) + index);

        SaveAndLoadManager.SetIntValue((int)_currentGameMode, SaveAndLoadManager.CurrentModeName);
    }

    public bool IsModeUnlocked(GameModes mode)
        => SaveAndLoadManager.GetIntValue(SaveAndLoadManager.ObtainedGameMode + mode) == 1;

    public void UnlockMode(int level)
    {
        if (_currentGameMode != GameModes.Dunk)
            return;

        GameModes mode = GetGameModeUnlockByLevel(level);

        if (mode == GameModes.Null || IsModeUnlocked(mode))
            return;

        if (SaveAndLoadManager.GetIntValue(SaveAndLoadManager.ObtainedGameMode + mode) == 0)
            SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.ObtainedGameMode + mode, true, true);
    }

    public void OnCompleteWorld(string currentWorldName)
    {
        if (SaveAndLoadManager.GetIntValue(SaveAndLoadManager.ObtainedBallSkins + currentWorldName) == 1
            && _currentGameMode != GameModes.Dunk)
            return;

        SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.ObtainedBallSkins + currentWorldName, true, true);
        UnlokedSkin = currentWorldName;
    }

    private GameModes GetGameModeUnlockByLevel(int level)
    {
        return level switch
        {
            5 => GameModes.Time,
            10 => GameModes.OneTouch,
            _ => GameModes.Null,
        };
    }

    public PlayerController SetGetPlayer { set; get; }

    public TapController SetGetTapController { set; get; }

    public WorldStateController SetGetWorldState { set; get; }

    public CameraController SetGetCameraController { set; get; }
    public DeathController SetGetDeathController { set; get; }
    public GameModes GetCurrentGameMode => _currentGameMode;
}
