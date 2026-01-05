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

        var buyTenGoldButton = FindAndValidateComponent<Button>(_offersScroll.transform, "CoinsSlotOffert1");
        buyTenGoldButton.onClick.AddListener(() =>
        {
            if (SaveAndLoadManager.GetIntValue(SaveAndLoadManager.OrbsName) >= 1)
            {
                SaveAndLoadManager.SetIntValue(
                        SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName) + 10, SaveAndLoadManager.CoinsName);
                SaveAndLoadManager.SetIntValue(
                        SaveAndLoadManager.GetIntValue(SaveAndLoadManager.OrbsName) - 1, SaveAndLoadManager.OrbsName);

                UpdateTexts();
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.PurchaseSound);
            }
            else
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.RejectionSound);
        });

        var buyIcePowerUpButton = FindAndValidateComponent<Button>(_offersScroll.transform, "IceSlot");
        buyIcePowerUpButton.onClick.AddListener(() =>
        {
            if (StoreManager.Instance.CanBuy(10, false))
            {
                StoreManager.Instance.Buy(10);
                string powerUpName = SaveAndLoadManager.PowerUpPrefix + PowerUpManager.PowerUpType.TimeStopPowerUp;
                SaveAndLoadManager.SetIntValue(SaveAndLoadManager.GetIntValue(powerUpName) + 1, powerUpName);
                UpdateTexts();
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.PurchaseSound);
            }
            else
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.RejectionSound);
        });

        var buyStonePowerUpButton = FindAndValidateComponent<Button>(_offersScroll.transform, "StoneSlot");
        buyStonePowerUpButton.onClick.AddListener(() =>
        {
            if (StoreManager.Instance.CanBuy(15, false))
            {
                StoreManager.Instance.Buy(15);
                string powerUpName = SaveAndLoadManager.PowerUpPrefix + PowerUpManager.PowerUpType.StopTouchCounterPowerUp;
                SaveAndLoadManager.SetIntValue(SaveAndLoadManager.GetIntValue(powerUpName) + 1, powerUpName);
                UpdateTexts();
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.PurchaseSound);
            }
            else
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.RejectionSound);
        });

        var buyImmunityPowerUpButton = FindAndValidateComponent<Button>(_offersScroll.transform, "InmmunitySlot");
        buyImmunityPowerUpButton.onClick.AddListener(() =>
        {
            if (StoreManager.Instance.CanBuy(30, false))
            {
                StoreManager.Instance.Buy(30);
                string powerUpName = SaveAndLoadManager.PowerUpPrefix + PowerUpManager.PowerUpType.ImmunityPowerUp;
                SaveAndLoadManager.SetIntValue(SaveAndLoadManager.GetIntValue(powerUpName) + 1, powerUpName);
                UpdateTexts();
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.PurchaseSound);
            }
            else
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.RejectionSound);
        });

        var buyRevivePowerUpButton = FindAndValidateComponent<Button>(_offersScroll.transform, "ReviveSlot");
        buyRevivePowerUpButton.onClick.AddListener(() =>
        {
            if (StoreManager.Instance.CanBuy(50, false))
            {
                StoreManager.Instance.Buy(50);
                string powerUpName = SaveAndLoadManager.PowerUpPrefix + PowerUpManager.PowerUpType.RevivePowerUp;
                SaveAndLoadManager.SetIntValue(SaveAndLoadManager.GetIntValue(powerUpName) + 1, powerUpName);
                UpdateTexts();
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.PurchaseSound);
            }
            else
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.RejectionSound);
        });

        var fiftyGoldPriceText = FindAndValidateComponent<TextMeshProUGUI>(_offersScroll.transform, "FiftyGoldPriceText");
        UIManager.Instance.SetText(fiftyGoldPriceText, "US$" + IAPManager.Instance.GetProductPriceById(IAPManager.FIFTY_GOLD));
        var oneHundredGoldPriceText = FindAndValidateComponent<TextMeshProUGUI>(_offersScroll.transform, "OneHundredGoldPriceText");
        UIManager.Instance.SetText(oneHundredGoldPriceText, "US$" + IAPManager.Instance.GetProductPriceById(IAPManager.ONE_HUNDRED_GOLD));
        var tenOrbsPriceText = FindAndValidateComponent<TextMeshProUGUI>(_offersScroll.transform, "TenOrbsPriceText");
        UIManager.Instance.SetText(tenOrbsPriceText, "US$" + IAPManager.Instance.GetProductPriceById(IAPManager.TEN_ORBS));
        var fiftyOrbsPriceText = FindAndValidateComponent<TextMeshProUGUI>(_offersScroll.transform, "FiftyOrbsPriceText");
        UIManager.Instance.SetText(fiftyOrbsPriceText, "US$" + IAPManager.Instance.GetProductPriceById(IAPManager.FIFTY_ORBS));
        var oneHundredOrbsPriceText = FindAndValidateComponent<TextMeshProUGUI>(_offersScroll.transform, "OneHundredOrbsPriceText");
        UIManager.Instance.SetText(oneHundredOrbsPriceText, "US$" + IAPManager.Instance.GetProductPriceById(IAPManager.ONE_HUNDRED_ORBS));
        var orbitalPackPriceText = FindAndValidateComponent<TextMeshProUGUI>(_offersScroll.transform, "OrbitalPackPriceText");
        UIManager.Instance.SetText(orbitalPackPriceText, "US$" + IAPManager.Instance.GetProductPriceById(IAPManager.ORBITAL_PACK));
        var galacticPackPriceText = FindAndValidateComponent<TextMeshProUGUI>(_offersScroll.transform, "GalacticPackPriceText");
        UIManager.Instance.SetText(galacticPackPriceText, "US$" + IAPManager.Instance.GetProductPriceById(IAPManager.GALACTIC_PACK));
        var multiversalPackPriceText = FindAndValidateComponent<TextMeshProUGUI>(_offersScroll.transform, "MultiversalPackPriceText");
        UIManager.Instance.SetText(multiversalPackPriceText, "US$" + IAPManager.Instance.GetProductPriceById(IAPManager.MULTIVERSAL_PACK));
        var icePackPriceText = FindAndValidateComponent<TextMeshProUGUI>(_offersScroll.transform, "IcePackPriceText");
        UIManager.Instance.SetText(icePackPriceText, "US$" + IAPManager.Instance.GetProductPriceById(IAPManager.ICE_PACK));
        var stonePackPriceText = FindAndValidateComponent<TextMeshProUGUI>(_offersScroll.transform, "StonePackPriceText");
        UIManager.Instance.SetText(stonePackPriceText, "US$" + IAPManager.Instance.GetProductPriceById(IAPManager.STONE_PACK));
        var protectionPackPriceText = FindAndValidateComponent<TextMeshProUGUI>(_offersScroll.transform, "ProtectionPackPriceText");
        UIManager.Instance.SetText(protectionPackPriceText, "US$" + IAPManager.Instance.GetProductPriceById(IAPManager.PROTECTION_PACK));
        var eternalPackPriceText = FindAndValidateComponent<TextMeshProUGUI>(_offersScroll.transform, "EternalPackPriceText");
        UIManager.Instance.SetText(eternalPackPriceText, "US$" + IAPManager.Instance.GetProductPriceById(IAPManager.INMORTAL_PACK));

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
