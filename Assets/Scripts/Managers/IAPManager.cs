using System;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour, IStoreListener
{
    public static IAPManager Instance { get; private set; }

    public Action OnCompletePurchase = delegate { };

    private static IStoreController m_StoreController;
    private static IExtensionProvider m_StoreExtensionProvider;

    public const string PRODUCT_NO_ADS = "no_ads_product";
    public const string FIFTY_GOLD = "fifty_gold";
    public const string ONE_HUNDRED_GOLD = "one_hundred_gold";
    public const string TEN_ORBS = "ten_orbs";
    public const string FIFTY_ORBS = "fifty_orbs";
    public const string ONE_HUNDRED_ORBS = "one_hundred_orbs";
    public const string ORBITAL_PACK = "orbital_pack";
    public const string GALACTIC_PACK = "galactic_pack";
    public const string MULTIVERSAL_PACK = "multiversal_pack";
    public const string ICE_PACK = "ice_pack";
    public const string STONE_PACK = "stone_pack";
    public const string PROTECTION_PACK = "protection_pack";
    public const string INMORTAL_PACK = "inmortal_pack";

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
            builder.AddProduct(FIFTY_GOLD, ProductType.Consumable);
            builder.AddProduct(ONE_HUNDRED_GOLD, ProductType.Consumable);
            builder.AddProduct(TEN_ORBS, ProductType.Consumable);
            builder.AddProduct(FIFTY_ORBS, ProductType.Consumable);
            builder.AddProduct(ONE_HUNDRED_ORBS, ProductType.Consumable);
            builder.AddProduct(ORBITAL_PACK, ProductType.Consumable);
            builder.AddProduct(GALACTIC_PACK, ProductType.Consumable);
            builder.AddProduct(MULTIVERSAL_PACK, ProductType.Consumable);
            builder.AddProduct(ICE_PACK, ProductType.Consumable);
            builder.AddProduct(STONE_PACK, ProductType.Consumable);
            builder.AddProduct(PROTECTION_PACK, ProductType.Consumable);
            builder.AddProduct(INMORTAL_PACK, ProductType.Consumable);

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
        if (!IsInitialized())
        {
            Debug.LogWarning($"[IAP] GetProductByID({id}) llamado antes de inicializar.");
            return null;
        }

        var product = m_StoreController.products.WithID(id);
        if (product == null)
            Debug.LogError($"[IAP] Producto no encontrado en el controller: {id}");

        return product;
    }

    public string GetProductPriceById(string id)
    {
        var product = GetProductByID(id);
        return product != null && product.metadata != null ? product.metadata.localizedPriceString : "-";
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
                AddCoinsOnProduct(100);
                AddOrbsOnProduct(15);
                SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.NoAdsBougthName, true, true);
                Debug.Log($"Reward: {id}");
                break;
            case FIFTY_GOLD:
                AddCoinsOnProduct(50);
                break;
            case ONE_HUNDRED_GOLD:
                AddCoinsOnProduct(100);
                break;
            case TEN_ORBS:
                AddOrbsOnProduct(10);
                break;
            case FIFTY_ORBS:
                AddOrbsOnProduct(50);
                break;
            case ONE_HUNDRED_ORBS:
                AddOrbsOnProduct(100);
                break;
            case ORBITAL_PACK:
                AddCoinsOnProduct(50);
                AddOrbsOnProduct(25);
                break;
            case GALACTIC_PACK:
                AddCoinsOnProduct(250);
                AddOrbsOnProduct(100);
                break;
            case MULTIVERSAL_PACK:
                AddCoinsOnProduct(500);
                AddOrbsOnProduct(250);
                break;
            case ICE_PACK:
                AddCoinsOnProduct(25);
                AddOrbsOnProduct(15);
                AddPowerUpOnProduct(15, PowerUpManager.PowerUpType.TimeStopPowerUp);
                break;
            case STONE_PACK:
                AddCoinsOnProduct(25);
                AddOrbsOnProduct(15);
                AddPowerUpOnProduct(15, PowerUpManager.PowerUpType.StopTouchCounterPowerUp);
                break;
            case PROTECTION_PACK:
                AddCoinsOnProduct(25);
                AddOrbsOnProduct(15);
                AddPowerUpOnProduct(15, PowerUpManager.PowerUpType.ImmunityPowerUp);
                break;
            case INMORTAL_PACK:
                AddCoinsOnProduct(25);
                AddOrbsOnProduct(15);
                AddPowerUpOnProduct(15, PowerUpManager.PowerUpType.RevivePowerUp);
                break;
            default:
                Debug.LogError($"Product not found: {id}");
                break;
        }

        OnCompletePurchase?.Invoke();
    }

    private void AddCoinsOnProduct(int amount)
    {
        SaveAndLoadManager.SetIntValue(
                    SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName) + amount, SaveAndLoadManager.CoinsName);
    }

    private void AddOrbsOnProduct(int amount)
    {
        SaveAndLoadManager.SetIntValue(
                    SaveAndLoadManager.GetIntValue(SaveAndLoadManager.OrbsName) + amount, SaveAndLoadManager.OrbsName);
    }

    private void AddPowerUpOnProduct(int amount, PowerUpManager.PowerUpType powerUpType)
    {
        string powerUpName = SaveAndLoadManager.PowerUpPrefix + powerUpType;
        SaveAndLoadManager.SetIntValue(
                    SaveAndLoadManager.GetIntValue(powerUpName) + amount, powerUpName);
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
