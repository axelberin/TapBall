using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreCanvas : CanvasElementLocator
{
    public static StoreCanvas Instance { get; private set; }

    private TextMeshProUGUI _coinsText;
    private TextMeshProUGUI _orbsText;
    private TextMeshProUGUI _icePowerUpText;
    private TextMeshProUGUI _noTouchowerUpText;
    private TextMeshProUGUI _immunityPowerUpText;
    private TextMeshProUGUI _revivePowerUpText;
    private PopUp _reviewPopUp;
    private Button _ballsFlapButton;
    private Image _ballsFlapOnImage;
    private Image _ballsFlapOffImage;
    private Button _offersFlapButton;
    private Image _offersFlapOnImage;
    private Image _offersFlapOffImage;
    private GameObject _ballsScroll;
    private GameObject _offersScroll;

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
        _icePowerUpText = FindAndValidateComponent<TextMeshProUGUI>(transform, "SnowPowerUpText");
        _noTouchowerUpText = FindAndValidateComponent<TextMeshProUGUI>(transform, "StonePowerupText");
        _immunityPowerUpText = FindAndValidateComponent<TextMeshProUGUI>(transform, "ShieldPowerUpText");
        _revivePowerUpText = FindAndValidateComponent<TextMeshProUGUI>(transform, "RevivalPowerupText");

        var closeButton = FindAndValidateComponent<Button>(transform, "StoreCloseButton");

        closeButton.onClick.AddListener(() => UIManager.Instance.ChangeCanvas(
            "StoreCanvas", "MenuManagerCanvas"));

        _ballsScroll = FindAndValidateGameObjectComponent(transform, "BallSkinsScroll");
        _offersScroll = FindAndValidateGameObjectComponent(transform, "OffersScroll");

        _ballsFlapButton = FindAndValidateComponent<Button>(transform, "BallsFlapButton");
        _ballsFlapButton.onClick.AddListener(OnSelectBallsFlap);
        _ballsFlapOnImage = FindAndValidateComponent<Image>(_ballsFlapButton.transform, "FlapOn");
        _ballsFlapOffImage = FindAndValidateComponent<Image>(_ballsFlapButton.transform, "FlapOff");

        _offersFlapButton = FindAndValidateComponent<Button>(transform, "OffersFlapButton");
        _offersFlapButton.onClick.AddListener(OnSelectOffersFlap);
        _offersFlapOnImage = FindAndValidateComponent<Image>(_offersFlapButton.transform, "FlapOn");
        _offersFlapOffImage = FindAndValidateComponent<Image>(_offersFlapButton.transform, "FlapOff");

        UpdateTexts();
        OnSelectBallsFlap();
        _reviewPopUp = FindAndValidateComponent<PopUp>(transform, "ReviewPopUp");
        _reviewPopUp.gameObject.SetActive(true);

        UIManager.Instance.AddCanvas(gameObject, false);
    }

    private void OnEnable()
    {
        if (_coinsText || _orbsText)
            UpdateTexts();
        if (IAPManager.Instance)
            IAPManager.Instance.OnCompletePurchase += UpdateTexts;
    }

    private void OnDisable()
    {
        if (IAPManager.Instance)
            IAPManager.Instance.OnCompletePurchase -= UpdateTexts;
    }

    public void UpdateTexts()
    {
        UIManager.Instance.SetText(_coinsText, SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName));
        UIManager.Instance.SetText(_orbsText, SaveAndLoadManager.GetIntValue(SaveAndLoadManager.OrbsName));
        UIManager.Instance.SetText(_icePowerUpText, SaveAndLoadManager.GetIntValue(
            SaveAndLoadManager.PowerUpPrefix + PowerUpManager.PowerUpType.TimeStopPowerUp));
        UIManager.Instance.SetText(_noTouchowerUpText, SaveAndLoadManager.GetIntValue
            (SaveAndLoadManager.PowerUpPrefix + PowerUpManager.PowerUpType.StopTouchCounterPowerUp));
        UIManager.Instance.SetText(_immunityPowerUpText, SaveAndLoadManager.GetIntValue(
            SaveAndLoadManager.PowerUpPrefix + PowerUpManager.PowerUpType.ImmunityPowerUp));
        UIManager.Instance.SetText(_revivePowerUpText, SaveAndLoadManager.GetIntValue(
            SaveAndLoadManager.PowerUpPrefix + PowerUpManager.PowerUpType.RevivePowerUp));
    }

    public void ShowReview()
    {
        _reviewPopUp.Initialize("rateus", "reviewdescription", OpenReviewFromLink);
        _reviewPopUp.Show();
    }

    private void OpenReviewFromLink()
    {
        Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);
    }

    private void OnSelectBallsFlap()
    {
        _ballsScroll.SetActive(true);
        _offersScroll.SetActive(false);
        UpdateFlapsButtonsState();
    }

    private void OnSelectOffersFlap()
    {
        _ballsScroll.SetActive(false);
        _offersScroll.SetActive(true);
        UpdateFlapsButtonsState();
    }

    private void UpdateFlapsButtonsState()
    {
        _ballsFlapOffImage.gameObject.SetActive(!_ballsScroll.activeInHierarchy);
        _ballsFlapOnImage.gameObject.SetActive(_ballsScroll.activeInHierarchy);
        _offersFlapOffImage.gameObject.SetActive(!_offersScroll.activeInHierarchy);
        _offersFlapOnImage.gameObject.SetActive(_offersScroll.activeInHierarchy);
    }
}
