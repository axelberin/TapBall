using UnityEngine;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("IronSource App Key")]
    [SerializeField] private string androidAppKey = "TU_APP_KEY_AQUI";
    [SerializeField] private string iosAppKey = "TU_APP_KEY_AQUI";

    private int _adsCounter;
    private string appKey;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        InitIronSource();
    }

    private void InitIronSource()
    {
#if UNITY_ANDROID
        appKey = androidAppKey;
#elif UNITY_IOS
        appKey = iosAppKey;
#endif

        // Inicializa IronSource con los tipos de anuncio que vas a usar
        IronSource.Agent.init(appKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL);

        // Opcional: valida que el SDK esté bien integrado
        IronSource.Agent.validateIntegration();

        Debug.Log("IronSource inicializado con appKey: " + appKey);
    }

    private void OnEnable()
    {
        // Rewarded Video
        IronSourceEvents.onRewardedVideoAdRewardedEvent += OnRewardedVideoAdRewarded;
        IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += OnRewardedVideoAvailabilityChanged;

        // Interstitial
        IronSourceEvents.onInterstitialAdReadyEvent += OnInterstitialReady;
        IronSourceEvents.onInterstitialAdLoadFailedEvent += OnInterstitialLoadFailed;

        // SDK Initialization
        IronSourceEvents.onSdkInitializationCompletedEvent += OnSdkInitialized;
    }

    private void OnDisable()
    {
        IronSourceEvents.onRewardedVideoAdRewardedEvent -= OnRewardedVideoAdRewarded;
        IronSourceEvents.onRewardedVideoAvailabilityChangedEvent -= OnRewardedVideoAvailabilityChanged;

        IronSourceEvents.onInterstitialAdReadyEvent -= OnInterstitialReady;
        IronSourceEvents.onInterstitialAdLoadFailedEvent -= OnInterstitialLoadFailed;

        IronSourceEvents.onSdkInitializationCompletedEvent -= OnSdkInitialized;
    }

    // === CALLBACKS ===

    private void OnSdkInitialized()
    {
        Debug.Log("IronSource SDK completamente inicializado");
    }

    private void OnRewardedVideoAvailabilityChanged(bool available)
    {
        Debug.Log("Rewarded Video disponible: " + available);
    }

    private void OnRewardedVideoAdRewarded(IronSourcePlacement placement)
    {
        Debug.Log("Jugador recibió recompensa: " + placement.getRewardName());
        // Aquí das la recompensa, por ejemplo:
        // GameManager.Instance.AddCoins(placement.getRewardAmount());
    }

    private void OnInterstitialReady()
    {
        Debug.Log("Interstitial listo para mostrarse");
    }

    private void OnInterstitialLoadFailed(IronSourceError error)
    {
        Debug.LogError("Fallo al cargar Interstitial: " + error.getDescription());
    }

    // === MÉTODOS PÚBLICOS PARA USAR DESDE TUS BOTONES ===

    public void ShowRewardedAd()
    {
        if (IronSource.Agent.isRewardedVideoAvailable())
        {
            IronSource.Agent.showRewardedVideo();
        }
        else
        {
            Debug.Log("Video recompensado no disponible");
        }
    }

    public void ShowInterstitialAd()
    {
        _adsCounter++;
        if (_adsCounter <= 3)
            return;

        if (IronSource.Agent.isInterstitialReady())
        {
            IronSource.Agent.showInterstitial();
        }
        else
        {
            Debug.Log("Interstitial no listo");
            IronSource.Agent.loadInterstitial(); // Puedes forzar recarga aquí si quieres
        }
    }
}
