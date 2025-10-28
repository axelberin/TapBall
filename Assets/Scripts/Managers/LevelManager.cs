using System;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public Action OnWinLevel = delegate { };
    public Action OnLoseLevel = delegate { };
    public Action OnPreLoseLevel = delegate { };

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
        DailyMissionsManager.OnMissionActionPerformed(MissionType.LevelsPassed, GetCurrentLevelData().levelCompleted ? 0 : 1);

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
                DailyMissionsManager.OnMissionActionPerformed(MissionType.TimeLimit, GameManager.Instance.SetGetWorldState.GetElapsedTime);
                TimeOnWin();
                break;
            case GameModes.OneTouch:
                DailyMissionsManager.OnMissionActionPerformed(MissionType.TouchesRemaining, GameManager.Instance.SetGetTapController.SetGetTapCount);
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
        bool withoutDeath = !GameManager.Instance.SetGetPlayer.HasDeath || currentData.withoutDeath;
        bool isUnderTouchLimit = tapCount <= GameManager.Instance.SetGetWorldState.GetLimitTouches;
        bool isUnderTouchLimitEver = isUnderTouchLimit || currentData.objectiveComplete;

        SaveAndLoadManager.CompleteLevel(
            GameModes.Dunk,
            _currentWorld,
            level,
            hasCoins,
            withoutDeath,
            isUnderTouchLimitEver,
            true,
            true
        );

        Debug.Log($"Dunk Level {level} completed - Coins: {hasCoins}, No Death: {withoutDeath}, Under Touch Limit: {isUnderTouchLimitEver}");

        // Mostrar información en UI
        LevelCanvas.Instance.SetAchievementByDunkMode(tapCount, !isUnderTouchLimit,
            !isUnderTouchLimitEver, GameManager.Instance.SetGetWorldState.GetLimitTouches);
    }

    private void TimeOnWin()
    {
        GameManager.Instance.SetGetWorldState.StopCountTimerMode();
        AudioManager.Instance.StopSound(false, true);
        LevelData currentData = GetCurrentLevelData();

        float remainingTime = GameManager.Instance.SetGetWorldState.GetRemainingTime;
        int level = GameManager.Instance.SetGetWorldState.GetLevel;
        bool hasCoins = _coinsObtained.Count > 0 || currentData.coinObtained;
        bool withoutDeath = !GameManager.Instance.SetGetPlayer.HasDeath || currentData.withoutDeath;
        float limitTime = GameManager.Instance.SetGetWorldState.GetLimitTime * 0.2f;
        bool underTimeLimit = remainingTime >= limitTime;
        bool underTimeLimitEver = underTimeLimit || currentData.objectiveComplete;

        SaveAndLoadManager.CompleteLevel(
            GameModes.Time,
            _currentWorld,
            level,
            hasCoins,
            withoutDeath,
            underTimeLimitEver,
            true,
            true
        );

        Debug.Log($"Time Level {level} completed - Coins: {hasCoins}, No Death: {withoutDeath}, Under Time Limit: {underTimeLimit}");

        LevelCanvas.Instance.SetAchievementByTimeMode(remainingTime, !underTimeLimit, !underTimeLimitEver, limitTime);
    }

    private void OneTouchOnWin()
    {
        LevelData currentData = GetCurrentLevelData();

        int level = GameManager.Instance.SetGetWorldState.GetLevel;
        int tapCount = GameManager.Instance.SetGetTapController.SetGetTapCount;
        bool hasCoins = _coinsObtained.Count > 0 || currentData.coinObtained;
        bool withoutDeath = !GameManager.Instance.SetGetPlayer.HasDeath || currentData.withoutDeath;

        int levelTouchesPercentage = (int)MathF.Max(1, GameManager.Instance.SetGetWorldState.GetLimitTapsOneTouch * 0.18f);
        bool isUnderTouchLimit = tapCount >= levelTouchesPercentage;
        bool isUnderTouchLimitEver = isUnderTouchLimit || currentData.objectiveComplete;

        SaveAndLoadManager.CompleteLevel(
            GameModes.OneTouch,
            _currentWorld,
            level,
            hasCoins,
            withoutDeath,
            isUnderTouchLimitEver,
            true,
            true
        );

        Debug.Log($"OneTouch Level {level} completed - Coins: {hasCoins}, No Death: {withoutDeath}, Touches: {tapCount}/{isUnderTouchLimit}");

        LevelCanvas.Instance.SetAchievementByOneTouchMode(tapCount, !isUnderTouchLimit, !isUnderTouchLimitEver,
            levelTouchesPercentage);
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
                GameManager.Instance.SetGetTapController.SetGetTapCount = 0;
                break;
            case GameModes.Time:
                GameManager.Instance.SetGetWorldState.ResetTimer();
                break;
            case GameModes.OneTouch:
                GameManager.Instance.SetGetTapController.SetGetTapCount =
                    GameManager.Instance.SetGetWorldState.GetLimitTapsOneTouch;
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
            DailyMissionsManager.OnMissionActionPerformed(MissionType.CoinsCollected, 1);
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
}