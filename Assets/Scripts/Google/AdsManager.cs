using UnityEngine;
using UnityEngine.Events;
using Unity.Services.LevelPlay;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("IronSource App Key")]
    [SerializeField] private string androidAppKey = "TU_APP_KEY_AQUI";
    [SerializeField] private string iosAppKey = "TU_APP_KEY_AQUI";

    private string appKey;
    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitIronSource();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitIronSource()
    {
#if UNITY_ANDROID
        appKey = androidAppKey;
#elif UNITY_IOS
        appKey = iosAppKey;
#else
        Debug.LogWarning("Plataforma no soportada para IronSource");
        return;
#endif

        if (string.IsNullOrEmpty(appKey) || appKey == "TU_APP_KEY_AQUI")
        {
            Debug.LogError("App Key no configurada correctamente!");
            return;
        }

        // Inicializa IronSource
        //IronSource.Agent.setSdkInitializationListener(this);
        //IronSource.Agent.init(appKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL);

        //// Opcional: valida integración
        //IronSource.Agent.validateIntegration();

        //// Listeners
        //IronSource.Agent.setRewardedVideoAdListener(this);
        //IronSource.Agent.setInterstitialAdListener(this);

        Debug.Log("IronSource inicializado con appKey: " + appKey);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) ForceShowInterstitial();
    }

    // === CALLBACKS SDK ===
    public void onSdkInitializationCompleted()
    {
        Debug.Log("IronSource SDK completamente inicializado");
        isInitialized = true;

        LoadInterstitial();
    }

    // === CALLBACKS REWARDED VIDEO ===
    public void onAdAvailable(IronSourceAdInfo adInfo)
    {
        Debug.Log("Rewarded Video disponible.");
    }

    public void onAdUnavailable()
    {
        Debug.Log("Rewarded Video no disponible.");
    }

    public void onAdRewarded(IronSourcePlacement placement, IronSourceAdInfo adInfo)
    {
        Debug.Log("Jugador recibió recompensa: " + placement.getRewardName());
        // Aquí das la recompensa
    }

    public void onAdShowFailed(IronSourceError error, IronSourceAdInfo adInfo) { }
    public void onAdOpened(IronSourceAdInfo adInfo) { }
    public void onAdClosed(IronSourceAdInfo adInfo) { }
    public void onAdClicked(IronSourcePlacement placement, IronSourceAdInfo adInfo) { }
    public void onAdStarted(IronSourceAdInfo adInfo) { }
    public void onAdEnded(IronSourceAdInfo adInfo) { }

    // === CALLBACKS INTERSTITIAL ===
    public void onAdReady(IronSourceAdInfo adInfo)
    {
        Debug.Log("Interstitial listo para mostrarse");
    }

    public void onAdLoadFailed(IronSourceError error)
    {
        Debug.LogError("Fallo al cargar Interstitial: " + error.getDescription());
        Invoke(nameof(LoadInterstitial), 30f);
    }

    public void onAdShowSucceeded(IronSourceAdInfo adInfo)
    {
        Debug.Log("Interstitial mostrado exitosamente");
    }

    public void onAdClicked(IronSourceAdInfo adInfo)
    {
        Debug.Log("Usuario hizo click en Interstitial");
    }

    // === MÉTODOS PRIVADOS ===
    private void LoadInterstitial()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("SDK no inicializado aún");
            return;
        }

        Debug.Log("Cargando Interstitial...");
        IronSource.Agent.loadInterstitial();
    }

    // === MÉTODOS PÚBLICOS ===
    public void ShowRewardedAd()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("SDK no inicializado");
            return;
        }

        if (IronSource.Agent.isRewardedVideoAvailable())
            IronSource.Agent.showRewardedVideo();
        else
            Debug.Log("Video recompensado no disponible");
    }

    public void ShowInterstitialAd()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("SDK no inicializado");
            return;
        }

        if (!IronSource.Agent.isInterstitialReady())
            LoadInterstitial();
        else
            IronSource.Agent.showInterstitial();
    }

    public void ForceShowInterstitial()
    {
        if (IronSource.Agent.isInterstitialReady())
            IronSource.Agent.showInterstitial();
        else
            LoadInterstitial();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (isInitialized)
        {
            IronSource.Agent.onApplicationPause(pauseStatus);
        }
    }
}
