using UnityEngine;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("IronSource App Key")]
    [SerializeField] private string androidAppKey = "TU_APP_KEY_AQUI";
    [SerializeField] private string iosAppKey = "TU_APP_KEY_AQUI";

    private int _adsCounter;
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
        IronSource.Agent.init(appKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL);

        // Opcional: valida integración
        IronSource.Agent.validateIntegration();

        Debug.Log("IronSource inicializado con appKey: " + appKey);
    }

    private void OnEnable()
    {
        // SDK Initialization
        IronSourceEvents.onSdkInitializationCompletedEvent += OnSdkInitialized;

        // Rewarded Video
        IronSourceEvents.onRewardedVideoAdRewardedEvent += OnRewardedVideoAdRewarded;
        IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += OnRewardedVideoAvailabilityChanged;

        // Interstitial - EVENTOS COMPLETOS
        IronSourceEvents.onInterstitialAdReadyEvent += OnInterstitialReady;
        IronSourceEvents.onInterstitialAdLoadFailedEvent += OnInterstitialLoadFailed;
        IronSourceEvents.onInterstitialAdShowSucceededEvent += OnInterstitialShowSucceeded;
        IronSourceEvents.onInterstitialAdShowFailedEvent += OnInterstitialShowFailed;
        IronSourceEvents.onInterstitialAdClickedEvent += OnInterstitialClicked;
        IronSourceEvents.onInterstitialAdClosedEvent += OnInterstitialClosed;
    }

    private void OnDisable()
    {
        // SDK
        IronSourceEvents.onSdkInitializationCompletedEvent -= OnSdkInitialized;

        // Rewarded
        IronSourceEvents.onRewardedVideoAdRewardedEvent -= OnRewardedVideoAdRewarded;
        IronSourceEvents.onRewardedVideoAvailabilityChangedEvent -= OnRewardedVideoAvailabilityChanged;

        // Interstitial
        IronSourceEvents.onInterstitialAdReadyEvent -= OnInterstitialReady;
        IronSourceEvents.onInterstitialAdLoadFailedEvent -= OnInterstitialLoadFailed;
        IronSourceEvents.onInterstitialAdShowSucceededEvent -= OnInterstitialShowSucceeded;
        IronSourceEvents.onInterstitialAdShowFailedEvent -= OnInterstitialShowFailed;
        IronSourceEvents.onInterstitialAdClickedEvent -= OnInterstitialClicked;
        IronSourceEvents.onInterstitialAdClosedEvent -= OnInterstitialClosed;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) ForceShowInterstitial();
    }

    // === CALLBACKS SDK ===
    private void OnSdkInitialized()
    {
        Debug.Log("IronSource SDK completamente inicializado");
        isInitialized = true;

        // Precarga el primer interstitial
        LoadInterstitial();
    }

    // === CALLBACKS REWARDED ===
    private void OnRewardedVideoAvailabilityChanged(bool available)
    {
        Debug.Log("Rewarded Video disponible: " + available);
    }

    private void OnRewardedVideoAdRewarded(IronSourcePlacement placement)
    {
        Debug.Log("Jugador recibió recompensa: " + placement.getRewardName());
        // Aquí das la recompensa
    }

    // === CALLBACKS INTERSTITIAL ===
    private void OnInterstitialReady()
    {
        Debug.Log("Interstitial listo para mostrarse");
    }

    private void OnInterstitialLoadFailed(IronSourceError error)
    {
        Debug.LogError("Fallo al cargar Interstitial: " + error.getDescription());

        // Reintenta cargar después de 30 segundos
        Invoke(nameof(LoadInterstitial), 30f);
    }

    private void OnInterstitialShowSucceeded()
    {
        Debug.Log("Interstitial mostrado exitosamente");
    }

    private void OnInterstitialShowFailed(IronSourceError error)
    {
        Debug.LogError("Fallo al mostrar Interstitial: " + error.getDescription());

        // Recarga inmediatamente si falla al mostrar
        LoadInterstitial();
    }

    private void OnInterstitialClicked()
    {
        Debug.Log("Usuario hizo click en Interstitial");
    }

    private void OnInterstitialClosed()
    {
        Debug.Log("Interstitial cerrado");

        // Resetea el contador y precarga el siguiente
        LoadInterstitial();
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

        _adsCounter++;
        if (_adsCounter < 3)
            return;

        IronSource.Agent.showInterstitial();
        _adsCounter = 0;
    }

    // Método para testing
    public void ForceShowInterstitial()
    {
        if (IronSource.Agent.isInterstitialReady())
            IronSource.Agent.showInterstitial();
        else
            LoadInterstitial();
    }

    // Información de debug
    public void LogAdStatus()
    {
        Debug.Log($"SDK Inicializado: {isInitialized}");
        Debug.Log($"Interstitial listo: {IronSource.Agent.isInterstitialReady()}");
        Debug.Log($"Rewarded disponible: {IronSource.Agent.isRewardedVideoAvailable()}");
        Debug.Log($"Contador ads: {_adsCounter}");
    }

    // Para pausar/reanudar cuando el juego pierde/gana foco
    private void OnApplicationPause(bool pauseStatus)
    {
        if (isInitialized)
        {
            IronSource.Agent.onApplicationPause(pauseStatus);
        }
    }
}