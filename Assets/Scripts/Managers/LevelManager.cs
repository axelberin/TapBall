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

    // Propiedad pública para acceso desde otros scripts
    public List<Coins> CoinsObtainedInSession => _coinsObtained;

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
        SaveAndLoadManager.SetIntValue(currentCoins, SaveAndLoadManager.CoinsName, true);

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
        LevelData currentData = GetCurrentLevelData();

        int level = GameManager.Instance.SetGetWorldState.GetLevel;
        int tapCount = GameManager.Instance.SetGetTapController.SetGetTapCount;
        bool hasCoins = _coinsObtained.Count > 0 || currentData.coinObtained;
        bool withoutDeath = !GameManager.Instance.SetGetPlayer.HasDeath || !currentData.withoutDeath;
        bool isUnderTouchLimit = tapCount <= GameManager.Instance.SetGetWorldState.GetLimitTouches;
        bool isUnderTouchLimitEver = isUnderTouchLimit || currentData.objectiveComplete;

        // Guardar datos usando el nuevo sistema solo si hay cambios
        if (hasCoins != currentData.coinObtained ||
            withoutDeath != currentData.withoutDeath ||
            isUnderTouchLimitEver != currentData.objectiveComplete)
        {
            SaveAndLoadManager.SetLevelData(
                GameModes.Dunk,
                _currentWorld,
                level,
                hasCoins,
                withoutDeath,
                isUnderTouchLimitEver,
                true,
                true
            );

            Debug.Log($"Dunk Level {level} data updated - Coins: {hasCoins}, No Death: {withoutDeath}, Under Touch Limit: {isUnderTouchLimitEver}");
        }

        // Mostrar información en UI
        DunkLevelCanvas.Instance.SetAchievementByDunkMode(tapCount, !isUnderTouchLimit, !isUnderTouchLimitEver, GameManager.Instance.SetGetWorldState.GetLimitTouches);
    }

    private void TimeOnWin()
    {
        LevelData currentData = GetCurrentLevelData();

        float timeCount = GameManager.Instance.SetGetWorldState.GetLimitTime;
        float remainingTime = GameManager.Instance.SetGetWorldState.GetRemainingTime;
        int level = GameManager.Instance.SetGetWorldState.GetLevel;
        bool hasCoins = _coinsObtained.Count > 0 || currentData.coinObtained;
        bool withoutDeath = !GameManager.Instance.SetGetPlayer.HasDeath || currentData.withoutDeath;
        bool underTimeLimit = remainingTime >= (timeCount * 0.1);
        bool underTimeLimitEver = underTimeLimit || currentData.objectiveComplete;

        if (hasCoins != currentData.coinObtained ||
            withoutDeath != currentData.withoutDeath ||
            underTimeLimit != currentData.objectiveComplete)
        {
            SaveAndLoadManager.SetLevelData(
                GameModes.Time,
                _currentWorld,
                level,
                hasCoins,
                withoutDeath,
                underTimeLimitEver,
                true,
                true
            );

            Debug.Log($"Time Level {level} data updated - Coins: {hasCoins}, No Death: {withoutDeath}, Under Time Limit: {underTimeLimit}");
        }

        DunkLevelCanvas.Instance.SetAchievementByTimeMode(remainingTime,!underTimeLimit, !underTimeLimitEver, timeCount);
    }

    private void OneTouchOnWin()
    {
        LevelData currentData = GetCurrentLevelData();

        int level = GameManager.Instance.SetGetWorldState.GetLevel;
        int tapCount = GameManager.Instance.SetGetTapController.SetGetTapCount;
        bool hasCoins = _coinsObtained.Count > 0 || currentData.coinObtained;
        bool withoutDeath = !GameManager.Instance.SetGetPlayer.HasDeath || currentData.withoutDeath;

        // Para OneTouch, el objetivo es usar exactamente 1 toque o menos del límite
        bool underTouchLimit = tapCount <= GameManager.Instance.SetGetWorldState.GetLimitTouches;

        // Guardar solo si hay cambios
        if (hasCoins != currentData.coinObtained ||
            withoutDeath != currentData.withoutDeath ||
            underTouchLimit != currentData.objectiveComplete)
        {
            SaveAndLoadManager.SetLevelData(
                GameModes.OneTouch,
                _currentWorld,
                level,
                hasCoins,
                withoutDeath,
                underTouchLimit,
                true,
                true
            );

            Debug.Log($"OneTouch Level {level} data updated - Coins: {hasCoins}, No Death: {withoutDeath}, Touches: {tapCount}/{underTouchLimit}");
        }
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
                GameManager.Instance.SetGetWorldState.ResetTimer();
                break;
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
        // Solo agregar la moneda si no se había obtenido previamente
        if (!HasGetedCoins)
        {
            _coinsObtained.Add(coinName);
        }
    }

    public void ResetCoins()
    {
        _coinsObtained.Clear();
    }

    /// <summary>
    /// Verifica si ya se obtuvieron monedas en el nivel actual
    /// Compatibilidad con el sistema nuevo
    /// </summary>
    public bool HasGetedCoins => _coinsObtained.Count > 0 || SaveAndLoadManager.GetLevelCoinObtained(
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
}