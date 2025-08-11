using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static string UnlokedSkin = "";

    public enum GameModes
    {
        Null,
        Dunk,               // Obstacle mode. Less touches. A lot of levels.
        Endless,            // Endless. More time. One procedural level.
        Time,               // Time mode. Less Time. A lot of levels.
        OneTouch,            // Limit touches. Less touches than limit. A lot of levels.
        Fall,               // Fall mode. More time. One procedural level.
    }

    GameModes _currentGameMode = GameModes.Dunk;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        SaveAndLoadManager.SetDebugMode(true);
    }

    /// <summary>
    /// Its called from buttons.
    /// </summary>
    /// <param name="gameModeIndex"></param>
    public void SelectGameMode(int gameModeIndex)
    {
        switch (gameModeIndex)
        {
            case 0:
                _currentGameMode = GameModes.Null;
                break;
            case 1:
                _currentGameMode = GameModes.Dunk;
                break;
            default:
                _currentGameMode = GameModes.Null;
                Debug.LogWarning($"Game mode not found: " + gameModeIndex);
                break;
        }
    }

    public void OnCompleteWorld(string currentWorldName)
    {
        if (SaveAndLoadManager.GetIntValue(SaveAndLoadManager.ObtainedBallSkins + currentWorldName) == 1)
            return;

        SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.ObtainedBallSkins + currentWorldName, true);
        UnlokedSkin = currentWorldName;
    }

    public PlayerController SetGetPlayer { set; get; }

    public TapController SetGetTapController { set; get; }

    public WorldStateController SetGetWorldState { set; get; }

    public CameraController SetGetCameraController { set; get; }
    public DeathController SetGetDeathController { set; get; }

    public GameModes GetCurrentGameMode => _currentGameMode;
}
