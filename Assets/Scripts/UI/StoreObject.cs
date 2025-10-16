using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreObject : CanvasElementLocator
{
    [SerializeField] private UISkins _skinSC;

    private Image _image;
    private Button _buyButton;
    private Button _equipButton;
    private TextMeshProUGUI _equipText;
    private Animator _backgroundAnimator;


    private void Awake()
    {
        if (_skinSC == null)
        {
            gameObject.SetActive(false);
            return;
        }

        _image = FindAndValidateComponent<Image>(transform, "ObjectImage");
        _buyButton = FindAndValidateComponent<Button>(transform, "BuyButton");
        _equipButton = FindAndValidateComponent<Button>(transform, "EquipButton");
        _equipText = FindAndValidateComponent<TextMeshProUGUI>(transform, "EquipText");

        _image.sprite = _skinSC.sprite;
        _image.rectTransform.sizeDelta = _skinSC.spriteSize;

        var priceText = FindAndValidateComponent<TextMeshProUGUI>(transform, "PriceText");
        if (UIManager.Instance != null)
            UIManager.Instance.SetText(priceText, _skinSC.price);

        var backgroundImage = FindAndValidateComponent<Image>(transform, "BackgroundImage");
        if (_skinSC.backgroundSprite != null)
            backgroundImage.sprite = _skinSC.backgroundSprite;
        else if (_skinSC.backgroundAnimator != null)
        {
            _backgroundAnimator = backgroundImage.gameObject.AddComponent<Animator>();
            _backgroundAnimator.runtimeAnimatorController = _skinSC.backgroundAnimator;
            _backgroundAnimator.enabled = true;
        }

        _buyButton.onClick.AddListener(() =>
        {
            if (StoreManager.Instance.CanBuy(_skinSC.price, false))
            {
                StoreManager.Instance.Buy(_skinSC.price);
                StoreCanvas.Instance.UpdateCoinsAndOrbsTexts();
                SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.ObtainedBallSkins + _skinSC.skinName);
                SaveAndLoadManager.SetStringValue(_skinSC.skinName, SaveAndLoadManager.CurrentBallSkinName, true, true);

                StoreManager.Instance.UpdateSkinsState?.Invoke();

                if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.ReviewSowedName))
                {
                    StoreCanvas.Instance.ShowReview();
                    SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.ReviewSowedName, true, true);
                }
            }
            else
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.RejectionSound);
        });

        _equipButton.onClick.AddListener(() =>
        {
            SaveAndLoadManager.SetStringValue(_skinSC.skinName, SaveAndLoadManager.CurrentBallSkinName, true, true);
            StoreManager.Instance.UpdateSkinsState?.Invoke();
        });
    }

    private void Start()
    {
        StoreManager.Instance.UpdateSkinsState += UpdateSkinState;
    }

    private void OnEnable()
    {
        if (!_image || !_buyButton || !_equipButton || !_equipText ||
            LanguageManager.Instance == null)
            return;

        UpdateSkinState();
    }

    public void UpdateSkinState()
    {
        if (_skinSC.unlockeable && SaveAndLoadManager.GetIntValue(SaveAndLoadManager.ObtainedBallSkins + _skinSC.skinName) == 0)
            OnSkinLocked();
        else
        {
            if (SaveAndLoadManager.GetStringValue(SaveAndLoadManager.CurrentBallSkinName) == _skinSC.skinName)
                OnSkinIsSelected();
            else if (SaveAndLoadManager.GetIntValue(SaveAndLoadManager.ObtainedBallSkins + _skinSC.skinName) == 1)
                OnSkinUnselected();
        }
    }

    private void OnSkinLocked()
    {
        ChangeButtonConditions(false, "locked");
    }

    private void OnSkinIsSelected()
    {
        ChangeButtonConditions(false, "equiped");
    }

    private void OnSkinUnselected()
    {
        ChangeButtonConditions(true, "equip");
    }

    private void ChangeButtonConditions(bool interactable, string buttonText)
    {
        _buyButton.gameObject.SetActive(false);
        _equipButton.gameObject.SetActive(true);
        _equipButton.interactable = interactable;
        UIManager.Instance.SetText(_equipText, LanguageManager.Instance.GetLocalizedText(buttonText));
    }
}
