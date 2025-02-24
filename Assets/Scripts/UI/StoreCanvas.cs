using UnityEngine;
using UnityEngine.UI;

public class StoreCanvas : CanvasElementLocator
{
    public static StoreCanvas Instance { get; private set; }

    [SerializeField] private string _storeObjectPrefabName;

    private Transform _ballSkinsViewportContent;
    private Button _closeButton;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        _ballSkinsViewportContent = FindAndValidateTransformComponent(transform, "BallSkinsContent");
        _closeButton = FindAndValidateButtonComponent(transform, "StoreCloseButton");

        _closeButton.onClick.AddListener(() => UIManager.Instance.ChangeCanvas(
            "StoreCanvas", "MenuManagerCanvas"));
        //Addressables.InstantiateAsync(_storeObjectPrefabName, _ballSkinsViewportContent);

        UIManager.Instance.AddCanvas(gameObject, false);
    }
}
