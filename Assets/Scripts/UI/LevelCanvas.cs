using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityAddressables;
using static PowerUpManager;

public class LevelCanvas : CanvasElementLocator
{
    public static LevelCanvas Instance;

    private TextMeshProUGUI _countText;
    private TextMeshProUGUI _timerText;
    private TextMeshProUGUI _timerDecimalsText;
    private TextMeshProUGUI _winTime;
    private GameObject _winUI;
    private GameObject _hasCoinGoal;
    private GameObject _emptyhasCoinGoal;
    private GameObject _deathGoal;
    private GameObject _emptydeathGoal;
    private GameObject _achievementsGoalPrefab;
    private GameObject _emptyAchievementsGoal;
    private GameObject _fullAchievementsGoal;
    private Button _nextLevelButton;
    private Button _pauseButton;
    private GameObject _pauseUI;
    private TextMeshProUGUI _nextLevelText;
    private TextMeshProUGUI _currentLevelText;
    private List<TextMeshProUGUI> _achievementTextList = new();
    private Button _stopTimePowUpButton;
    private Button _stopTouchCountPowUpButton;
    private Button _immunityPowUpButton;
    private Button _revivePowUpButton;
    private TextMeshProUGUI _stopTimePowUpText;
    private TextMeshProUGUI _stopTouchCountPowUpText;
    private TextMeshProUGUI _immunityPowUpText;
    private TextMeshProUGUI _revivePowUpText;
    private GameObject _revivePowerUpUI;    // Hacer PopUp
    private Image _useRevivePoweUpToReviveImage;
    private Image _useOrbsToReviveImage;
    private Image _viewVideoToReviveImage;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        PauseAndResumeManager.Instance.AddPauseAction(AudioManager.Instance.OnPause);
        PauseAndResumeManager.Instance.AddResumeAction(AudioManager.Instance.OnResume);

        SetCounterByGameMode(GameManager.Instance.GetCurrentGameMode);
        _winTime = FindAndValidateComponent<TextMeshProUGUI>(transform, "WinTime");
        _winUI = FindAndValidateGameObjectComponent(transform, "WinUI");
        _revivePowerUpUI = FindAndValidateGameObjectComponent(transform, "RevivePowerUpUI");

        var fadeAnimator = FindAndValidateGameObjectComponent(transform, "Fade").GetComponent<Animator>();

