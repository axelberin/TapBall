using UnityEngine;
using UnityEngine.AddressableAssets;

public class StoreCanvas : CanvasElementLocator
{
    public static StoreCanvas Instance { get; private set; }

    [SerializeField] private string _storeObjectPrefabName;

    private Transform _ballSkinsViewportContent;

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

        Addressables.InstantiateAsync(_storeObjectPrefabName, _ballSkinsViewportContent);

        UIManager.Instance.AddCanvas(gameObject, false);
    }
}
