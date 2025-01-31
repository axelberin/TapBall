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
    }
}
