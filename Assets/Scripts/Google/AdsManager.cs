using System;
using UnityEngine;
using Unity.Services.LevelPlay;

public class AdsManager : MonoBehaviour
{
    [Header("Ad Unit IDs")]
    [SerializeField] private string appId = "";
    [SerializeField] private string interstitialAdUnitId = "";
    [SerializeField] private string rewardedAdUnitId = "";

    [Header("Ad Settings")]
    [SerializeField] private float interstitialCooldown = 30f;

    private LevelPlayInterstitialAd interstitialAd;
    private LevelPlayRewardedAd rewardedAd;
    private float lastInterstitialTime;

    private bool isInterstitialReady;
    private bool isRewardedReady;

    private Action onInterstitialCompleted, onInterstitialFailed;
    private Action onRewardedCompleted, onRewardedFailed;

    public static AdsManager Instance { get; private set; }
    public event Action OnInterstitialReady;
    public event Action OnRewardedReady;
    public event Action<string> OnAdError;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitLevelPlay();
        }
        else Destroy(gameObject);
    }

    private void InitLevelPlay()
    {
        com.unity3d.mediation.LevelPlayAdFormat[] formats = new[] {
        com.unity3d.mediation.LevelPlayAdFormat.INTERSTITIAL,
        com.unity3d.mediation.LevelPlayAdFormat.REWARDED
    };

        LevelPlay.OnInitSuccess += config =>
        {
            Debug.Log("LevelPlay inicializado correctamente");
            // tu creación de anuncios...
        };

        LevelPlay.OnInitFailed += error =>
        {
            Debug.LogError($"Init failed: {error.ErrorMessage}");
            OnAdError?.Invoke(error.ErrorMessage);
        };

        LevelPlay.Init(appId, null, formats);

        Debug.Log("Inicializando LevelPlay con formatos especificados...");

        interstitialAd = new LevelPlayInterstitialAd(interstitialAdUnitId);
        rewardedAd = new LevelPlayRewardedAd(rewardedAdUnitId);

        // Intersticial
        interstitialAd.OnAdLoaded += _ => { isInterstitialReady = true; OnInterstitialReady?.Invoke(); };
        interstitialAd.OnAdLoadFailed += e => { isInterstitialReady = false; OnAdError?.Invoke(e.ErrorMessage); Invoke(nameof(LoadInterstitialAd), 5f); };
        interstitialAd.OnAdDisplayed += _ => Time.timeScale = 0f;
        interstitialAd.OnAdDisplayFailed += e => { Time.timeScale = 1f; onInterstitialFailed?.Invoke(); OnAdError?.Invoke(e.LevelPlayError.ErrorMessage); ClearInterstitial(); LoadInterstitialAd(); };
        interstitialAd.OnAdClosed += _ => { Time.timeScale = 1f; isInterstitialReady = false; lastInterstitialTime = Time.time; onInterstitialCompleted?.Invoke(); ClearInterstitial(); LoadInterstitialAd(); };

        // Recompensado
        rewardedAd.OnAdLoaded += _ => { isRewardedReady = true; OnRewardedReady?.Invoke(); };
        rewardedAd.OnAdLoadFailed += e => { isRewardedReady = false; OnAdError?.Invoke(e.ErrorMessage); Invoke(nameof(LoadRewardedAd), 5f); };
        rewardedAd.OnAdDisplayed += _ => Time.timeScale = 0f;
        rewardedAd.OnAdDisplayFailed += e => { Time.timeScale = 1f; onRewardedFailed?.Invoke(); OnAdError?.Invoke(e.LevelPlayError.ErrorMessage); ClearRewarded(); LoadRewardedAd(); };
        rewardedAd.OnAdClosed += _ => { Time.timeScale = 1f; isRewardedReady = false; LoadRewardedAd(); };
        rewardedAd.OnAdRewarded += OnRewardedAdCompleted;

        LoadInterstitialAd();
        LoadRewardedAd();
    }

    public void LoadInterstitialAd()
    {
        if (!interstitialAd.IsAdReady())
        {
            Debug.Log("Load interstitial...");
            interstitialAd.LoadAd();
        }
    }

    public void ShowInterstitialAd(Action onCompleted = null, Action onFailed = null)
    {
        if (SaveAndLoadManager.ContainsKey(SaveAndLoadManager.NoAdsBougthName) ||
            SaveAndLoadManager.GetIntValue(SaveAndLoadManager.NoAdsBougthName) == 1)
            return;

        if (Time.time - lastInterstitialTime < interstitialCooldown)
        {
            Debug.Log($"Interstitial cooldown {interstitialCooldown - (Time.time - lastInterstitialTime):F1}s");
            onFailed?.Invoke(); return;
        }
        if (!isInterstitialReady)
        {
            Debug.LogWarning("Interstitial not ready");
            LoadInterstitialAd();
            onFailed?.Invoke();
            return;
        }

        onInterstitialCompleted = onCompleted;
        onInterstitialFailed = onFailed;

        Debug.Log("Show interstitial");
        interstitialAd.ShowAd();
    }

    public bool IsInterstitialReady() => isInterstitialReady && Time.time - lastInterstitialTime >= interstitialCooldown;

    void ClearInterstitial()
    {
        onInterstitialCompleted = onInterstitialFailed = null;
    }

    public void LoadRewardedAd()
    {
        if (!rewardedAd.IsAdReady())
        {
            Debug.Log("Load rewarded...");
            rewardedAd.LoadAd();
        }
    }

    public void ShowRewardedAd(Action onCompleted, Action onFailed = null)
    {
        if (!isRewardedReady)
        {
            Debug.LogWarning("Rewarded not ready");
            LoadRewardedAd();
            onFailed?.Invoke();
            return;
        }

        onRewardedCompleted = onCompleted;
        onRewardedFailed = onFailed;

        Debug.Log("Show rewarded");
        rewardedAd.ShowAd();
    }

    private void OnRewardedAdCompleted(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        Debug.Log("Rewarded ad completed successfully.");
        isRewardedReady = false;
        LoadRewardedAd();

        Time.timeScale = 1f;
        //onRewardedAdClosed?.Invoke();
        onRewardedCompleted?.Invoke();
    }

    public bool IsRewardedReady() => isRewardedReady;

    void ClearRewarded()
    {
        onRewardedCompleted = onRewardedFailed = null;
    }

    public string GetAdStatus()
    {
        var cd = Mathf.Max(0, interstitialCooldown - (Time.time - lastInterstitialTime));
        return $"Interstitial: {(isInterstitialReady ? "ready" : "no")} | Rewarded: {(isRewardedReady ? "ready" : "no")} | Cooldown: {cd:F1}s";
    }
}
