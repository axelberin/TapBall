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
            gameObject.SetActive(false);
    }

    private void Start()
    {
        _image = FindAndValidateImageComponent(transform, "ObjectImage");
        _buyButton = FindAndValidateButtonComponent(transform, "BuyButton");
        _equipButton = FindAndValidateButtonComponent(transform, "EquipButton");
        _equipText = FindAndValidateTextComponent(transform, "EquipText");

        _image.sprite = _skinSC.sprite;
        _image.rectTransform.sizeDelta = _skinSC.spriteSize;

        if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CurrentBallSkin))
            SaveAndLoadManager.SetStringValue(SaveAndLoadManager.CurrentBallSkin, SaveAndLoadManager.CurrentBallSkin);

        var priceText = FindAndValidateTextComponent(transform, "PriceText");
        priceText.text = _skinSC.price.ToString();

        _buyButton.onClick.AddListener(() =>
        {
            if (StoreManager.Instance.CanBuy(_skinSC.price, false))
            {
                StoreManager.Instance.Buy(_skinSC.price);
                OnSkinIsSelected();
                StoreCanvas.Instance.UpdateCoinsText();
                SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.ObtainedBallSkins + _skinSC.skinName);
            }
            else
                return; //TODO Crear cartel de que no se puede comprar.
        });

        _equipButton.onClick.AddListener(() => OnSkinIsSelected());

        if (SaveAndLoadManager.CurrentBallSkin == _skinSC.skinName)
            OnSkinIsSelected();
        else if (SaveAndLoadManager.GetIntValue(SaveAndLoadManager.ObtainedBallSkins + _skinSC.skinName) == 1)
            OnSkinUnselected();
    }

    private void OnSkinIsSelected()
    {
        _buyButton.gameObject.SetActive(false);
        _equipButton.gameObject.SetActive(true);
        _equipButton.interactable = false;
        _equipText.text = LanguageManager.Instance.GetLocalizedText("equiped");
        SaveAndLoadManager.SetStringValue(_skinSC.skinName, SaveAndLoadManager.CurrentBallSkin, true);
    }

    private void OnSkinUnselected()
    {
        _equipButton.interactable = true;
        _equipText.text = LanguageManager.Instance.GetLocalizedText("equip");
    }
}
