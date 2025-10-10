using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityAddressables;
using static GameManager;

public class MenuManagerCanvas : CanvasElementLocator
{
    private GameObject _menuPanel;
    private GameObject _levelsPanel;
    private GameObject _dailyMissionsPanel;
    private GameObject _downPanel;

    private TextMeshProUGUI _coinsText;
    private TextMeshProUGUI _orbsText;
    private TextMeshProUGUI _modeText;
    private PopUp _unlockSkinsPopUp;
    private PopUp _thanksForBuyPopUp;
    private Button _levelsCloseButton;
    private Image _achievementImageInLevelsSelector = null;

    // Mundo actual - temporal hasta que implementes el sistema completo
    private string _currentWorld = "Neon";

    void Start()
    {
        if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CurrentModeName))
            SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.CurrentModeName, true, true);

        Instance.SelectGameMode(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CurrentModeName));

        _menuPanel = FindAndValidateGameObjectComponent(transform, "MenuPanel");
        _levelsPanel = FindAndValidateGameObjectComponent(transform, "LevelsPanel");
        _dailyMissionsPanel = FindAndValidateGameObjectComponent(transform, "DailyQuestsPanel");
        _downPanel = FindAndValidateGameObjectComponent(transform, "DownPanel");

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
            _downPanel.SetActive(false);
        });
        var dailyQuestsBackButton = FindAndValidateComponent<Button>(transform, "DailyQuestsBackButton");
        dailyQuestsBackButton.onClick.AddListener(() =>
        {
            _dailyMissionsPanel.SetActive(false);
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
        nextModeBtn.interactable = (SaveAndLoadManager.GetHighestLevelReachedByGameModeAndWorld(GameModes.Dunk, "Neon") / 15) > 0;

        nextModeBtn.onClick.AddListener(() =>
        {
            Instance.SetCurrentModeByIndex(1);
            UpdateModeTexts(_modeText, nextLevelText);

            AddressablesUtility.LoadAsset<GameObject>($"{Instance.GetCurrentGameMode}AchievementLevelSelector", achievementImageAddressable =>
            {
                _achievementImageInLevelsSelector = achievementImageAddressable.GetComponent<Image>();
                OnLevelSelectorClicked(false);
            });
        });

        var previousModeBtn = FindAndValidateComponent<Button>(transform, "PreviousModeBTN");
        previousModeBtn.interactable = (SaveAndLoadManager.GetHighestLevelReachedByGameModeAndWorld(GameModes.Dunk, "Neon") / 15) > 0;
        previousModeBtn.onClick.AddListener(() =>
        {
            Instance.SetCurrentModeByIndex(-1);
            UpdateModeTexts(_modeText, nextLevelText);

            AddressablesUtility.LoadAsset<GameObject>($"{Instance.GetCurrentGameMode}AchievementLevelSelector", achievementImageAddressable =>
            {
                _achievementImageInLevelsSelector = achievementImageAddressable.GetComponent<Image>();
                OnLevelSelectorClicked(false);
            });
        });

        UpdateModeTexts(_modeText, nextLevelText);
        UpdateNotificationsOnNoAds();

        _unlockSkinsPopUp = FindAndValidateComponent<PopUp>(transform, "UnlockSkinsPopUp");
        if (!string.IsNullOrEmpty(UnlokedSkin))
            OnUnlockSkin();
        else
            _unlockSkinsPopUp.gameObject.SetActive(true);

        var fadeController = FindAndValidateGameObjectComponent(transform, "FadeController");
        _thanksForBuyPopUp = FindAndValidateComponent<PopUp>(transform, "ThanksForBuyPopUp");
        _dailyMissionsPanel.SetActive(false);
        fadeController.SetActive(true);
        _thanksForBuyPopUp.gameObject.SetActive(true);
        noAdsPopUp.gameObject.SetActive(true);
        _menuPanel.SetActive(true);
        _levelsPanel.SetActive(false);

        PauseAndResumeManager.Instance.RestartResumeAction();
        PauseAndResumeManager.Instance.RestartPauseAction();

        UIManager.Instance.AddCanvas(gameObject, true);
        LevelManager.Instance.ResetCoins();
        AudioManager.Instance.PlayMusicByType(AudioManager.MusicClipType.MenuMusic);

        AddressablesUtility.LoadAsset<GameObject>($"{Instance.GetCurrentGameMode}AchievementLevelSelector", achievementImageAddressable =>
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
                LanguageManager.Instance.GetLocalizedText(Instance.GetCurrentGameMode.ToString()));

        if (IAPManager.Instance)
        {
            IAPManager.Instance.OnCompletePurchase += UpdateTexts;
            IAPManager.Instance.OnCompletePurchase += ActivatePopUpAfterBuy;
            IAPManager.Instance.OnCompletePurchase += UpdateNotificationsOnNoAds;
        }

        DailyMissionsManager.Instance.OnCompleteMission += UpdateTexts;
    }

    private void OnDisable()
    {
        if (IAPManager.Instance)
        {
            IAPManager.Instance.OnCompletePurchase -= UpdateTexts;
            IAPManager.Instance.OnCompletePurchase -= ActivatePopUpAfterBuy;
            IAPManager.Instance.OnCompletePurchase -= UpdateNotificationsOnNoAds;
        }

        DailyMissionsManager.Instance.OnCompleteMission -= UpdateTexts;
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
                button.interactable = SaveAndLoadManager.GetLevelCompleted(Instance.GetCurrentGameMode, _currentWorld, i - 1);
            #endregion

            #region HAS COINS
            var hasCoinImage = FindAndValidateComponent<Image>(button.transform, $"DunkHasCoin");
            // Usar nuevo sistema para verificar monedas
            hasCoinImage.gameObject.SetActive(
                SaveAndLoadManager.HasLevelData(Instance.GetCurrentGameMode, _currentWorld, i) &&
                SaveAndLoadManager.GetLevelCoinObtained(Instance.GetCurrentGameMode, _currentWorld, i));
            #endregion

            #region HAS ACHIEVEMENT
            var achievementImage = FindAndValidateComponent<Image>(button.transform, $"DunkRecord");
            achievementImage.sprite = _achievementImageInLevelsSelector.sprite;


            // Usar nuevo sistema para verificar objetivo completado
            achievementImage.gameObject.SetActive(
                SaveAndLoadManager.HasLevelData(Instance.GetCurrentGameMode, _currentWorld, i) &&
                SaveAndLoadManager.GetLevelObjectiveComplete(Instance.GetCurrentGameMode, _currentWorld, i));
            #endregion

            #region WITHOUT DEATH
            var noDeathImage = FindAndValidateComponent<Image>(button.transform, $"DunkWithoutDeath");
            // Usar nuevo sistema para verificar sin muerte
            noDeathImage.gameObject.SetActive(
                SaveAndLoadManager.HasLevelData(Instance.GetCurrentGameMode, _currentWorld, i) &&
                SaveAndLoadManager.GetLevelWithoutDeath(Instance.GetCurrentGameMode, _currentWorld, i));
            #endregion

            #region LEVEL TEXT
            var levelNumText = FindAndValidateComponent<TextMeshProUGUI>(button.transform, $"DunkLevelNumText");
            UIManager.Instance.SetText(levelNumText, i);
            #endregion
        }

        var playButton = FindAndValidateComponent<Button>(transform, "PlayBTN");
        playButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ClearCnavasesList();
            ScenesManager.Instance.LoadSceneAsync(
                $"{_currentWorld}Level{SaveAndLoadManager.GetHighestLevelReachedByGameModeAndWorld(Instance.GetCurrentGameMode, _currentWorld, 49) + 1}", fadeAnimator);
        });
    }
    #endregion

    private void OnUnlockSkin()
    {
        string unlockedSkinName = UnlokedSkin;

        AddressablesUtility.LoadAsset<GameObject>(
            SaveAndLoadManager.GetStringValue(SaveAndLoadManager.CurrentWorldName) + "SkinUI", iGo =>
            {
                _unlockSkinsPopUp.InitializeWithIcon("unlockskin", iGo, "wantequipskin",
                () =>
                {
                    SaveAndLoadManager.SetStringValue(unlockedSkinName, SaveAndLoadManager.CurrentBallSkinName, true, true);
                    StoreManager.Instance.UpdateSkinsState?.Invoke();
                });
                _unlockSkinsPopUp.Show();
                UnlokedSkin = null;
            });
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
    }

    private void UpdateModeTexts(TextMeshProUGUI modeText, TextMeshProUGUI nextLevelText)
    {
        if (modeText != null)
            UIManager.Instance.SetText(modeText,
                LanguageManager.Instance.GetLocalizedText(Instance.GetCurrentGameMode.ToString()));
        if (nextLevelText != null)
            UIManager.Instance.SetText(nextLevelText,
                SaveAndLoadManager.GetHighestLevelReachedByGameModeAndWorld(Instance.GetCurrentGameMode, _currentWorld, 49) + 1);
    }

    public void UpdateNotificationsOnNoAds()
    {
        var hasNotificationsNoAds = FindAndValidateComponent<Image>(transform, "HasNotificationsNoAds");
        hasNotificationsNoAds.gameObject.SetActive(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.NoAdsBougthName) != 1);
    }
    #endregion
}