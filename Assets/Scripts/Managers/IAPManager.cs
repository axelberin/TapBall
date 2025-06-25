using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour, IStoreListener
{
    private static IStoreController m_StoreController; // The Unity Purchasing system.
    private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

    // IDs of your products
    public const string TEST_PRODUCT = "test.product";
    public const string PRODUCT_200_COINS = "com.miempresa.migame.monedas200";

    void Start()
    {
        if (m_StoreController == null)
        {
            InitializePurchasing();
        }
    }

    public void InitializePurchasing()
    {
        if (IsInitialized())
        {
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Add your products here
        builder.AddProduct(TEST_PRODUCT, ProductType.NonConsumable);
        // For subscriptions: builder.AddProduct(PRODUCT_SUBSCRIPTION, ProductType.Subscription, new IDs(){{PRODUCT_SUBSCRIPTION, GooglePlay.Name}});

        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    // Call this method when a user wants to buy a product
    public void Buy100Coins()
    {
        BuyProductID(TEST_PRODUCT);
    }

    public void BuyProductID(string productId)
    {
        if (IsInitialized())
        {
            Product product = m_StoreController.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                Debug.Log($"Purchasing product: {product.transactionID}");
                m_StoreController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log("BuyProductID: FAIL. Not initialized or product not found/available.");
            }
        }
        else
        {
            Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }

    //
    // --- IStoreListener Implementation ---
    //

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: PASS");
        m_StoreController = controller;
        m_StoreExtensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log($"OnInitializeFailed InitializationFailureReason:{error}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        // A consumable product has been purchased by this user.
        if (string.Equals(args.purchasedProduct.definition.id, TEST_PRODUCT, System.StringComparison.Ordinal))
        {
            Debug.Log($"ProcessPurchase: PASS. Product: {args.purchasedProduct.definition.id}");
            // Grant the user their coins
            Debug.Log("Granting 100 coins!");
            // TODO: Add coins to player's balance
        }
        else if (string.Equals(args.purchasedProduct.definition.id, PRODUCT_200_COINS, System.StringComparison.Ordinal))
        {
            Debug.Log($"ProcessPurchase: PASS. Product: {args.purchasedProduct.definition.id}");
            // Grant the user their coins
            Debug.Log("Granting 200 coins!");
            // TODO: Add coins to player's balance
        }
        // This is where you would process other product types (non-consumable, subscription)
        else
        {
            Debug.Log($"ProcessPurchase: FAIL. Unrecognized product: {args.purchasedProduct.definition.id}");
        }

        // Return a flag indicating whether this product has completely been received, or if the application should
        // re-attempt to receive it later.
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log($"OnPurchaseFailed: FAIL. Product: {product.definition.storeSpecificId}, Reason: {failureReason}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log($"OnInitializeFailed InitializationFailureReason: {error}, Message: {message}");
    }
}