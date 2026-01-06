using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour
{
    public static IAPManager Instance { get; private set; }

    public Action OnCompletePurchase = delegate { };

    private StoreController m_StoreController;
    private readonly Dictionary<string, Product> _productsById = new();

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

    private bool _initializing;

    void Awake()
    {
        if (Instance == null) 
            Instance = this;
        else 
            Destroy(this);
    }

    async void Start()
    {
        await InitializeIAP_V5();
    }

    private async Task InitializeIAP_V5()
    {
        if (_initializing) return;
        if (m_StoreController != null) return;

        _initializing = true;

        try
        {
            m_StoreController = UnityIAPServices.StoreController();

            // Eventos recomendados en v5
            m_StoreController.OnProductsFetched += OnProductsFetched;
            m_StoreController.OnProductsFetchFailed += OnProductsFetchFailed;

            m_StoreController.OnPurchasesFetched += OnPurchasesFetched;
            m_StoreController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;

            m_StoreController.OnPurchasePending += OnPurchasePending;
            m_StoreController.OnPurchaseFailed += OnPurchaseFailed;
            m_StoreController.OnStoreDisconnected += OnStoreDisconnected;

            Debug.Log("[IAP] Connecting to store...");
            await m_StoreController.Connect();
            Debug.Log("[IAP] Store connected. Fetching products...");

            // Definís productos acá
            var initialProductsToFetch = new List<ProductDefinition>
            {
                new(PRODUCT_NO_ADS, ProductType.NonConsumable),
                new(FIFTY_GOLD, ProductType.Consumable),
                new(ONE_HUNDRED_GOLD, ProductType.Consumable),
                new(TEN_ORBS, ProductType.Consumable),
                new(FIFTY_ORBS, ProductType.Consumable),
                new(ONE_HUNDRED_ORBS, ProductType.Consumable),
                new(ORBITAL_PACK, ProductType.Consumable),
                new(GALACTIC_PACK, ProductType.Consumable),
                new(MULTIVERSAL_PACK, ProductType.Consumable),
                new(ICE_PACK, ProductType.Consumable),
                new(STONE_PACK, ProductType.Consumable),
                new(PROTECTION_PACK, ProductType.Consumable),
                new(INMORTAL_PACK, ProductType.Consumable),
            };

            m_StoreController.FetchProducts(initialProductsToFetch); // v5: FetchProducts :contentReference[oaicite:2]{index=2}
        }
        catch (Exception e)
        {
            Debug.LogError("[IAP] InitializeIAP_V5 exception: " + e);
        }
        finally
        {
            _initializing = false;
        }
    }

    private void OnProductsFetched(List<Product> products)
    {
        _productsById.Clear();

        foreach (var p in products)
        {
            if (p?.definition?.id == null) continue;
            _productsById[p.definition.id] = p;
            Debug.Log($"[IAP] Product fetched: {p.definition.id} | available={p.availableToPurchase} | price={p.metadata?.localizedPriceString}");
        }

        // Después de productos, fetch de compras (restore/entitlements)
        m_StoreController.FetchPurchases(); // v5: FetchPurchases :contentReference[oaicite:3]{index=3}
    }

    private void OnProductsFetchFailed(ProductFetchFailed failed)
    {
        Debug.LogError($"[IAP] Products fetch failed: {failed}");
    }

    private void OnPurchasesFetched(Orders orders)
    {
        // Consumibles: el store no “restaura” consumibles consumidos.
        // No-consumibles: acá podrías chequear entitlements si querés.
        Debug.Log("[IAP] Purchases fetched.");
    }

    private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
    {
        Debug.LogError($"[IAP] Purchases fetch failed: {failure}");
    }

    private void OnStoreDisconnected(StoreConnectionFailureDescription failure)
    {
        Debug.LogError($"[IAP] Store disconnected: {failure}");
    }

    private bool IsInitialized()
    {
        return m_StoreController != null && _productsById.Count > 0;
    }

    public void BuyProductID(string productId)
    {
        Debug.Log($"[IAP] CLICK -> BuyProductID({productId})");

        if (!IsInitialized())
        {
            Debug.LogError("[IAP] BuyProductID FAIL. Not initialized (products not fetched yet).");
            return;
        }

        if (!_productsById.TryGetValue(productId, out var product) || product == null)
        {
            Debug.LogError($"[IAP] Product not found in fetched list: {productId}");
            Debug.Log($"[IAP] Fetched products: {string.Join(", ", _productsById.Keys)}");
            return;
        }

        Debug.Log($"[IAP] Found: {product.definition.id} | available={product.availableToPurchase}");

        if (!product.availableToPurchase)
        {
            Debug.LogError($"[IAP] NOT availableToPurchase: {product.definition.id}");
            return;
        }

        Debug.Log($"[IAP] PurchaseProduct -> {product.definition.id}");
        m_StoreController.PurchaseProduct(product.definition.id); // v5 purchase flow :contentReference[oaicite:4]{index=4}
    }

    public Product GetProductByID(string id)
    {
        if (!IsInitialized())
        {
            Debug.LogWarning($"[IAP] GetProductByID({id}) llamado antes de inicializar.");
            return null;
        }

        if (_productsById.TryGetValue(id, out var product))
            return product;

        Debug.LogError($"[IAP] Producto no encontrado en la cache: {id}");
        return null;
    }

    public string GetProductPriceById(string id)
    {
        var product = GetProductByID(id);
        return product != null && product.metadata != null ? product.metadata.localizedPriceString : "-";
    }

    private void OnPurchasePending(PendingOrder pendingOrder)
    {
        try
        {
            var firstItem = pendingOrder.CartOrdered.Items().FirstOrDefault();
            var productId = firstItem?.Product?.definition?.id;

            if (string.IsNullOrEmpty(productId))
            {
                Debug.LogError("[IAP] PendingOrder without product id.");
                return;
            }

            Debug.Log($"[IAP] OnPurchasePending: {productId}");

            GetRewardAfterProcessPurchase(productId);

            m_StoreController.ConfirmPurchase(pendingOrder); 
            Debug.Log($"[IAP] ConfirmPurchase OK: {productId}");
        }
        catch (Exception e)
        {
            Debug.LogError("[IAP] OnPurchasePending exception: " + e);
        }
    }

    private void OnPurchaseFailed(FailedOrder failedOrder)
    {
        try
        {
            var firstItem = failedOrder.CartOrdered.Items().FirstOrDefault();
            var productId = firstItem?.Product?.definition?.id ?? "(unknown)";
            Debug.LogError($"[IAP] OnPurchaseFailed: {productId} | reason={failedOrder.FailureReason} | details={failedOrder.Details}");
        }
        catch (Exception e)
        {
            Debug.LogError("[IAP] OnPurchaseFailed exception: " + e);
        }
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
            case FIFTY_GOLD: AddCoinsOnProduct(50); break;
            case ONE_HUNDRED_GOLD: AddCoinsOnProduct(100); break;
            case TEN_ORBS: AddOrbsOnProduct(10); break;
            case FIFTY_ORBS: AddOrbsOnProduct(50); break;
            case ONE_HUNDRED_ORBS: AddOrbsOnProduct(100); break;
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
            SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName) + amount,
            SaveAndLoadManager.CoinsName);
    }

    private void AddOrbsOnProduct(int amount)
    {
        SaveAndLoadManager.SetIntValue(
            SaveAndLoadManager.GetIntValue(SaveAndLoadManager.OrbsName) + amount,
            SaveAndLoadManager.OrbsName);
    }

    private void AddPowerUpOnProduct(int amount, PowerUpManager.PowerUpType powerUpType)
    {
        string powerUpName = SaveAndLoadManager.PowerUpPrefix + powerUpType;
        SaveAndLoadManager.SetIntValue(
            SaveAndLoadManager.GetIntValue(powerUpName) + amount,
            powerUpName);
    }
}
