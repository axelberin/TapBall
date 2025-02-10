using UnityEngine;
using UnityEngine.AddressableAssets;

public class StoreCanvas : CanvasElementLocator
{
    public static StoreCanvas Instance { get; private set; }

    [SerializeField] private string _storeObjectPrefabName;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        Addressables.InstantiateAsync(_storeObjectPrefabName, transform);
    }
}
