using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : MonoBehaviour
{
    private static IStoreController m_StoreController;
    private static IExtensionProvider m_StoreExtensionProvider;

    public const string TEST_PRODUCT = "test.product";
    public const string PRODUCT_200_COINS = "com.miempresa.migame.monedas200";

    private bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

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

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureReason)
    {
        Debug.Log($"OnPurchaseFailed: FAIL. Product: {product.definition.storeSpecificId}, Reason: {failureReason}");
    }

    public void OnProductFetched(Product product)
    {
        Debug.Log($"OnProductFetched: PASS. Product: {product.definition.storeSpecificId}");
    }
}