using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DunkLevelCanvas : CanvasElementLocator
{
    public static DunkLevelCanvas Instance;

    private TextMeshProUGUI _tapCountText;
    private TextMeshProUGUI _winTime;
    private GameObject _winUI;
    private GameObject _hasCoinGoal;
    private GameObject _emptyhasCoinGoal;
    private GameObject _deathGoal;
    private GameObject _emptydeathGoal;
    private GameObject _touchesGoal;
    private GameObject _emptytouchesGoal;
    private Button _nextLevelButton;
    private Button _pauseButton;
    private GameObject _pauseUI;
    private TextMeshProUGUI _touchesInLevelText;
    private TextMeshProUGUI _limitTouchesText;
    private TextMeshProUGUI _nextLevelText;

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

        _tapCountText = FindAndValidateComponent<TextMeshProUGUI>(transform, "PointsText");
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
        _touchesGoal = FindAndValidateGameObjectComponent(transform, "ToachesGoalFull");
        _emptytouchesGoal = FindAndValidateGameObjectComponent(transform, "TouchEmpty");

        _touchesInLevelText = FindAndValidateComponent<TextMeshProUGUI>(transform, "TouchesText");
        _limitTouchesText = FindAndValidateComponent<TextMeshProUGUI>(transform, "LimitTouchesText");

        UIManager.Instance.AddCanvas(gameObject, true);
        AudioManager.Instance.PlayMusicByType(AudioManager.MusicClipType.DunkMusic);

        if (LevelManager.Instance)
        {
            LevelManager.Instance.OnWinLevel += OnWin;
            LevelManager.Instance.OnLoseLevel += OnLose;
        }
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
        UIManager.Instance.SetText(_tapCountText, 0);
    }

    public void OnTap(int tapCount)
    {
        UIManager.Instance.SetText(_tapCountText, tapCount);
    }

    public void OnExitWinBase()
    {
        UIManager.Instance.ActivateUI(_winTime.gameObject, false);
    }

    public void OnCountTime(float time)
    {
        UIManager.Instance.SetText(_winTime, (int)time);
    }

    public void SetTouchesInLevel(int touchesInLevel, bool isOverLimit, bool isOverLimitEver)
    {
        UIManager.Instance.SetText(_touchesInLevelText, touchesInLevel);
        UIManager.Instance.SetText(_limitTouchesText, "/ " +
            GameManager.Instance.SetGetWorldState.GetLimitTouches);
        if (isOverLimitEver)
            _touchesInLevelText.color = Color.red;
        else
        {
            StartCoroutine(ShowGoal(2.8f, _touchesGoal, _emptytouchesGoal));
            _touchesInLevelText.color = isOverLimit ? Color.red : Color.green;
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
}
