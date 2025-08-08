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
        priceText.text = _skinSC.price.ToString();

        _buyButton.onClick.AddListener(() =>
        {
            if (StoreManager.Instance.CanBuy(_skinSC.price, false))
            {
                StoreManager.Instance.Buy(_skinSC.price);
                StoreCanvas.Instance.UpdateCoinsText();
                SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.ObtainedBallSkins + _skinSC.skinName);
                SaveAndLoadManager.SetStringValue(_skinSC.skinName, SaveAndLoadManager.CurrentBallSkinName);

                StoreManager.Instance.UpdateSkinsState?.Invoke();

                if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.ReviewSowed))
                {
                    StoreCanvas.Instance.ShowReview();
                    SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.ReviewSowed);
                }

                SaveAndLoadManager.Save();
            }
            else
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.RejectionSound);
        });

        _equipButton.onClick.AddListener(() =>
        {
            SaveAndLoadManager.SetStringValue(_skinSC.skinName, SaveAndLoadManager.CurrentBallSkinName, true);
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
        _buyButton.gameObject.SetActive(false);
        _equipButton.gameObject.SetActive(true);
        _equipButton.interactable = false;
        var (text, font) = LanguageManager.Instance.GetlocalizatedTextAndFont("locked");
        _equipText.text = text;
        _equipText.font = font;
    }

    private void OnSkinIsSelected()
    {
        _buyButton.gameObject.SetActive(false);
        _equipButton.gameObject.SetActive(true);
        _equipButton.interactable = false;
        var (text, font) = LanguageManager.Instance.GetlocalizatedTextAndFont("equiped");
        _equipText.text = text;
        _equipText.font = font;
    }

    private void OnSkinUnselected()
    {
        _buyButton.gameObject.SetActive(false);
        _equipButton.gameObject.SetActive(true);
        _equipButton.interactable = true;
        var (text, font) = LanguageManager.Instance.GetlocalizatedTextAndFont("equip");
        _equipText.text = text;
        _equipText.font = font;
    }
}
