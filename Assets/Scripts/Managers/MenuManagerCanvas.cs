using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityAddressables;
using static GameManager;

public class MenuManagerCanvas : CanvasElementLocator
{
    private GameObject _menuPanel;
    private GameObject _levelsPanel;

    private TextMeshProUGUI _coinsText;
    private TextMeshProUGUI _orbsText;
    private TextMeshProUGUI _modeText;
    private PopUp _unlockSkinsPopUp;
    private Button _levelsCloseButton;

    // Mundo actual - temporal hasta que implementes el sistema completo
    private string _currentWorld = "Neon";

    void Start()
    {
        Application.targetFrameRate = 60;

        if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CurrentModeName))
            SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.CurrentModeName, true, true);

        Instance.SelectGameMode(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CurrentModeName));

        _menuPanel = FindAndValidateGameObjectComponent(transform, "MenuPanel");
        _levelsPanel = FindAndValidateGameObjectComponent(transform, "LevelsPanel");

        var levelsSelectorButton = FindAndValidateComponent<Button>(transform, "LevelsSelectorButton");
        levelsSelectorButton.onClick.AddListener(() =>
        {
            _levelsPanel.SetActive(true);
            _menuPanel.SetActive(false);
            OnLevelSelectorClicked(false);
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

        var nextModeBtn = FindAndValidateComponent<Button>(transform, "NextModeBTN");
        nextModeBtn.interactable = (SaveAndLoadManager.GetHighestLevelReachedByGameModeAndWorld(GameModes.Dunk, "Neon") / 15) > 0;

        nextModeBtn.onClick.AddListener(() =>
        {
            Instance.SetCurrentModeByIndex(1);
            UIManager.Instance.SetText(_modeText,
                LanguageManager.Instance.GetLocalizedText(Instance.GetCurrentGameMode.ToString()));
        });

        var previousModeBtn = FindAndValidateComponent<Button>(transform, "PreviousModeBTN");
        previousModeBtn.interactable = (SaveAndLoadManager.GetHighestLevelReachedByGameModeAndWorld(GameModes.Dunk, "Neon") / 15) > 0;
        previousModeBtn.onClick.AddListener(() =>
        {
            Instance.SetCurrentModeByIndex(-1);
            UIManager.Instance.SetText(_modeText,
                LanguageManager.Instance.GetLocalizedText(Instance.GetCurrentGameMode.ToString()));
        });

        UIManager.Instance.SetText(_modeText,
            LanguageManager.Instance.GetLocalizedText(Instance.GetCurrentGameMode.ToString()));

        UpdateNotificationsOnNoAds();

        _unlockSkinsPopUp = FindAndValidateComponent<PopUp>(transform, "UnlockSkinsPopUp");
        if (!string.IsNullOrEmpty(UnlokedSkin))
            OnUnlockSkin();

        PauseAndResumeManager.Instance.RestartResumeAction();
        PauseAndResumeManager.Instance.RestartPauseAction();

        UIManager.Instance.AddCanvas(gameObject, true);
        LevelManager.Instance.ResetCoins();
        AudioManager.Instance.PlayMusicByType(AudioManager.MusicClipType.MenuMusic);

        OnLevelSelectorClicked(true);
    }

    private void OnEnable()
    {
        UpdateTexts();
        if (IAPManager.Instance)
        {
            IAPManager.Instance.OnCompletePurchase += UpdateTexts;
            IAPManager.Instance.OnCompletePurchase += ActivatePopUpAfterBuy;
            IAPManager.Instance.OnCompletePurchase += UpdateNotificationsOnNoAds;
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

        int nextDunkLevel = 1;
        var buttonsName = "DunkLevel";

        for (int i = 1; i <= 50; i++)
        {
            if (!firstTime)
                buttonsName = $"{_currentWorld}Level{i}";

            var button = FindAndValidateComponent<Button>(transform, buttonsName);

            if (button == null)
                break;

            button.name = $"{_currentWorld}Level{i}";

            // Usar nuevo sistema para determinar el siguiente nivel
            if (SaveAndLoadManager.HasLevelData(Instance.GetCurrentGameMode, _currentWorld, i) && nextDunkLevel <= 49)
                nextDunkLevel = i + 1;

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
                // Usar nuevo sistema para verificar si el nivel anterior está completado
                button.interactable = SaveAndLoadManager.HasLevelData(Instance.GetCurrentGameMode, _currentWorld, i - 1);
            #endregion

            #region HAS COINS
            var hasCoinImage = FindAndValidateComponent<Image>(button.transform, $"DunkHasCoin");
            // Usar nuevo sistema para verificar monedas
            hasCoinImage.gameObject.SetActive(
                SaveAndLoadManager.HasLevelData(Instance.GetCurrentGameMode, _currentWorld, i) &&
                SaveAndLoadManager.GetLevelCoinObtained(Instance.GetCurrentGameMode, _currentWorld, i));
            #endregion

            #region HAS ACHIEVEMENT
            var touchImage = FindAndValidateComponent<Image>(button.transform, $"DunkRecord");
            // Usar nuevo sistema para verificar objetivo completado
            touchImage.gameObject.SetActive(
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
            ScenesManager.Instance.LoadSceneAsync($"{_currentWorld}Level{nextDunkLevel}", fadeAnimator);
        });

        var nextLevelText = FindAndValidateComponent<TextMeshProUGUI>(transform, "NextLevelNumberText");
        UIManager.Instance.SetText(nextLevelText, nextDunkLevel);
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
        var thanksForBuyPopUp = FindAndValidateComponent<PopUp>(transform, "ThanksForBuyPopUp");
        if (thanksForBuyPopUp == null)
            return;

        thanksForBuyPopUp.Initialize("tnksforbuytittle", "tnksforbuydescription");
        thanksForBuyPopUp.Show();
    }

    #region Utility Methods

    public void UpdateTexts()
    {
        if (_coinsText != null)
            UIManager.Instance.SetText(_coinsText, SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName));
        if (_orbsText != null)
            UIManager.Instance.SetText(_orbsText, SaveAndLoadManager.GetIntValue(SaveAndLoadManager.OrbsName));
        if (_modeText != null)
            UIManager.Instance.SetText(_modeText,
                LanguageManager.Instance.GetLocalizedText(Instance.GetCurrentGameMode.ToString()));
    }

    public void UpdateNotificationsOnNoAds()
    {
        var hasNotificationsNoAds = FindAndValidateComponent<Image>(transform, "HasNotificationsNoAds");
        hasNotificationsNoAds.gameObject.SetActive(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.NoAdsBougthName) != 1);
    }

    ///// <summary>
    ///// Cambia el mundo actual y actualiza la UI
    ///// Usar cuando implementes el selector de mundos
    ///// </summary>
    //public void ChangeCurrentWorld(string worldName)
    //{
    //    _currentWorld = worldName;
    //    SaveAndLoadManager.SetStringValue(worldName, SaveAndLoadManager.CurrentWorldName, true, true);

    //    // Refrescar la UI de niveles
    //    OnLevelSelectorClicked();

    //    Debug.Log($"World changed to: {worldName}");
    //}

    ///// <summary>
    ///// Obtiene el mundo actual
    ///// </summary>
    //public string GetCurrentWorld()
    //{
    //    return _currentWorld;
    //}

    ///// <summary>
    ///// Obtiene estadísticas del mundo actual
    ///// </summary>
    //public (int completed, int withCoins, int withoutDeath, int objectiveComplete) GetWorldStats()
    //{
    //    int completed = 0;
    //    int withCoins = 0;
    //    int withoutDeath = 0;
    //    int objectiveComplete = 0;

    //    for (int i = 1; i <= 50; i++)
    //    {
    //        if (SaveAndLoadManager.HasLevelData(GameModes.Dunk, _currentWorld, i))
    //        {
    //            completed++;

    //            if (SaveAndLoadManager.GetLevelCoinObtained(GameModes.Dunk, _currentWorld, i))
    //                withCoins++;

    //            if (SaveAndLoadManager.GetLevelWithoutDeath(GameModes.Dunk, _currentWorld, i))
    //                withoutDeath++;

    //            if (SaveAndLoadManager.GetLevelObjectiveComplete(GameModes.Dunk, _currentWorld, i))
    //                objectiveComplete++;
    //        }
    //    }

    //    return (completed, withCoins, withoutDeath, objectiveComplete);
    //}
    #endregion
}