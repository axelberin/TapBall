using System;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public Action OnWinLevel = delegate { };
    public Action OnLoseLevel = delegate { };

    private int _deathCount = 0;
    private List<Coins> _coinsObtained = new List<Coins>();

    // Mundo actual - temporal hasta que implementes el sistema de mundos
    private string _currentWorld = "Neon";

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

        // Guardar monedas obtenidas
        int savedCoins = 0;
        if (SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CoinsName))
            savedCoins = SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName);

        int currentCoins = savedCoins + _coinsObtained.Count;
        SaveAndLoadManager.SetIntValue(currentCoins, SaveAndLoadManager.CoinsName);

        AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.WinSound);

        // Procesar según el modo de juego
        switch (GameManager.Instance.GetCurrentGameMode)
        {
            case GameModes.Dunk:
                DunkOnWin();
                break;
            case GameModes.Time:
                TimeOnWin();
                break;
            case GameModes.OneTouch:
                OneTouchOnWin();
                break;
            case GameModes.Endless:
                EndlessOnWin();
                break;
            case GameModes.Fall:
                FallOnWin();
                break;
            default:
                Debug.LogWarning("Game mode not implemented for win processing: " + GameManager.Instance.GetCurrentGameMode);
                break;
        }

        _coinsObtained.Clear();
    }

    private void DunkOnWin()
    {
        int level = GameManager.Instance.SetGetWorldState.GetLevel;
        int tapCount = GameManager.Instance.SetGetTapController.SetGetTapCount;
        bool hasCoins = _coinsObtained.Count > 0;
        bool withoutDeath = !GameManager.Instance.SetGetPlayer.HasDeath;
        bool isUnderTouchLimit = tapCount <= GameManager.Instance.SetGetWorldState.GetLimitTouches;

        // Guardar datos usando el nuevo sistema
        SaveAndLoadManager.SetLevelData(
            GameModes.Dunk,
            _currentWorld,
            level,
            hasCoins,
            withoutDeath,
            isUnderTouchLimit,
            true // Guardar inmediatamente
        );

        // Mostrar información en UI
        DunkLevelCanvas.Instance.SetTouchesInLevel(tapCount, !isUnderTouchLimit);

        Debug.Log($"Dunk Level {level} completed - Coins: {hasCoins}, No Death: {withoutDeath}, Under Touch Limit: {isUnderTouchLimit}");
    }

    private void TimeOnWin()
    {
        int level = GameManager.Instance.SetGetWorldState.GetLevel;
        bool hasCoins = _coinsObtained.Count > 0;
        bool withoutDeath = !GameManager.Instance.SetGetPlayer.HasDeath;

        // Para Time mode, el objetivo sería completar dentro del tiempo límite
        // Aquí necesitarías obtener el tiempo actual y compararlo con el límite
        bool underTimeLimit = true; // TODO: Implementar lógica de tiempo

        SaveAndLoadManager.SetLevelData(
            GameModes.Time,
            _currentWorld,
            level,
            hasCoins,
            withoutDeath,
            underTimeLimit,
            true
        );

        Debug.Log($"Time Level {level} completed - Coins: {hasCoins}, No Death: {withoutDeath}, Under Time Limit: {underTimeLimit}");
    }

    private void OneTouchOnWin()
    {
        int level = GameManager.Instance.SetGetWorldState.GetLevel;
        int tapCount = GameManager.Instance.SetGetTapController.SetGetTapCount;
        bool hasCoins = _coinsObtained.Count > 0;
        bool withoutDeath = !GameManager.Instance.SetGetPlayer.HasDeath;

        // Para OneTouch, el objetivo es usar exactamente 1 toque o menos del límite
        int touchLimit = GameManager.Instance.SetGetWorldState.GetLimitTouches;
        bool underTouchLimit = tapCount <= touchLimit;

        SaveAndLoadManager.SetLevelData(
            GameModes.OneTouch,
            _currentWorld,
            level,
            hasCoins,
            withoutDeath,
            underTouchLimit,
            true
        );

        Debug.Log($"OneTouch Level {level} completed - Coins: {hasCoins}, No Death: {withoutDeath}, Touches: {tapCount}/{touchLimit}");
    }

    private void EndlessOnWin()
    {
        // Endless es procedural, no tiene niveles específicos
        // Podrías guardar estadísticas como mejor puntuación, tiempo sobrevivido, etc.
        Debug.Log("Endless mode completed - No level data to save");
    }

    private void FallOnWin()
    {
        // Fall es procedural, similar a Endless
        Debug.Log("Fall mode completed - No level data to save");
    }

    public void OnLose()
    {
        OnLoseLevel?.Invoke();

        _coinsObtained.ForEach(coin => coin.OnLose());
        _coinsObtained.Clear();

        _deathCount++;
        if (_deathCount >= 10)
        {
            AdsManager.Instance.ShowInterstitialAd();
            _deathCount = 0;
        }

        // Resetear según el modo de juego
        switch (GameManager.Instance.GetCurrentGameMode)
        {
            case GameModes.Dunk:
            case GameModes.Time:
            case GameModes.OneTouch:
                GameManager.Instance.SetGetTapController.SetGetTapCount = 0;
                break;
            default:
                break;
        }

        AudioManager.Instance.PlayMusicByType(AudioManager.MusicClipType.DunkMusic);
    }

    public void OnGetCoin(Coins coinName)
    {
        _coinsObtained.Add(coinName);
    }

    public void ResetCoins()
    {
        _coinsObtained.Clear();
    }

    /// <summary>
    /// Verifica si ya se obtuvieron monedas en el nivel actual
    /// Compatibilidad con el sistema nuevo
    /// </summary>
    public bool HasGetedCoins => _coinsObtained.Count > 0 ||
        SaveAndLoadManager.GetLevelCoinObtained(
            GameManager.Instance.GetCurrentGameMode,
            _currentWorld,
            GameManager.Instance.SetGetWorldState.GetLevel
        );

    /// <summary>
    /// Obtiene los datos completos del nivel actual
    /// </summary>
    public LevelData GetCurrentLevelData()
    {
        return SaveAndLoadManager.GetLevelData(
            GameManager.Instance.GetCurrentGameMode,
            _currentWorld,
            GameManager.Instance.SetGetWorldState.GetLevel
        );
    }

    /// <summary>
    /// Verifica si el nivel actual está completado
    /// </summary>
    public bool IsCurrentLevelCompleted()
    {
        return SaveAndLoadManager.HasLevelData(
            GameManager.Instance.GetCurrentGameMode,
            _currentWorld,
            GameManager.Instance.SetGetWorldState.GetLevel
        );
    }

    /// <summary>
    /// Establece el mundo actual
    /// Usar cuando implementes el sistema de mundos
    /// </summary>
    public void SetCurrentWorld(string world)
    {
        _currentWorld = world;
        Debug.Log($"Current world set to: {world}");
    }

    /// <summary>
    /// Obtiene el mundo actual
    /// </summary>
    public string GetCurrentWorld()
    {
        return _currentWorld;
    }

    #region Migration Helper - Llamar una vez para migrar datos existentes
    [ContextMenu("Migrate Legacy Data")]
    public void MigrateLegacyData()
    {
        SaveAndLoadManager.MigrateLegacyData();
    }
    #endregion
}