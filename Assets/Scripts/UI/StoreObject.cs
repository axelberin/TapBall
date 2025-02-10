using UnityEngine.UI;

public class StoreObject : CanvasElementLocator
{
    private Image _image;
    private Button _buyButton;
    private Button _equipButton;
    private Button _unequipButton;

    private void Start()
    {
        _image = FindAndValidateImageComponent(transform, "Image");
        _buyButton = FindAndValidateButtonComponent(transform, "BuyButton");
        _equipButton = FindAndValidateButtonComponent(transform, "EquipButton");
        _unequipButton = FindAndValidateButtonComponent(transform, "UnequipButton");

        _buyButton.onClick.AddListener(() =>
        {
            if (StoreManager.Instance.CanBuy(0, false)) //TODO Poner precio en base a prefab de skin o valor guardado.
                StoreManager.Instance.Buy(0);
            else
                return; //TODO Crear cartel de que no se puede comprar.
        });

        //_equipButton.onClick.AddListener();
        //_unequipButton.onClick.AddListener();
    }
}
