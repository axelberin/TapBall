using UnityEngine;
using Unity.Services.LevelPlay;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    public string YOUR_APP_KEY = "tu_app_key_de_ironsource"; // App Key de IronSource
    public string REWARDED_AD_UNIT_ID = "DefaultRewarded";   // ID de Rewarded Video Ad Unit
    public string INTERSTITIAL_AD_UNIT_ID = "DefaultInterstitial"; //ID de Interstitial Ad Unit
    public string BANNER_AD_UNIT_ID = "DefaultBanner";     // ID de Banner Ad Unit

    // Eventos personalizados para tu juego (opcional pero recomendado)
    public static event System.Action OnAdFailedToLoad;
    public static event System.Action OnRewardedAdAvailable;
    public static event System.Action OnRewardedAdNotAvailable;
    public static event System.Action<LevelPlayAdInfo> OnRewardedAdFinished;
    public static event System.Action<string> OnInterstitialAdAvailable;
    public static event System.Action<string> OnInterstitialAdNotAvailable;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    void Start()
    {
        Debug.Log("Inicializando AdManager y escuchando eventos de IronSource...");
        // Asegúrate de reemplazar "YOUR_APP_KEY" con tu App Key real de IronSource
        if (string.IsNullOrEmpty(YOUR_APP_KEY) || YOUR_APP_KEY == "tu_app_key_de_ironsource")
        {
            Debug.LogError("IronSource App Key no configurada en AdManager.cs. ¡Por favor, reemplaza 'tu_app_key_de_ironsource'!");
            return;
        }

        // 1. Añadir Listeners de eventos
        // Rewarded Video
        //LevelPlay.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
        //LevelPlay.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent;
        //LevelPlay.onRewardedVideoAdAvailableEvent += RewardedVideoAdAvailableEvent;
        //LevelPlay.onRewardedVideoAdNotAvailableEvent += RewardedVideoAdNotAvailableEvent;
        //LevelPlay.onRewardedVideoAdShowFailedEvent += RewardedVideoAdShowFailedEvent;
        //LevelPlay.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
        //LevelPlay.onRewardedVideoAdClickedEvent += RewardedVideoAdClickedEvent;

        //// Interstitial
        //LevelPlay.onInterstitialAdReadyEvent += InterstitialAdReadyEvent;
        //LevelPlay.onInterstitialAdLoadFailedEvent += InterstitialAdLoadFailedEvent;
        //LevelPlay.onInterstitialAdOpenedEvent += InterstitialAdOpenedEvent;
        //LevelPlay.onInterstitialAdClickedEvent += InterstitialAdClickedEvent;
        //LevelPlay.onInterstitialAdClosedEvent += InterstitialAdClosedEvent;
        //LevelPlay.onInterstitialAdShowFailedEvent += InterstitialAdShowFailedEvent;

        //// Banner
        //LevelPlay.onBannerAdLoadedEvent += BannerAdLoadedEvent;
        //LevelPlay.onBannerAdLoadFailedEvent += BannerAdLoadFailedEvent;
        //LevelPlay.onBannerAdClickedEvent += BannerAdClickedEvent;
        //LevelPlay.onBannerAdScreenPresentedEvent += BannerAdScreenPresentedEvent;
        //LevelPlay.onBannerAdScreenDismissedEvent += BannerAdScreenDismissedEvent;
        //LevelPlay.onBannerAdLeftApplicationEvent += BannerAdLeftApplicationEvent;

        //// Impression Data (opcional, pero buena práctica para analíticas)
        //LevelPlay.onImpressionDataReadyEvent += ImpressionDataReadyEvent;

        // 2. Inicializar IronSource SDK
        // Define los tipos de anuncios que vas a usar al inicializar
        IronSource.Agent.init(YOUR_APP_KEY, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);

        // Opcional: Cargar los Interstitial y Rewarded Video al inicio para tenerlos listos
        IronSource.Agent.loadInterstitial();
        IronSource.Agent.loadRewardedVideo(); // Aunque IronSource precarga rewarded, esto asegura la primera carga.
    }

    #region Public Methods to Show Ads

    public bool IsRewardedAdReady()
    {
        return IronSource.Agent.isRewardedVideoAvailable();
    }

    public void ShowRewardedAd()
    {
        Debug.Log("Intentando mostrar Rewarded Video...");
        if (IronSource.Agent.isRewardedVideoAvailable())
        {
            IronSource.Agent.showRewardedVideo(REWARDED_AD_UNIT_ID);
        }
        else
        {
            Debug.LogWarning("Rewarded Video no está disponible.");
            OnRewardedAdNotAvailable?.Invoke(); // Dispara el evento personalizado
            // Puedes intentar cargar uno nuevo aquí si quieres
            IronSource.Agent.loadRewardedVideo();
        }
    }

    public bool IsInterstitialAdReady()
    {
        return IronSource.Agent.isInterstitialReady();
    }

    public void ShowInterstitialAd()
    {
        Debug.Log("Intentando mostrar Interstitial...");
        if (IronSource.Agent.isInterstitialReady())
        {
            IronSource.Agent.showInterstitial();
        }
        else
        {
            Debug.LogWarning("Interstitial no está disponible.");
            OnInterstitialAdNotAvailable?.Invoke("Interstitial no disponible."); // Dispara el evento
            // Puedes intentar cargar uno nuevo aquí si quieres
            IronSource.Agent.loadInterstitial();
        }
    }

    public void LoadBannerAd()
    {
        Debug.Log("Cargando Banner Ad...");
        // Define el tamaño y la posición del banner
        IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM, BANNER_AD_UNIT_ID);
    }

    public void ShowBannerAd()
    {
        Debug.Log("Mostrando Banner Ad...");
        IronSource.Agent.displayBanner();
    }

    public void HideBannerAd()
    {
        Debug.Log("Ocultando Banner Ad...");
        IronSource.Agent.hideBanner();
    }

    public void DestroyBannerAd()
    {
        Debug.Log("Destruyendo Banner Ad...");
        IronSource.Agent.destroyBanner();
    }

    #endregion

    #region IronSource Rewarded Video Listener Events

    void RewardedVideoAdOpenedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Rewarded Video Ad Opened: " + adInfo.AdUnitId);
    }

    void RewardedVideoAdClosedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Rewarded Video Ad Closed: " + adInfo.AdUnitId);
        // Cargar un nuevo Rewarded Video después de que uno se cierra
        IronSource.Agent.loadRewardedVideo();
    }

    void RewardedVideoAdAvailableEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Rewarded Video Ad Available: " + adInfo.AdUnitId);
        OnRewardedAdAvailable?.Invoke(); // Dispara el evento personalizado
    }

    void RewardedVideoAdNotAvailableEvent()
    {
        Debug.Log("Rewarded Video Ad Not Available");
        OnRewardedAdNotAvailable?.Invoke(); // Dispara el evento personalizado
    }

    void RewardedVideoAdShowFailedEvent(LevelPlayAdError error, LevelPlayAdInfo adInfo)
    {
        Debug.LogError("Rewarded Video Show Failed: " + error.ErrorMessage + " (AdUnit: " + adInfo.AdUnitId + ")");
        OnAdFailedToLoad?.Invoke(); // Dispara el evento personalizado
        // Intenta cargar un nuevo Rewarded Video si falla la muestra
        IronSource.Agent.loadRewardedVideo();
    }

    void RewardedVideoAdRewardedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("User rewarded! Amount: " + adInfo.Revenue + ", Item: " + adInfo.AdUnitName);
        OnRewardedAdFinished?.Invoke(adInfo); // Dispara el evento personalizado con los detalles del anuncio
    }

    void RewardedVideoAdClickedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Rewarded Video Ad Clicked: " + adInfo.AdUnitId);
    }

    #endregion

    #region IronSource Interstitial Listener Events

    void InterstitialAdReadyEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Interstitial Ad Ready: " + adInfo.AdUnitId);
        OnInterstitialAdAvailable?.Invoke(adInfo.AdUnitId);
    }

    void InterstitialAdLoadFailedEvent(LevelPlayAdError error)
    {
        Debug.LogError("Interstitial Ad Load Failed: " + error.ErrorMessage);
        OnAdFailedToLoad?.Invoke(); // Dispara el evento
    }

    void InterstitialAdOpenedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Interstitial Ad Opened: " + adInfo.AdUnitId);
    }

    void InterstitialAdClickedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Interstitial Ad Clicked: " + adInfo.AdUnitId);
    }

    void InterstitialAdClosedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Interstitial Ad Closed: " + adInfo.AdUnitId);
        // Cargar un nuevo Interstitial después de que uno se cierra
        IronSource.Agent.loadInterstitial();
    }

    void InterstitialAdShowFailedEvent(LevelPlayAdError error, LevelPlayAdInfo adInfo)
    {
        Debug.LogError("Interstitial Show Failed: " + error.ErrorMessage + " (AdUnit: " + adInfo.AdUnitId + ")");
        OnAdFailedToLoad?.Invoke(); // Dispara el evento
        // Intenta cargar un nuevo Interstitial si falla la muestra
        IronSource.Agent.loadInterstitial();
    }

    #endregion

    #region IronSource Banner Listener Events

    void BannerAdLoadedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Banner Ad Loaded: " + adInfo.AdUnitId);
        // Puedes optar por mostrar el banner automáticamente aquí
        // IronSource.Agent.displayBanner();
    }

    void BannerAdLoadFailedEvent(IronSourceError error)
    {
        Debug.LogError("Banner Ad Load Failed: " + error.getDescription());
        OnAdFailedToLoad?.Invoke(); // Dispara el evento
    }

    void BannerAdClickedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Banner Ad Clicked: " + adInfo.AdUnitId);
    }

    void BannerAdScreenPresentedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Banner Ad Screen Presented: " + adInfo.AdUnitId);
    }

    void BannerAdScreenDismissedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Banner Ad Screen Dismissed: " + adInfo.AdUnitId);
    }

    void BannerAdLeftApplicationEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Banner Ad Left Application: " + adInfo.AdUnitId);
    }

    #endregion

    #region Impression Data Listener

    // Este callback te da información detallada sobre el anuncio que se mostró.
    // Muy útil para analíticas y depuración.
    void ImpressionDataReadyEvent(IronSourceImpressionData impressionData)
    {
        if (impressionData != null)
        {
            Debug.Log("ImpressionData: " + impressionData.allData);
            // Puedes acceder a campos específicos como:
            // string adUnit = impressionData.adUnit;
            // string network = impressionData.adNetwork;
            // double revenue = impressionData.revenue ?? 0;
            // string country = impressionData.country;
            // etc.
        }
    }

    #endregion

    // Asegúrate de que los eventos de IronSource se desregistren al destruir el objeto
    void OnDestroy()
    {
        //IronSourceEvents.onRewardedVideoAdOpenedEvent -= RewardedVideoAdOpenedEvent;
        //IronSourceEvents.onRewardedVideoAdClosedEvent -= RewardedVideoAdClosedEvent;
        //IronSourceEvents.onRewardedVideoAdAvailableEvent -= RewardedVideoAdAvailableEvent;
        //IronSourceEvents.onRewardedVideoAdNotAvailableEvent -= RewardedVideoAdNotAvailableEvent;
        //IronSourceEvents.onRewardedVideoAdShowFailedEvent -= RewardedVideoAdShowFailedEvent;
        //IronSourceEvents.onRewardedVideoAdRewardedEvent -= RewardedVideoAdRewardedEvent;
        //IronSourceEvents.onRewardedVideoAdClickedEvent -= RewardedVideoAdClickedEvent;

        //IronSourceEvents.onInterstitialAdReadyEvent -= InterstitialAdReadyEvent;
        //IronSourceEvents.onInterstitialAdLoadFailedEvent -= InterstitialAdLoadFailedEvent;
        //IronSourceEvents.onInterstitialAdOpenedEvent -= InterstitialAdOpenedEvent;
        //IronSourceEvents.onInterstitialAdClickedEvent -= InterstitialAdClickedEvent;
        //IronSourceEvents.onInterstitialAdClosedEvent -= InterstitialAdClosedEvent;
        //IronSourceEvents.onInterstitialAdShowFailedEvent -= InterstitialAdShowFailedEvent;

        //IronSourceEvents.onBannerAdLoadedEvent -= BannerAdLoadedEvent;
        //IronSourceEvents.onBannerAdLoadFailedEvent -= BannerAdLoadFailedEvent;
        //IronSourceEvents.onBannerAdClickedEvent -= BannerAdClickedEvent;
        //IronSourceEvents.onBannerAdScreenPresentedEvent -= BannerAdScreenPresentedEvent;
        //IronSourceEvents.onBannerAdScreenDismissedEvent -= BannerAdScreenDismissedEvent;
        //IronSourceEvents.onBannerAdLeftApplicationEvent -= BannerAdLeftApplicationEvent;

        //IronSourceEvents.onImpressionDataReadyEvent -= ImpressionDataReadyEvent;

        // También destruye cualquier banner activo si lo hay
        //IronSource.Agent.destroyBanner();
    }
}