        var menuButton = FindAndValidateComponent<Button>(transform, "MenuBTN");
        menuButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ClearCnavasesList();
            ScenesManager.Instance.LoadSceneAsync("Menu", fadeAnimator);
        });

        var menuPauseButton = FindAndValidateComponent<Button>(transform, "PauseMenuBTN");
        menuPauseButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.StopMusic();
            UIManager.Instance.ClearCnavasesList();
            ScenesManager.Instance.LoadSceneAsync("Menu", fadeAnimator);
            AudioManager.Instance.StopSound();
        });

        _nextLevelButton = FindAndValidateComponent<Button>(transform, "NextLevelBTN");
        _nextLevelButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ClearCnavasesList();

            ScenesManager.Instance.LoadLevelByType(GameManager.Instance.SetGetWorldState.GetLevel + 1,
                GameManager.Instance.GetCurrentGameMode, fadeAnimator);
        });
        _nextLevelText = FindAndValidateComponent<TextMeshProUGUI>(_nextLevelButton.transform, "NextLevelText");

        _pauseButton = FindAndValidateComponent<Button>(transform, "PauseButton");
        _pauseButton.onClick.AddListener(OnPauseClicked);

        _pauseUI = FindAndValidateGameObjectComponent(transform, "PauseUI");
        _pauseUI.SetActive(false);

        var restartButton = FindAndValidateComponent<Button>(transform, "RestartBTN");
        restartButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ClearCnavasesList();
            ScenesManager.Instance.LoadLevelByType(GameManager.Instance.SetGetWorldState.GetLevel,
                                                        GameManager.Instance.GetCurrentGameMode, fadeAnimator);
        });

        var restartPauseButton = FindAndValidateComponent<Button>(transform, "PauseRestartBTN");
        restartPauseButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.StopMusic();
            UIManager.Instance.ClearCnavasesList();
            ScenesManager.Instance.LoadLevelByType(GameManager.Instance.SetGetWorldState.GetLevel,
                                                        GameManager.Instance.GetCurrentGameMode, fadeAnimator);
            AudioManager.Instance.StopSound();
        });

        var resumeButton = FindAndValidateComponent<Button>(transform, "ResumeBTN");
        resumeButton.onClick.AddListener(OnResumeClicked);

        var configsButton = FindAndValidateComponent<Button>(transform, "ConfigsButton");
        configsButton.onClick.AddListener(() =>
            UIManager.Instance.ChangeCanvas("DunkCanvas", "ConfigsCanvas"));


        _stopTimePowUpButton = FindAndValidateComponent<Button>(transform, "StopTimePowerButton");
        _stopTouchCountPowUpButton = FindAndValidateComponent<Button>(transform, "StopTouchCountPowerButton");
        _immunityPowUpButton = FindAndValidateComponent<Button>(transform, "ImmunityPowerButton");

        #region REVIVE POWER UP UI
        // Este va a aser el power up de arriba en la UI
        _revivePowUpButton = FindAndValidateComponent<Button>(transform, "RevivePowerButton");
        //Estas son las imagenes que estan dentro del botón de Ok del pop up
        _useRevivePoweUpToReviveImage = FindAndValidateComponent<Image>(_revivePowerUpUI.transform, "RevivePowerUpImage");
        _useOrbsToReviveImage = FindAndValidateComponent<Image>(_revivePowerUpUI.transform, "OrbImage");
        _viewVideoToReviveImage = FindAndValidateComponent<Image>(_revivePowerUpUI.transform, "AdsImage");
        _revivePowUpText = FindAndValidateComponent<TextMeshProUGUI>(transform, "RevivePowerText");
        #endregion

        _stopTimePowUpText = FindAndValidateComponent<TextMeshProUGUI>(transform, "StopTimePowerText");
        _stopTouchCountPowUpText = FindAndValidateComponent<TextMeshProUGUI>(transform, "StopTouchCountPowerText");
        _immunityPowUpText = FindAndValidateComponent<TextMeshProUGUI>(transform, "ImmunityPowerText");

        _stopTimePowUpButton.interactable = PowerUpManager.Instance.HasPowerUp(PowerUpType.TimeStopPowerUp)
            && GameManager.Instance.GetCurrentGameMode == GameManager.GameModes.Time;

        _stopTouchCountPowUpButton.interactable = PowerUpManager.Instance.HasPowerUp(PowerUpType.StopTouchCounterPowerUp)
            && GameManager.Instance.GetCurrentGameMode != GameManager.GameModes.Time;
        _immunityPowUpButton.interactable = PowerUpManager.Instance.HasPowerUp(PowerUpType.ImmunityPowerUp);

        #region REVIVE POWERUP UI INTERACTUABLE BUTTONS
        _revivePowUpButton.interactable = PowerUpManager.Instance.HasPowerUp(PowerUpType.RevivePowerUp);
        _useOrbsToReviveImage.gameObject.SetActive(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.OrbsName) > 0);

        //Lo que hacen estos botones va a tener que pasar en un solo botón, el OK del pop up, y va a variar en base al recurso disponible.
        //_revivePowUpButton.onClick.AddListener(() =>
        //{
        //    PowerUpManager.Instance.SelectPowerUp(PowerUpType.RevivePowerUp);

        //    SaveAndLoadManager.SetIntValue(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.PowerUpPrefix +
        //       PowerUpType.RevivePowerUp.ToString()) - 1, SaveAndLoadManager.PowerUpPrefix + PowerUpType.RevivePowerUp, true, true);

        //    UpdatePowerUpTexts(PowerUpType.RevivePowerUp);
        //});
        //_useOrbsToReviveImage.onClick.AddListener(() =>
        //{
        //    PowerUpManager.Instance.SelectPowerUp(PowerUpType.RevivePowerUp);

        //    SaveAndLoadManager.SetIntValue(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.OrbsName)
        //               - 10, SaveAndLoadManager.OrbsName, true, true);

        //    UpdatePowerUpTexts(PowerUpType.RevivePowerUp);
        //});
        //_viewVideoToReviveImage.onClick.AddListener(() => Debug.Log("Estás viendo un vidio para revivir........."));
        #endregion

        _stopTimePowUpButton.onClick.AddListener(() =>
        {
            PowerUpManager.Instance.SelectPowerUp(PowerUpType.TimeStopPowerUp);

            SaveAndLoadManager.SetIntValue(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.PowerUpPrefix +
                PowerUpType.TimeStopPowerUp.ToString()) - 1, SaveAndLoadManager.PowerUpPrefix + PowerUpType.TimeStopPowerUp, true, true);

            UpdatePowerUpTexts(PowerUpType.TimeStopPowerUp);
        });
        _stopTouchCountPowUpButton.onClick.AddListener(() =>
        {
            PowerUpManager.Instance.SelectPowerUp(PowerUpType.StopTouchCounterPowerUp);

            SaveAndLoadManager.SetIntValue(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.PowerUpPrefix +
                PowerUpType.StopTouchCounterPowerUp.ToString()) - 1, SaveAndLoadManager.PowerUpPrefix + PowerUpType.StopTouchCounterPowerUp, true, true);

            UpdatePowerUpTexts(PowerUpType.StopTouchCounterPowerUp);
        });
        _immunityPowUpButton.onClick.AddListener(() =>
        {
            PowerUpManager.Instance.SelectPowerUp(PowerUpType.ImmunityPowerUp);

            SaveAndLoadManager.SetIntValue(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.PowerUpPrefix +
                PowerUpType.ImmunityPowerUp.ToString()) - 1, SaveAndLoadManager.PowerUpPrefix + PowerUpType.ImmunityPowerUp, true, true);

            UpdatePowerUpTexts(PowerUpType.ImmunityPowerUp);
        });

        _currentLevelText = FindAndValidateComponent<TextMeshProUGUI>(transform, "CurrentLevelText");

        _hasCoinGoal = FindAndValidateGameObjectComponent(transform, "CoinGoalFull");
        _emptyhasCoinGoal = FindAndValidateGameObjectComponent(transform, "CoinEmpty");
        _deathGoal = FindAndValidateGameObjectComponent(transform, "DeathGoalFull");
        _emptydeathGoal = FindAndValidateGameObjectComponent(transform, "DeathEmpty");
        _achievementsGoalPrefab = FindAndValidateGameObjectComponent(transform, "AchievementGoalPrefab");


        StartCoroutine(UpdateTextsDelay());
        SetAchivementsByMode();

        UIManager.Instance.AddCanvas(gameObject, true);
        AudioManager.Instance.PlayMusicByType(AudioManager.MusicClipType.DunkMusic);

        if (LevelManager.Instance)
        {
            LevelManager.Instance.OnWinLevel += OnWin;
            LevelManager.Instance.OnLoseLevel += OnLose;
            LevelManager.Instance.OnPreLoseLevel += OnPreLose;
        }
    }

    private void SetAchivementsByMode()
    {
        RectTransform goalPrefabRectTransform = FindAndValidateComponent<RectTransform>(transform, "AchievementGoalPrefab");

        AddressablesUtility.LoadAsset<GameObject>(
            $"{GameManager.Instance.GetCurrentGameMode}AchievementUI", imageGo =>
            {
                _achievementsGoalPrefab.SetActive(false);

                _achievementsGoalPrefab = Instantiate(imageGo, _achievementsGoalPrefab.transform.parent, false);
                _achievementsGoalPrefab.name = "AchievementGoalPrefab";

                RectTransform newRectTransform = _achievementsGoalPrefab.GetComponent<RectTransform>();
                newRectTransform.anchoredPosition = goalPrefabRectTransform.anchoredPosition;
                newRectTransform.sizeDelta = goalPrefabRectTransform.sizeDelta;

                _emptyAchievementsGoal = FindAndValidateGameObjectComponent(_achievementsGoalPrefab.transform, "EmptyAchievement");
                _fullAchievementsGoal = FindAndValidateGameObjectComponent(_achievementsGoalPrefab.transform, "FullAchievement");

                for (int i = 0; i < _achievementsGoalPrefab.transform.childCount; i++)
                {
                    _achievementTextList.Add(FindAndValidateComponent<TextMeshProUGUI>(
                        _achievementsGoalPrefab.transform, $"Text{i}", false));
                }
            });
    }

    private void OnEnable()
    {
        UpdateTexts();
        if (PowerUpManager.Instance)
            PowerUpManager.Instance.OnPowerUpActivated += UpdatePowerUpTexts;
    }

    private void OnDisable()
    {
        if (PowerUpManager.Instance)
            PowerUpManager.Instance.OnPowerUpActivated -= UpdatePowerUpTexts;
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance)
        {
            LevelManager.Instance.OnWinLevel -= OnWin;
            LevelManager.Instance.OnLoseLevel -= OnLose;
            LevelManager.Instance.OnPreLoseLevel -= OnPreLose;
        }
    }

    private IEnumerator UpdateTextsDelay()
    {
        yield return new WaitForSeconds(0.1f);
        UpdateTexts();
    }

    private void UpdateTexts()
    {
        if (_currentLevelText != null)
            _currentLevelText.text =
                $"{LanguageManager.Instance.GetLocalizedText("level")} {GameManager.Instance.SetGetWorldState.GetLevel}";
    }

    public void ActivateAndDeactivateRevivePowerUI()
    {
        // Definir lógica del botón de OK en base al recurso disponible.
        // Activar la imagen del botón de Ok que corresponda según el recurso disponible.

        // Para que muestre un anuncio es así.
        //AdsManager.Instance.ShowInterstitialAd(Pasarle la funcion de AcceptRevive al finalizar el anuncio);

        // Activar pop up.
    }

    private void UpdatePowerUpTexts(PowerUpType powerUp)
    {
        switch (powerUp)
        {
            case PowerUpType.TimeStopPowerUp:
                SetPowerUpTexts(_stopTimePowUpText, powerUp);
                break;
            case PowerUpType.StopTouchCounterPowerUp:
                SetPowerUpTexts(_stopTouchCountPowUpText, powerUp);
                break;
            case PowerUpType.ImmunityPowerUp:
                SetPowerUpTexts(_immunityPowUpText, powerUp);
                break;
            case PowerUpType.RevivePowerUp:
                SetPowerUpTexts(_revivePowUpText, powerUp);
                break;
        }
    }

    private void SetPowerUpTexts(TextMeshProUGUI text, PowerUpType powUpType)
    {
        UIManager.Instance.SetText(text, $"x{SaveAndLoadManager.GetIntValue(SaveAndLoadManager.PowerUpPrefix + powUpType.ToString())}");
    }

    private void OnResumeClicked()
    {
        _pauseUI.SetActive(false);
        PauseAndResumeManager.Instance.InvokeResume();
    }

    private void OnPauseClicked()
    {
        _pauseUI.SetActive(true);
        PauseAndResumeManager.Instance.InvokePause();
    }

    public void OnWin()
    {
        var existsNextLevel = ScenesManager.Instance.IsSceneExisting(
            $"{SaveAndLoadManager.GetStringValue(SaveAndLoadManager.CurrentWorldName)}" +
            $"Level{GameManager.Instance.SetGetWorldState.GetLevel + 1}");

        _nextLevelButton.interactable = existsNextLevel;
        if (!existsNextLevel)
        {
            _nextLevelText.gameObject.SetActive(false);
            GameManager.Instance.OnCompleteWorld(
                SaveAndLoadManager.GetStringValue(SaveAndLoadManager.CurrentWorldName) + "BallSkin");
        }
        else
        {
            UIManager.Instance.SetText(_nextLevelText, LanguageManager.Instance.GetLocalizedText("nextLevel"));
        }

        UIManager.Instance.ActivateUI(_winTime.gameObject, false);
        UIManager.Instance.ActivateUI(_winUI, true);
        _pauseButton.interactable = false;

        GameManager.Instance.UnlockMode(GameManager.Instance.SetGetWorldState.GetLevel);

        if (LevelManager.Instance.HasGetedCoins)
            StartCoroutine(ShowGoal(0.6f, _hasCoinGoal, _emptyhasCoinGoal));

        if (!GameManager.Instance.SetGetPlayer.HasDeath)
            StartCoroutine(ShowGoal(1.7f, _deathGoal, _emptydeathGoal));
    }

    public void OnLose()
    {
        switch (GameManager.Instance.GetCurrentGameMode)
        {
            case GameManager.GameModes.Dunk:
                UIManager.Instance.SetText(_countText, 0);
                break;
            case GameManager.GameModes.OneTouch:
                UIManager.Instance.SetText(_countText,
                    GameManager.Instance.SetGetWorldState.GetLimitTapsOneTouch);
                break;
            case GameManager.GameModes.Time:
                ShowTimerText(GameManager.Instance.SetGetWorldState.GetLimitTime);
                break;
        }

        _pauseButton.interactable = true;
    }

    private void OnPreLose()
    {
        _pauseButton.interactable = false;
    }

    public void OnTap(int tapCount)
    {
        UIManager.Instance.SetText(_countText, tapCount);
    }

    public void ShowTimerText(float timer)
    {
        float formattedTime = Mathf.Round(timer * 100f) / 100f;

        int seconds = (int)formattedTime;
        int decimals = Mathf.RoundToInt((formattedTime - seconds) * 100f);

        decimals = Mathf.Clamp(decimals, 0, 99);

        UIManager.Instance.SetText(_timerText, seconds);
        UIManager.Instance.SetText(_timerDecimalsText, $".{decimals:00}", true);
    }

    public void OnExitWinBase()
    {
        UIManager.Instance.ActivateUI(_winTime.gameObject, false);
    }

    public void OnCountTime(float time)
    {
        UIManager.Instance.SetText(_winTime, (int)time);
    }

    public void SetAchievementByDunkMode(int touchesInLevel, bool isOverLimit, bool isOverLimitEver, int limitOfMode)
    {
        UIManager.Instance.SetText(_achievementTextList[0], $"{touchesInLevel}", true);
        UIManager.Instance.SetText(_achievementTextList[1], $"/{limitOfMode}", true);
        if (isOverLimitEver)
            _achievementTextList[0].color = Color.red;
        else
        {
            StartCoroutine(ShowGoal(2.8f, _fullAchievementsGoal, _emptyAchievementsGoal));
            _achievementTextList[0].color = isOverLimit ? Color.red : Color.green;
        }
    }

    public void SetAchievementByTimeMode(float timeInLevel, bool isOverLimit, bool isOverLimitEver, float limitOfMode)
    {
        UIManager.Instance.SetText(_achievementTextList[0], $"{timeInLevel}s", true);
        UIManager.Instance.SetText(_achievementTextList[1], $"/{limitOfMode}s", true);
        if (isOverLimitEver)
            _achievementTextList[0].color = Color.red;
        else
        {
            StartCoroutine(ShowGoal(2.8f, _fullAchievementsGoal, _emptyAchievementsGoal));
            _achievementTextList[0].color = isOverLimit ? Color.red : Color.green;
        }
    }
    public void SetAchievementByOneTouchMode(float remainingTouchesInLevel, bool isOverLimit, bool isOverLimitEver, float limitOfMode)
    {
        UIManager.Instance.SetText(_achievementTextList[0], $"{remainingTouchesInLevel}", true);
        UIManager.Instance.SetText(_achievementTextList[1], $"/{limitOfMode}", true);
        if (isOverLimitEver)
            _achievementTextList[0].color = Color.red;
        else
        {
            StartCoroutine(ShowGoal(2.8f, _fullAchievementsGoal, _emptyAchievementsGoal));
            _achievementTextList[0].color = isOverLimit ? Color.red : Color.green;
        }
    }

    private IEnumerator ShowGoal(float time, GameObject goalObject, GameObject emptyGoal)
    {
        yield return new WaitForSeconds(time);
        UIManager.Instance.ActivateUI(goalObject, true);
        AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.AchivmentSound);
        yield return new WaitForSeconds(1f);
        UIManager.Instance.ActivateUI(emptyGoal, false);
    }

    private void SetCounterByGameMode(GameManager.GameModes gameModes)
    {
        FindAndValidateGameObjectComponent(transform, "TapsCount").SetActive(gameModes == GameManager.GameModes.Dunk || gameModes == GameManager.GameModes.OneTouch);
        FindAndValidateGameObjectComponent(transform, "TimeCount").SetActive(gameModes == GameManager.GameModes.Time);


        switch (gameModes)
        {
            case GameManager.GameModes.Dunk:
                _countText = FindAndValidateComponent<TextMeshProUGUI>(transform, "PointsText");
                break;
            case GameManager.GameModes.Time:
                _timerText = FindAndValidateComponent<TextMeshProUGUI>(transform, "TimeText");
                _timerDecimalsText = FindAndValidateComponent<TextMeshProUGUI>(transform, "TimeDecimalsText");
                ShowTimerText(GameManager.Instance.SetGetWorldState.GetRemainingTime);
                break;
            case GameManager.GameModes.Endless:
                break;
            case GameManager.GameModes.OneTouch:
                _countText = FindAndValidateComponent<TextMeshProUGUI>(transform, "PointsText");
                UIManager.Instance.SetText(_countText, GameManager.Instance.SetGetWorldState.GetLimitTapsOneTouch);
                break;
            case GameManager.GameModes.Fall:
                break;
            case GameManager.GameModes.Null:
            default:
                Debug.LogError("Game mode not found");
                break;
        }
    }
}
