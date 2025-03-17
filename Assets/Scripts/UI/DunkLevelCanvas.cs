using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DunkLevelCanvas : CanvasElementLocator
{
    public static DunkLevelCanvas Instance;

    private TextMeshProUGUI _tapCountText;
    private TextMeshProUGUI _winTime;
    private GameObject _winUI;
    private Button _menuButton;
    private Button _nextLevelButton;
    private Button _pauseButton;
    private Button _menuPauseButton;
    private Button _restartButton;
    private Button _restartPauseButton;
    private Button _resumeButton;
    private GameObject _pauseUI;
    private TextMeshProUGUI _touchesInLevelText;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
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
            UIManager.Instance.ClearCnavasesList();
            ScenesManager.Instance.LoadSceneAsync("Menu", fadeAnimator);
        });

        _nextLevelButton = FindAndValidateButtonComponent(transform, "NextLevelBTN");
        _nextLevelButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ClearCnavasesList();

            ScenesManager.Instance.LoadLevelByType(GameManager.Instance.SetGetWorldState.GetLevel + 1,
                GameManager.Instance.GetCurrentGameMode, fadeAnimator);
            AdsManager.Instance.LoadInterstitialAd();
        });

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
            UIManager.Instance.ClearCnavasesList();
            ScenesManager.Instance.LoadLevelByType(GameManager.Instance.SetGetWorldState.GetLevel,
                                                        GameManager.Instance.GetCurrentGameMode, fadeAnimator);
        });

        _resumeButton = FindAndValidateButtonComponent(transform, "ResumeBTN");
        _resumeButton.onClick.AddListener(OnResumeClicked);

        var configsButton = FindAndValidateButtonComponent(transform, "ConfigsButton");
        configsButton.onClick.AddListener(() =>
            UIManager.Instance.ChangeCanvas("DunkCanvas", "ConfigsCanvas"));

        _touchesInLevelText = FindAndValidateTextComponent(transform, "TouchesText");
        var limitTouchesText = FindAndValidateTextComponent(transform, "LimitTouchesText");
        limitTouchesText.text = "/ " + GameManager.Instance.SetGetWorldState.GetLimitTouches.ToString();

        UIManager.Instance.AddCanvas(gameObject, true);

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
        _nextLevelButton.interactable = ScenesManager.Instance.IsSceneExisting(
            $"DunkLevel{GameManager.Instance.SetGetWorldState.GetLevel + 1}");

        UIManager.Instance.ActivateUI(_winTime.gameObject, false);
        UIManager.Instance.ActivateUI(_winUI, true);
        _pauseButton.interactable = false;
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
        UIManager.Instance.SetText(_winTime, (int)(time));
    }

    public void SetTouchesInLevel(int touchesInLevel, bool isOverLimit)
    {
        UIManager.Instance.SetText(_touchesInLevelText, touchesInLevel);
        if (isOverLimit)
            _touchesInLevelText.color = Color.red;
        else
            _touchesInLevelText.color = Color.green;
    }
}
