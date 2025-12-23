using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityAddressables;

public class MenuManagerCanvas : CanvasElementLocator
{
    public static MenuManagerCanvas Instance;

    private GameObject _menuPanel;
    private GameObject _levelsPanel;
    private GameObject _dailyMissionsPanel;
    private GameObject _dailyRewardsPanel;
    private GameObject _downPanel;

    private TextMeshProUGUI _coinsText;
    private TextMeshProUGUI _orbsText;
    private TextMeshProUGUI _modeText;
    private PopUp _unlockSkinsPopUp;
    private PopUp _thanksForBuyPopUp;
    private PopUp _unlockModePopUp;
    private Button _levelsCloseButton;
    private Image _achievementImageInLevelsSelector = null;
    private GameObject _levelsSelectorHasNotifications;
    private GameObject _missionsButtonHasNotifications;
    private TextMeshProUGUI _missionsButtonsTextNotification;

    // Mundo actual - temporal hasta que implementes el sistema completo
    private string _currentWorld = "Neon";

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    void Start()
    {
        if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CurrentModeName))
            SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.CurrentModeName, true, true);

        GameManager.Instance.SelectGameMode(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CurrentModeName));

        _menuPanel = FindAndValidateGameObjectComponent(transform, "MenuPanel");
        _levelsPanel = FindAndValidateGameObjectComponent(transform, "LevelsPanel");
        _dailyMissionsPanel = FindAndValidateGameObjectComponent(transform, "DailyQuestsPanel");
        _downPanel = FindAndValidateGameObjectComponent(transform, "DownPanel");
        _dailyRewardsPanel = FindAndValidateGameObjectComponent(transform, "DailyRewardsPanel");

        var levelsSelectorButton = FindAndValidateComponent<Button>(transform, "LevelsSelectorButton");
        levelsSelectorButton.onClick.AddListener(() =>
        {
            _levelsPanel.SetActive(true);
            _menuPanel.SetActive(false);
            OnLevelSelectorClicked(false);
        });

        var dailyQuestBTN = FindAndValidateComponent<Button>(transform, "DialyQuestBTN");
        dailyQuestBTN.onClick.AddListener(() =>
        {
            _dailyMissionsPanel.SetActive(true);
            UIManager.Instance.SetText(_missionsButtonsTextNotification,
                DailyMissionsManager.Instance.GetAlMissionsCompletedCount().ToString(), true);
            _downPanel.SetActive(false);
        });
        var dailyQuestsBackButton = FindAndValidateComponent<Button>(transform, "DailyQuestsBackButton");
        dailyQuestsBackButton.onClick.AddListener(() =>
        {
            _dailyMissionsPanel.SetActive(false);
            UIManager.Instance.SetText(_missionsButtonsTextNotification,
                DailyMissionsManager.Instance.GetAlMissionsCompletedCount().ToString(), true);
            _downPanel.SetActive(true);
        });

        var dailyRewardBTN = FindAndValidateComponent<Button>(transform, "DailyRewardBTN");
        dailyRewardBTN.onClick.AddListener(() =>
        {
            _dailyRewardsPanel.SetActive(true);
            _downPanel.SetActive(false);
        });

        var dailyBackRewardBTN = FindAndValidateComponent<Button>(transform, "DailyRewardsBackButton");
        dailyBackRewardBTN.onClick.AddListener(() =>
        {
            _dailyRewardsPanel.SetActive(false);
            _downPanel.SetActive(true);
        });
        _coinsText = FindAndValidateComponent<TextMeshProUGUI>(transform, "CoinsText");
        _orbsText = FindAndValidateComponent<TextMeshProUGUI>(transform, "OrbsText");
        UpdateTexts();

        if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CurrentBallSkinName))
        {
            SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.ObtainedBallSkins + "BallBasicSkin");
            SaveAndLoadManager.SetStringValue("BallBasicSkin", SaveAndLoadManager.CurrentBallSkinName, true, true);
        }
        else
        {
            // Validar que la skin actual esté realmente desbloqueada
            string currentSkin = SaveAndLoadManager.GetStringValue(SaveAndLoadManager.CurrentBallSkinName);

            // Si la skin no está desbloqueada, volver a la básica
            if (SaveAndLoadManager.GetIntValue(SaveAndLoadManager.ObtainedBallSkins + currentSkin) == 0)
            {
                Debug.LogWarning($"Current skin '{currentSkin}' is not unlocked. Reverting to BallBasicSkin.");
                SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.ObtainedBallSkins + "BallBasicSkin", true);
                SaveAndLoadManager.SetStringValue("BallBasicSkin", SaveAndLoadManager.CurrentBallSkinName, true, true);
            }
        }

        if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CurrentWorldName))
            SaveAndLoadManager.SetStringValue("Neon", SaveAndLoadManager.CurrentWorldName, true, true);

        // Obtener mundo actual del save
        _currentWorld = SaveAndLoadManager.GetStringValue(SaveAndLoadManager.CurrentWorldName);
        if (string.IsNullOrEmpty(_currentWorld))
            _currentWorld = "Neon";

        StoreManager.Instance.UpdateSkinsState?.Invoke();

        _levelsCloseButton = FindAndValidateComponent<Button>(transform, "LevelsCloseButton");
        _levelsCloseButton.onClick.AddListener(() =>
        {
            _levelsPanel.SetActive(false);
            _menuPanel.SetActive(true);
        });

        var storeButton = FindAndValidateComponent<Button>(transform, "StoreBTN");
        storeButton.onClick.AddListener(() =>
            UIManager.Instance.ChangeCanvas("MenuManagerCanvas", "StoreCanvas"));

        var configsButton = FindAndValidateComponent<Button>(transform, "ConfigsButton");
        configsButton.onClick.AddListener(() =>
            UIManager.Instance.ChangeCanvas("MenuManagerCanvas", "ConfigsCanvas"));

        var noAdsPopUp = FindAndValidateComponent<PopUp>(transform, "NoAdsPopUp");
        var noAdsPriceText = FindAndValidateComponent<TextMeshProUGUI>(transform, "NoAdsPriceText");

        var noAdsBtn = FindAndValidateComponent<Button>(transform, "NoAdsBtn");
        noAdsBtn.onClick.AddListener(() =>
        {
            noAdsPopUp.Initialize("noadstittle", "noadsdescription");
            UIManager.Instance.SetText(noAdsPriceText, "US$ " + IAPManager.Instance.GetProductByID(
                "no_ads_product").metadata.localizedPriceString);
            noAdsPopUp.Show();
        });

        _modeText = FindAndValidateComponent<TextMeshProUGUI>(transform, "ModeTittleText");
        var nextLevelText = FindAndValidateComponent<TextMeshProUGUI>(transform, "NextLevelNumberText");
        var nextModeBtn = FindAndValidateComponent<Button>(transform, "NextModeBTN");
        nextModeBtn.interactable = (SaveAndLoadManager.GetHighestLevelReachedByGameModeAndWorld(GameManager.GameModes.Dunk, "Neon") / 5) > 0;

        nextModeBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.SetCurrentModeByIndex(1);
            UpdateModeTexts(_modeText, nextLevelText);

            AddressablesUtility.LoadAsset<GameObject>($"{GameManager.Instance.GetCurrentGameMode}AchievementLevelSelector", achievementImageAddressable =>
            {
                _achievementImageInLevelsSelector = achievementImageAddressable.GetComponent<Image>();
                OnLevelSelectorClicked(false);
            });
        });

        var previousModeBtn = FindAndValidateComponent<Button>(transform, "PreviousModeBTN");
        previousModeBtn.interactable = (SaveAndLoadManager.GetHighestLevelReachedByGameModeAndWorld(GameManager.GameModes.Dunk, "Neon") / 5) > 0;
        previousModeBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.SetCurrentModeByIndex(-1);
            UpdateModeTexts(_modeText, nextLevelText);

            AddressablesUtility.LoadAsset<GameObject>($"{GameManager.Instance.GetCurrentGameMode}AchievementLevelSelector", achievementImageAddressable =>
            {
                _achievementImageInLevelsSelector = achievementImageAddressable.GetComponent<Image>();
                OnLevelSelectorClicked(false);
            });
        });

        _levelsSelectorHasNotifications = FindAndValidateGameObjectComponent(transform, "HasNotificationsLevelsSelector");
        _levelsSelectorHasNotifications.SetActive(false);

        _missionsButtonHasNotifications = FindAndValidateGameObjectComponent(transform, "HasNotificationsMissions");
        _missionsButtonHasNotifications.SetActive(DailyMissionsManager.Instance.GetAllMissionsCompletedStatus());

        _missionsButtonsTextNotification = FindAndValidateComponent<TextMeshProUGUI>(transform, "MissionsNotificationsCountText");
        _missionsButtonsTextNotification.text = DailyMissionsManager.Instance.GetAlMissionsCompletedCount().ToString();

        UpdateModeTexts(_modeText, nextLevelText);
        UpdateNotificationsOnNoAds();

        _unlockSkinsPopUp = FindAndValidateComponent<PopUp>(transform, "UnlockSkinsPopUp");
        if (!string.IsNullOrEmpty(GameManager.UnlokedSkin))
            OnUnlockWorldSkin(SaveAndLoadManager.GetStringValue(SaveAndLoadManager.CurrentWorldName) + "SkinUI");
        else
            _unlockSkinsPopUp.StrongHide();

        CheckForUnlokedModes();

        var fadeController = FindAndValidateGameObjectComponent(transform, "FadeController");
        _thanksForBuyPopUp = FindAndValidateComponent<PopUp>(transform, "ThanksForBuyPopUp");
        _dailyMissionsPanel.SetActive(false);
        _dailyRewardsPanel.SetActive(false);
        fadeController.SetActive(true);
        _thanksForBuyPopUp.StrongHide();
        noAdsPopUp.StrongHide();
        _menuPanel.SetActive(true);
        _levelsPanel.SetActive(false);

        PauseAndResumeManager.Instance.RestartResumeAction();
        PauseAndResumeManager.Instance.RestartPauseAction();

        UIManager.Instance.AddCanvas(gameObject, true);
        LevelManager.Instance.ResetCoins();
        AudioManager.Instance.PlayMusicByType(AudioManager.MusicClipType.MenuMusic);

        AddressablesUtility.LoadAsset<GameObject>($"{GameManager.Instance.GetCurrentGameMode}AchievementLevelSelector", achievementImageAddressable =>
        {
            _achievementImageInLevelsSelector = achievementImageAddressable.GetComponent<Image>();
            OnLevelSelectorClicked(true);
        });
    }

    private void OnEnable()
    {
        UpdateTexts();
        if (_modeText != null)
            UIManager.Instance.SetText(_modeText,
                LanguageManager.Instance.GetLocalizedText(GameManager.Instance.GetCurrentGameMode.ToString()));

        if (IAPManager.Instance)
        {
            IAPManager.Instance.OnCompletePurchase += UpdateTexts;
            IAPManager.Instance.OnCompletePurchase += ActivatePopUpAfterBuy;
            IAPManager.Instance.OnCompletePurchase += UpdateNotificationsOnNoAds;
        }

        if (DailyMissionsManager.Instance)
        {
            DailyMissionsManager.Instance.OnCompleteMission += UpdateTexts;
            DailyMissionsManager.Instance.OnDailyMissionsReset += UpdateTexts;
        }
    }

    private void OnDisable()
    {
        if (IAPManager.Instance)
        {
            IAPManager.Instance.OnCompletePurchase -= UpdateTexts;
            IAPManager.Instance.OnCompletePurchase -= ActivatePopUpAfterBuy;
            IAPManager.Instance.OnCompletePurchase -= UpdateNotificationsOnNoAds;
        }

        if (DailyMissionsManager.Instance)
        {
            DailyMissionsManager.Instance.OnCompleteMission -= UpdateTexts;
            DailyMissionsManager.Instance.OnDailyMissionsReset -= UpdateTexts;
        }
    }

    private void Update()
    {
#if UNITY_ANDROID
        if (Input.GetKeyUp(KeyCode.Escape))
            Application.Quit();
#endif
    }

    #region LEVEL SELECTOR

    private void OnLevelSelectorClicked(bool firstTime)
    {
        var fadeAnimator = FindAndValidateGameObjectComponent(transform, "Fade").GetComponent<Animator>();
        var buttonsName = "DunkLevel";

        for (int i = 1; i <= 50; i++)
        {
            if (!firstTime)
                buttonsName = $"{_currentWorld}Level{i}";

            var button = FindAndValidateComponent<Button>(transform, buttonsName);

            if (button == null)
                break;

            button.name = $"{_currentWorld}Level{i}";

            #region UNLOCK LEVELS
            button.onClick.RemoveAllListeners();
            int levelIndex = i; // Variable temporal para capturar el valor actual de 'i'
            button.onClick.AddListener(() =>
            {
                button.interactable = false;
                _levelsCloseButton.interactable = false;
                UIManager.Instance.ClearCnavasesList();
                ScenesManager.Instance.LoadSceneAsync($"{_currentWorld}Level{levelIndex}", fadeAnimator);
                AudioManager.Instance.StopMusic();
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.PlayLevelSound);
            });

            if (i == 1)
                button.interactable = true;
            else
                button.interactable = SaveAndLoadManager.GetLevelCompleted(GameManager.Instance.GetCurrentGameMode, _currentWorld, i - 1);
            #endregion

            #region HAS COINS
            var hasCoinImage = FindAndValidateComponent<Image>(button.transform, $"DunkHasCoin");
            // Usar nuevo sistema para verificar monedas
            hasCoinImage.gameObject.SetActive(
                SaveAndLoadManager.HasLevelData(GameManager.Instance.GetCurrentGameMode, _currentWorld, i) &&
                SaveAndLoadManager.GetLevelCoinObtained(GameManager.Instance.GetCurrentGameMode, _currentWorld, i));
            #endregion

            #region HAS ACHIEVEMENT
            var achievementImage = FindAndValidateComponent<Image>(button.transform, $"DunkRecord");
            achievementImage.sprite = _achievementImageInLevelsSelector.sprite;


            // Usar nuevo sistema para verificar objetivo completado
            achievementImage.gameObject.SetActive(
                SaveAndLoadManager.HasLevelData(GameManager.Instance.GetCurrentGameMode, _currentWorld, i) &&
                SaveAndLoadManager.GetLevelObjectiveComplete(GameManager.Instance.GetCurrentGameMode, _currentWorld, i));
            #endregion

            #region WITHOUT DEATH
            var noDeathImage = FindAndValidateComponent<Image>(button.transform, $"DunkWithoutDeath");
            // Usar nuevo sistema para verificar sin muerte
            noDeathImage.gameObject.SetActive(
                SaveAndLoadManager.HasLevelData(GameManager.Instance.GetCurrentGameMode, _currentWorld, i) &&
                SaveAndLoadManager.GetLevelWithoutDeath(GameManager.Instance.GetCurrentGameMode, _currentWorld, i));
            #endregion

            #region LEVEL TEXT
            var levelNumText = FindAndValidateComponent<TextMeshProUGUI>(button.transform, $"DunkLevelNumText");
            UIManager.Instance.SetText(levelNumText, i);
            #endregion

            if (_levelsSelectorHasNotifications.activeInHierarchy || !hasCoinImage.isActiveAndEnabled ||
                !achievementImage.isActiveAndEnabled || !noDeathImage.isActiveAndEnabled)
                _levelsSelectorHasNotifications.SetActive(true);
        }

        var playButton = FindAndValidateComponent<Button>(transform, "PlayBTN");
        playButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ClearCnavasesList();
            ScenesManager.Instance.LoadSceneAsync(
                $"{_currentWorld}Level{SaveAndLoadManager.GetHighestLevelReachedByGameModeAndWorld(GameManager.Instance.GetCurrentGameMode, _currentWorld, 49) + 1}", fadeAnimator);
        });
    }
    #endregion

    private void OnUnlockWorldSkin(string skinNamePrefab)
    {
        string unlockedSkinName = GameManager.UnlokedSkin;

        AddressablesUtility.LoadAsset<GameObject>(
            skinNamePrefab, iGo =>
            {
                _unlockSkinsPopUp.InitializeWithIcon("unlockskin", iGo, "wantequipskin",
                () =>
                {
                    SaveAndLoadManager.SetStringValue(unlockedSkinName, SaveAndLoadManager.CurrentBallSkinName, true, true);
                    StoreManager.Instance.UpdateSkinsState?.Invoke();
                });
                _unlockSkinsPopUp.Show();
                GameManager.UnlokedSkin = null;
            });
    }

    public void OnUnlockSkin(string skinNamePrefab)
    {
        AddressablesUtility.LoadAsset<GameObject>(
            skinNamePrefab, iGo =>
            {
                _unlockSkinsPopUp.InitializeWithIcon("unlockskin", iGo, "wantequipskin",
                () =>
                {
                    SaveAndLoadManager.SetStringValue(skinNamePrefab, SaveAndLoadManager.CurrentBallSkinName, true, true);
                    StoreManager.Instance.UpdateSkinsState?.Invoke();
                });
                _unlockSkinsPopUp.Show();
            });
    }

    private void CheckForUnlokedModes()
    {
        _unlockModePopUp = FindAndValidateComponent<PopUp>(transform, "UnlockModePopUp");

        foreach (GameManager.GameModes mode in Enum.GetValues(typeof(GameManager.GameModes)))
        {
            if (!GameManager.Instance.IsModeUnlocked(mode) || mode == GameManager.GameModes.Dunk || mode == GameManager.GameModes.Null)
                continue;

            _unlockModePopUp.Initialize($"unlock{mode}Tittle");
            _unlockModePopUp.Show();
            SaveAndLoadManager.SetIntValue(2, SaveAndLoadManager.ObtainedGameMode + mode, true, true);
        }
    }

    private void ActivatePopUpAfterBuy()
    {
        if (_thanksForBuyPopUp == null)
            return;

        _thanksForBuyPopUp.Initialize("tnksforbuytittle", "tnksforbuydescription");
        _thanksForBuyPopUp.Show();
    }

    #region Utility Methods

    private void UpdateTexts()
    {
        if (_coinsText != null)
            UIManager.Instance.SetText(_coinsText, SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName));
        if (_orbsText != null)
            UIManager.Instance.SetText(_orbsText, SaveAndLoadManager.GetIntValue(SaveAndLoadManager.OrbsName));
        if (_missionsButtonsTextNotification != null)
            UIManager.Instance.SetText(_missionsButtonsTextNotification,
                DailyMissionsManager.Instance.GetAlMissionsCompletedCount().ToString(), true);
    }

    private void UpdateModeTexts(TextMeshProUGUI modeText, TextMeshProUGUI nextLevelText)
    {
        if (modeText != null)
            UIManager.Instance.SetText(modeText,
                LanguageManager.Instance.GetLocalizedText(GameManager.Instance.GetCurrentGameMode.ToString()));
        if (nextLevelText != null)
            UIManager.Instance.SetText(nextLevelText,
                SaveAndLoadManager.GetHighestLevelReachedByGameModeAndWorld(GameManager.Instance.GetCurrentGameMode, _currentWorld, 49) + 1);
    }

    public void UpdateNotificationsOnNoAds()
    {
        var hasNotificationsNoAds = FindAndValidateComponent<Image>(transform, "HasNotificationsNoAds");
        hasNotificationsNoAds.gameObject.SetActive(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.NoAdsBougthName) != 1);
    }
    #endregion
}