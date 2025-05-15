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
    private Button _menuButton;
    private Button _nextLevelButton;
    private Button _pauseButton;
    private Button _menuPauseButton;
    private Button _restartButton;
    private Button _restartPauseButton;
    private Button _resumeButton;
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

        _tapCountText = FindAndValidateTextComponent(transform, "PointsText");
        _winTime = FindAndValidateTextComponent(transform, "WinTime");
        _winUI = FindAndValidateGameObjectComponent(transform, "WinUI");

        var fadeAnimator = FindAndValidateGameObjectComponent(transform, "Fade").GetComponent<Animator>();

        _menuButton = FindAndValidateButtonComponent(transform, "MenuBTN");
        _menuButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ClearCnavasesList();
            ScenesManager.Instance.LoadSceneAsync("Menu", fadeAnimator);
        });

        _menuPauseButton = FindAndValidateButtonComponent(transform, "PauseMenuBTN");
        _menuPauseButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.StopMusic();
            UIManager.Instance.ClearCnavasesList();
            ScenesManager.Instance.LoadSceneAsync("Menu", fadeAnimator);
            AudioManager.Instance.StopSound();
        });

        _nextLevelButton = FindAndValidateButtonComponent(transform, "NextLevelBTN");
        _nextLevelButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ClearCnavasesList();

            ScenesManager.Instance.LoadLevelByType(GameManager.Instance.SetGetWorldState.GetLevel + 1,
                GameManager.Instance.GetCurrentGameMode, fadeAnimator);
            AdsManager.Instance.ShowInterstitialAd();
        });
        _nextLevelText = FindAndValidateTextComponent(_nextLevelButton.transform, "NextLevelText");

        _pauseButton = FindAndValidateButtonComponent(transform, "PauseButton");
        _pauseButton.onClick.AddListener(OnPauseClicked);

        _pauseUI = FindAndValidateGameObjectComponent(transform, "PauseUI");
        _pauseUI.SetActive(false);

        _restartButton = FindAndValidateButtonComponent(transform, "RestartBTN");
        _restartButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ClearCnavasesList();
            ScenesManager.Instance.LoadLevelByType(GameManager.Instance.SetGetWorldState.GetLevel,
                                                        GameManager.Instance.GetCurrentGameMode, fadeAnimator);
        });

        _restartPauseButton = FindAndValidateButtonComponent(transform, "PauseRestartBTN");
        _restartPauseButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.StopMusic();
            UIManager.Instance.ClearCnavasesList();
            ScenesManager.Instance.LoadLevelByType(GameManager.Instance.SetGetWorldState.GetLevel,
                                                        GameManager.Instance.GetCurrentGameMode, fadeAnimator);
            AudioManager.Instance.StopSound();
        });

        _resumeButton = FindAndValidateButtonComponent(transform, "ResumeBTN");
        _resumeButton.onClick.AddListener(OnResumeClicked);

        var configsButton = FindAndValidateButtonComponent(transform, "ConfigsButton");
        configsButton.onClick.AddListener(() =>
            UIManager.Instance.ChangeCanvas("DunkCanvas", "ConfigsCanvas"));

        _hasCoinGoal = FindAndValidateGameObjectComponent(transform, "CoinGoalFull");
        _emptyhasCoinGoal = FindAndValidateGameObjectComponent(transform, "CoinEmpty");
        _deathGoal = FindAndValidateGameObjectComponent(transform, "DeathGoalFull");
        _emptydeathGoal = FindAndValidateGameObjectComponent(transform, "DeathEmpty");
        _touchesGoal = FindAndValidateGameObjectComponent(transform, "ToachesGoalFull");
        _emptytouchesGoal = FindAndValidateGameObjectComponent(transform, "TouchEmpty");

        _touchesInLevelText = FindAndValidateTextComponent(transform, "TouchesText");
        _limitTouchesText = FindAndValidateTextComponent(transform, "LimitTouchesText");

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
            $"DunkLevel{GameManager.Instance.SetGetWorldState.GetLevel + 1}");

        _nextLevelButton.interactable = existsNextLevel;
        if (!existsNextLevel)
            _nextLevelText.text = LanguageManager.Instance.GetLocalizedText("comingSoon");
        else
            _nextLevelText.text = LanguageManager.Instance.GetLocalizedText("nextLevel");

        _nextLevelText.font = LanguageManager.Instance.GetFontByLanguage();

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

    public void SetTouchesInLevel(int touchesInLevel, bool isOverLimit)
    {
        UIManager.Instance.SetText(_touchesInLevelText, touchesInLevel);
        UIManager.Instance.SetText(_limitTouchesText, "/ " +
            GameManager.Instance.SetGetWorldState.GetLimitTouches);
        if (isOverLimit)
            _touchesInLevelText.color = Color.red;
        else
        {
            StartCoroutine(ShowGoal(2.8f, _touchesGoal, _emptytouchesGoal));
            _touchesInLevelText.color = Color.green;
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
