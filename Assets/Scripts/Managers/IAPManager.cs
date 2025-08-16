using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : MonoBehaviour, IStoreListener
{
    public static IAPManager Instance { get; private set; }

    public Action OnCompletePurchase = delegate { };

    private static IStoreController m_StoreController;
    private static IExtensionProvider m_StoreExtensionProvider;

    public const string PRODUCT_NO_ADS = "no_ads_product";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    void Start()
    {
        if (m_StoreController == null)
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            builder.AddProduct(PRODUCT_NO_ADS, ProductType.NonConsumable);

            UnityPurchasing.Initialize(this, builder);
        }
    }

    private bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    public void BuyProductID(string productId)
    {
        if (IsInitialized())
        {
            Product product = m_StoreController.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                Debug.Log($"Purchasing product: {product.definition.id}");
                m_StoreController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log("BuyProductID: FAIL. Product not found or not available.");
            }
        }
        else
        {
            Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }

    public Product GetProductByID(string id)
    {
        return m_StoreController.products.WithID(id);
    }

    // --------------------------
    // Funciones para IAPButton
    // --------------------------
    public void OnPurchaseComplete(Product product)
    {
        Debug.Log($"OnPurchaseComplete (IAPButton): {product.definition.storeSpecificId}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureReason)
    {
        Debug.Log($"OnPurchaseFailed (IAPButton): {product.definition.storeSpecificId}, Reason: {failureReason}");
    }

    public void OnProductFetched(Product product)
    {
        Debug.Log($"OnProductFetched (IAPButton): {product.definition.storeSpecificId}");
    }

    // --------------------------
    // Implementación IStoreListener
    // --------------------------
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        m_StoreController = controller;
        m_StoreExtensionProvider = extensions;
        Debug.Log("IAPManager Initialized");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log($"IAP Initialization Failed: {error}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string id = args.purchasedProduct.definition.id;

        Debug.Log($"ProcessPurchase: {id}");
        GetRewardAfterProcessPurchase(id);

        return PurchaseProcessingResult.Complete;
    }

    private void GetRewardAfterProcessPurchase(string id)
    {
        switch (id)
        {
            case PRODUCT_NO_ADS:
                SaveAndLoadManager.SetIntValue(
                    SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName) + 30, SaveAndLoadManager.CoinsName);
                SaveAndLoadManager.SetIntValue(
                    SaveAndLoadManager.GetIntValue(SaveAndLoadManager.OrbsName) + 10, SaveAndLoadManager.OrbsName);
                SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.NoAdsBougthName, true, true);
                Debug.Log($"Reward: {id}");
                break;
            default:
                Debug.LogError($"Product not found: {id}");
                break;
        }

        OnCompletePurchase?.Invoke();
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log($"OnPurchaseFailed (Global): {product.definition.id}, Reason: {failureReason}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log($"OnPurchaseFailed: {error}, Reason: {message}");
    }
}
