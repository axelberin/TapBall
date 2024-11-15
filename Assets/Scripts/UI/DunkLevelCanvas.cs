using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DunkLevelCanvas : ACanvas
{
    public static DunkLevelCanvas Instance;

    private TextMeshProUGUI _tapCountText;
    private TextMeshProUGUI _winTime;
    private TextMeshProUGUI _winText;
    private Button _menuButton;
    private Button _nextLevelButton;
    private Button _pauseButton;
    private Button _menuPauseButton;
    private Button _restartButton;
    private Button _resumeButton;
    private GameObject _pauseUI;

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
        _winText = FindAndValidateTextComponent(transform, "WinUI");

        _menuButton = FindAndValidateButtonComponent(transform, "MenuBTN");
        _menuButton.onClick.AddListener(() => ScenesManager.Instance.LoadScene("Menu"));
        _menuPauseButton = FindAndValidateButtonComponent(transform, "PauseMenuBTN");
        _menuPauseButton.onClick.AddListener(() => ScenesManager.Instance.LoadScene("Menu"));

        _nextLevelButton = FindAndValidateButtonComponent(transform, "NextLevelBTN");

        if (_nextLevelButton.interactable)
            _nextLevelButton.onClick.AddListener(() =>
            {
                ScenesManager.Instance.LoadLevelByType(GameManager.Instance.SetGetWorldState.GetLevel + 1,
                    GameManager.Instance.GetCurrentGameMode);
                AdsManager.Instance.LoadInterstitialAd();
            });

        _pauseButton = FindAndValidateButtonComponent(transform, "PauseButton");
        _pauseButton.onClick.AddListener(OnPauseClicked);

        _pauseUI = FindAndValidateGameObjectComponent(transform, "PauseUI");
        _pauseUI.SetActive(false);

        _restartButton = FindAndValidateButtonComponent(transform, "RestartBTN");
        _restartButton.onClick.AddListener(() => ScenesManager.Instance.LoadLevelByType(
                                                    GameManager.Instance.SetGetWorldState.GetLevel,
                                                    GameManager.Instance.GetCurrentGameMode));

        _resumeButton = FindAndValidateButtonComponent(transform, "ResumeBTN");
        _resumeButton.onClick.AddListener(OnResumeClicked);
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
        UIManager.Instance.ActivateUI(_winText.gameObject, transform);
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
}
