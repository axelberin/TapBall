using TMPro;
using UnityEngine.UI;

public class StoreCanvas : CanvasElementLocator
{
    public static StoreCanvas Instance { get; private set; }

    private TextMeshProUGUI _coinsText;
    private TextMeshProUGUI _orbsText;
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
        _orbsText = FindAndValidateComponent<TextMeshProUGUI>(transform, "OrbsText");
        UpdateCoinsAndOrbsTexts();

        var closeButton = FindAndValidateComponent<Button>(transform, "StoreCloseButton");

        closeButton.onClick.AddListener(() => UIManager.Instance.ChangeCanvas(
            "StoreCanvas", "MenuManagerCanvas"));

        _reviewPopUp = FindAndValidateComponent<PopUp>(transform, "ReviewPopUp");
        _reviewPopUp.Initialize("rateus", "reviewdescription", ReviewManagerController.Instance.RequestReview);

        UIManager.Instance.AddCanvas(gameObject, false);
    }

    private void OnEnable()
    {
        UpdateCoinsAndOrbsTexts();
        if (IAPManager.Instance)
            IAPManager.Instance.OnCompletePurchase += UpdateCoinsAndOrbsTexts;
    }

    private void OnDisable()
    {
        if (IAPManager.Instance)
            IAPManager.Instance.OnCompletePurchase -= UpdateCoinsAndOrbsTexts;
    }

    public void UpdateCoinsAndOrbsTexts()
    {
        if (_coinsText != null)
            UIManager.Instance.SetText(_coinsText, SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName));
        if (_orbsText != null)
            UIManager.Instance.SetText(_orbsText, SaveAndLoadManager.GetIntValue(SaveAndLoadManager.OrbsName));
    }

    public void ShowReview()
    {
        _reviewPopUp.Show();
    }
}
