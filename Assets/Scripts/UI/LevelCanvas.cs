using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityAddressables;

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
    private List<TextMeshProUGUI> _achievementTextList = new();

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

        _hasCoinGoal = FindAndValidateGameObjectComponent(transform, "CoinGoalFull");
        _emptyhasCoinGoal = FindAndValidateGameObjectComponent(transform, "CoinEmpty");
        _deathGoal = FindAndValidateGameObjectComponent(transform, "DeathGoalFull");
        _emptydeathGoal = FindAndValidateGameObjectComponent(transform, "DeathEmpty");
        _achievementsGoalPrefab = FindAndValidateGameObjectComponent(transform, "AchievementGoalPrefab");

        SetAchivementsByMode();

        UIManager.Instance.AddCanvas(gameObject, true);
        AudioManager.Instance.PlayMusicByType(AudioManager.MusicClipType.DunkMusic);

        if (LevelManager.Instance)
        {
            LevelManager.Instance.OnWinLevel += OnWin;
            LevelManager.Instance.OnLoseLevel += OnLose;
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
                    _achievementTextList.Add(FindAndValidateComponent<TextMeshProUGUI>(_achievementsGoalPrefab.transform, $"Text{i}"));
                }
            });
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance)
        {
            LevelManager.Instance.OnWinLevel -= OnWin;
            LevelManager.Instance.OnLoseLevel -= OnLose;
        }
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

        if (LevelManager.Instance.HasGetedCoins)
            StartCoroutine(ShowGoal(0.6f, _hasCoinGoal, _emptyhasCoinGoal));

        if (!GameManager.Instance.SetGetPlayer.HasDeath)
            StartCoroutine(ShowGoal(1.7f, _deathGoal, _emptydeathGoal));
    }

    public void OnLose()
    {
        UIManager.Instance.SetText(_countText, 0);
    }

    public void OnTap(int tapCount)
    {
        UIManager.Instance.SetText(_countText, tapCount);
    }

    public void ShowTimerText(float timer)
    {
        int seconds = (int)timer;
        int decimals = (int)((timer - seconds) * 100);
        UIManager.Instance.SetText(_timerText, seconds);
        UIManager.Instance.SetText(_timerDecimalsText, $".{decimals}", true);
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
        UIManager.Instance.SetText(_achievementTextList[0], $"{touchesInLevel}");
        UIManager.Instance.SetText(_achievementTextList[1], $"/{limitOfMode}");
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
        FindAndValidateGameObjectComponent(transform, "TapsCount").SetActive(gameModes == GameManager.GameModes.Dunk);
        FindAndValidateGameObjectComponent(transform, "TimeCount").SetActive(gameModes == GameManager.GameModes.Time);

        switch (gameModes)
        {
            case GameManager.GameModes.Dunk:
                _countText = FindAndValidateComponent<TextMeshProUGUI>(transform, "PointsText");
                break;
            case GameManager.GameModes.Time:
                _timerText = FindAndValidateComponent<TextMeshProUGUI>(transform, "TimeText");
                _timerDecimalsText = FindAndValidateComponent<TextMeshProUGUI>(transform, "TimeDecimalsText");
                break;
            case GameManager.GameModes.Endless:
                break;
            case GameManager.GameModes.OneTouch:
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
