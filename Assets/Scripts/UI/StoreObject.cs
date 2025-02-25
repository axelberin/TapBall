using UnityEngine;
using UnityEngine.UI;

public class StoreObject : CanvasElementLocator
{
    [SerializeField] private UISkins _skinSC;

    private Image _image;
    private Button _buyButton;
    private Button _equipButton;

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

        _image.sprite = _skinSC.sprite;
        _image.rectTransform.sizeDelta = _skinSC.spriteSize;

        if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CurrentBallSkin))
            SaveAndLoadManager.SetStringValue(SaveAndLoadManager.CurrentBallSkin, SaveAndLoadManager.CurrentBallSkin);

        _buyButton.onClick.AddListener(() =>
        {
            if (StoreManager.Instance.CanBuy(_skinSC.price, false)) //TODO Poner precio en base a prefab de skin o valor guardado.
            {
                StoreManager.Instance.Buy(_skinSC.price);
                OnSkinIsSelected();
                StoreCanvas.Instance.UpdateCoinsText();
            }
            else
                return; //TODO Crear cartel de que no se puede comprar.
        });

        _equipButton.onClick.AddListener(() =>
        {
            SaveAndLoadManager.SetStringValue(SaveAndLoadManager.CurrentBallSkin, _skinSC.skinName);
            OnSkinIsSelected();
        });

        if (SaveAndLoadManager.CurrentBallSkin == _skinSC.skinName)
            OnSkinIsSelected();
    }

    private void OnSkinIsSelected()
    {
        _buyButton.gameObject.SetActive(false);
        _equipButton.gameObject.SetActive(true);
        _equipButton.interactable = false;
    }
}
