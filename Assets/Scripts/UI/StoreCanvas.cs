using TMPro;
using UnityEngine.UI;

public class StoreCanvas : CanvasElementLocator
{
    public static StoreCanvas Instance { get; private set; }

    private TextMeshProUGUI _coinsText;
    private PopUp _reviewPopUp;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        _coinsText = FindAndValidateComponent<TextMeshProUGUI>(transform, "CoinsText");
        if (_coinsText != null && SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CoinsName))
            _coinsText.text = SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName).ToString();

        var closeButton = FindAndValidateComponent<Button>(transform, "StoreCloseButton");

        closeButton.onClick.AddListener(() => UIManager.Instance.ChangeCanvas(
            "StoreCanvas", "MenuManagerCanvas"));

        _reviewPopUp = FindAndValidateComponent<PopUp>(transform, "ReviewPopUp");
        _reviewPopUp.Initialize("rateus", "reviewdescription", ReviewManagerController.Instance.RequestReview);

        UIManager.Instance.AddCanvas(gameObject, false);
    }

    public void UpdateCoinsText()
    {
        UIManager.Instance.SetText(_coinsText, SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName));
    }

    public void ShowReview()
    {
        _reviewPopUp.Show();
    }
}
