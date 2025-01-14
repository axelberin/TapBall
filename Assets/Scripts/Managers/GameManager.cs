using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private PlayerController _playerController;
    private TapController _tapController;
    private WorldStateController _worldStateController;

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
