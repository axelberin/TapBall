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
                    SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName) + 15, SaveAndLoadManager.CoinsName);
                SaveAndLoadManager.SetIntValue(
                    SaveAndLoadManager.GetIntValue(SaveAndLoadManager.OrbsName) + 5, SaveAndLoadManager.OrbsName);
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

/*
using UnityEngine;
using UnityEngine.Purchasing;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using System;
using System.Threading.Tasks;

public class IAPManager : MonoBehaviour, IStoreListener
{
    public static IAPManager Instance { get; private set; }

    private IStoreController storeController;
    private IExtensionProvider extensionProvider;

    public const string PRODUCT_NO_ADS = "no_ads_product";
    public Action OnCompletePurchase = delegate { };

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        InitializeUnityServices();
    }

    // --------------------
    // Inicializa Unity Gaming Services manualmente
    // --------------------
    private async void InitializeUnityServices()
    {
        try
        {
            var options = new InitializationOptions()
                .SetEnvironmentName("production"); // podes cambiarlo si usas otro env

            await UnityServices.InitializeAsync(options);
            Debug.Log("Unity Gaming Services inicializado");

            InitializePurchasing(); // ahora sí inicializamos IAP
        }
        catch (Exception e)
        {
            Debug.LogError("Error inicializando UGS: " + e);
        }
    }

    private void InitializePurchasing()
    {
        if (storeController != null)
        {
            Debug.Log("IAP ya estaba inicializado");
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(PRODUCT_NO_ADS, ProductType.NonConsumable);

        UnityPurchasing.Initialize(this, builder);
    }

    public void BuyNoAds()
    {
        if (storeController != null)
        {
            storeController.InitiatePurchase(PRODUCT_NO_ADS);
        }
        else
        {
            Debug.LogError("IAP no inicializado");
        }
    }

    public Product GetProductByID(string id)
    {
        if (storeController != null)
            return storeController.products.WithID(id);

        Debug.LogError("StoreController no inicializado");
        return null;
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Debug.Log("Compra completada: " + args.purchasedProduct.definition.id);
        GetRewardAfterProcessPurchase(args.purchasedProduct.definition.id);
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        Debug.LogError($"Compra fallida: {product.definition.id}, Razón: {reason}");
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        extensionProvider = extensions;
        Debug.Log("IAP inicializado correctamente con " + controller.products.all.Length + " productos");
        foreach (var p in controller.products.all)
            Debug.Log($"Producto cargado: {p.definition.id}, disponible={p.availableToPurchase}");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError("Falló la inicialización de IAP: " + error);
    }

    private void GetRewardAfterProcessPurchase(string id)
    {
        switch (id)
        {
            case PRODUCT_NO_ADS:
                SaveAndLoadManager.SetIntValue(
                    SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName) + 15, SaveAndLoadManager.CoinsName);
                SaveAndLoadManager.SetIntValue(
                    SaveAndLoadManager.GetIntValue(SaveAndLoadManager.OrbsName) + 5, SaveAndLoadManager.OrbsName);
                SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.NoAdsBougthName, true, true);

                Debug.Log("Recompensa aplicada: No Ads comprado");
                break;

            default:
                Debug.LogError($"Producto no reconocido: {id}");
                break;
        }

        OnCompletePurchase?.Invoke();
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError(error + " " + message);
    }
}

*/
