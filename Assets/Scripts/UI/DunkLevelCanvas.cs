using TMPro;
using UnityEngine.UI;

public class DunkLevelCanvas : ACanvas
{
    public static DunkLevelCanvas Instance;

    private TextMeshProUGUI _tapCountText;
    private TextMeshProUGUI _winTime;
    private TextMeshProUGUI _winText;
    private Button _menuButton;
    private Button _nextLevelButton;

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

        _nextLevelButton = FindAndValidateButtonComponent(transform, "NextLevelBTN");

        if (_nextLevelButton.interactable)
            _nextLevelButton.onClick.AddListener(() =>
            {
                ScenesManager.Instance.LoadNextLevel(GameManager.Instance.SetGetWorldState.GetLevel);
                AdsManager.Instance.LoadInterstitialAd();
            });
    }

    public void OnWin()
    {
        _nextLevelButton.interactable = ScenesManager.Instance.IsSceneExisting(
            $"DunkLevel{GameManager.Instance.SetGetWorldState.GetLevel + 1}");

        UIManager.Instance.ActivateUI(_winTime.gameObject, false);
        UIManager.Instance.ActivateUI(_winText.gameObject, transform);
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
