using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityAddressables;

public class MenuManagerCanvas : CanvasElementLocator
{
    private GameObject _menuPanel;
    private GameObject _dunkLevelsPanel;

    private TextMeshProUGUI _coinsText;
    private PopUp _unlockSkinsPopUp;

    void Start()
    {
        Application.targetFrameRate = 60;

        GameManager.Instance.SelectGameMode(1);     //TODO: Mandar un 0 y luego cambiar el modo desde el menu

        _menuPanel = FindAndValidateGameObjectComponent(transform, "MenuPanel");
        _dunkLevelsPanel = FindAndValidateGameObjectComponent(transform, "DunkLevelsPanel");

        var levelsSelectorButton = FindAndValidateComponent<Button>(transform, "LevelsSelectorButton");
        levelsSelectorButton.onClick.AddListener(() =>
        {
            _dunkLevelsPanel.SetActive(true);
            _menuPanel.SetActive(false);
        });

        _coinsText = FindAndValidateComponent<TextMeshProUGUI>(transform, "CoinsText");
        UIManager.Instance.SetText(_coinsText, SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName));

        if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CurrentBallSkinName))
        {
            SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.ObtainedBallSkins + "BallBasicSkin");
            SaveAndLoadManager.SetStringValue("BallBasicSkin", SaveAndLoadManager.CurrentBallSkinName, true);
        }
        else
        {
            // Validar que la skin actual esté realmente desbloqueada
            string currentSkin = SaveAndLoadManager.GetStringValue(SaveAndLoadManager.CurrentBallSkinName);

            // Si la skin no está desbloqueada, volver a la básica
            if (SaveAndLoadManager.GetIntValue(SaveAndLoadManager.ObtainedBallSkins + currentSkin) == 0)
            {
                Debug.LogWarning($"Current skin '{currentSkin}' is not unlocked. Reverting to BallBasicSkin.");
                SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.ObtainedBallSkins + "BallBasicSkin");
                SaveAndLoadManager.SetStringValue("BallBasicSkin", SaveAndLoadManager.CurrentBallSkinName, true);
            }
        }

        if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CurrentWorldName))
            SaveAndLoadManager.SetStringValue("Neon", SaveAndLoadManager.CurrentWorldName, true);

        StoreManager.Instance.UpdateSkinsState?.Invoke();

        var dunkCloseButton = FindAndValidateComponent<Button>(transform, "DunkCloseButton");
        dunkCloseButton.onClick.AddListener(() =>
        {
            _dunkLevelsPanel.SetActive(false);
            _menuPanel.SetActive(true);
        });

        var storeButton = FindAndValidateComponent<Button>(transform, "StoreBTN");
        storeButton.onClick.AddListener(() =>
            UIManager.Instance.ChangeCanvas("MenuManagerCanvas", "StoreCanvas"));

        var configsButton = FindAndValidateComponent<Button>(transform, "ConfigsButton");
        configsButton.onClick.AddListener(() =>
            UIManager.Instance.ChangeCanvas("MenuManagerCanvas", "ConfigsCanvas"));

        _unlockSkinsPopUp = FindAndValidateComponent<PopUp>(transform, "UnlockSkinsPopUp");
        if (!string.IsNullOrEmpty(GameManager.UnlokedSkin))
            OnUnlockSkin();

        PauseAndResumeManager.Instance.RestartResumeAction();
        PauseAndResumeManager.Instance.RestartPauseAction();

        UIManager.Instance.AddCanvas(gameObject, true);
        LevelManager.Instance.ResetCoins();
        AudioManager.Instance.PlayMusicByType(AudioManager.MusicClipType.MenuMusic);

        OnDunkLevelsClicked();
    }

    private void OnEnable()
    {
        if (_coinsText != null)
            UIManager.Instance.SetText(_coinsText, SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName));
    }

    #region DUNK
    private void OnDunkLevelsClicked()
    {
        var fadeAnimator = FindAndValidateGameObjectComponent(transform, "Fade").GetComponent<Animator>();

        int nextDunkLevel = 1;

        for (int i = 1; i <= 50; i++)
        {
            var button = FindAndValidateComponent<Button>(transform, $"DunkLevel");

            if (button == null)
                break;

            button.name = $"DunkLevel{i}";
            if (SaveAndLoadManager.ContainsKey(SaveAndLoadManager.DunkLevelName + i) && nextDunkLevel <= 49)
                nextDunkLevel = SaveAndLoadManager.GetIntValue(SaveAndLoadManager.DunkLevelName + i) + 1;

            #region UNLOCK LEVELS
            int levelIndex = i; // Variable temporal para capturar el valor actual de 'i'
            button.onClick.AddListener(() =>
            {
                button.interactable = false;
                UIManager.Instance.ClearCnavasesList();
                ScenesManager.Instance.LoadSceneAsync($"DunkLevel{levelIndex}", fadeAnimator);
                AudioManager.Instance.StopMusic();
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.PlayLevelSound);
            });

            if (i == 1)
                button.interactable = true;
            else
                button.interactable = SaveAndLoadManager.ContainsKey(
                    SaveAndLoadManager.DunkLevelName + (i - 1));
            #endregion
            #region HAS COINS
            var hasCoinImage = FindAndValidateComponent<Image>(button.transform, $"DunkHasCoin");
            hasCoinImage.gameObject.SetActive(
                SaveAndLoadManager.ContainsKey(SaveAndLoadManager.DunkLevelName + i) &&
                SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinNameByLevel + GameManager.GameModes.Dunk + i) == 1);
            #endregion

            #region BEST
            var touchImage = FindAndValidateComponent<Image>(button.transform, $"DunkRecord");
            touchImage.gameObject.SetActive(
                SaveAndLoadManager.ContainsKey(SaveAndLoadManager.DunkLevelName + i) &&
                SaveAndLoadManager.GetIntValue(SaveAndLoadManager.DunkTouchesCompleteName + i) == 1);
            #endregion

            #region WITHOUT DEATH
            var noDeathImage = FindAndValidateComponent<Image>(button.transform, $"DunkWithoutDeath");
            noDeathImage.gameObject.SetActive(
                SaveAndLoadManager.ContainsKey(SaveAndLoadManager.DunkLevelName + i) &&
                SaveAndLoadManager.GetIntValue(SaveAndLoadManager.DunkWithoutDeathName + i) == 1);
            #endregion

            #region LEVEL TEXT
            var levelNumText = FindAndValidateComponent<TextMeshProUGUI>(button.transform, $"DunkLevelNumText");
            levelNumText.text = i.ToString();
            #endregion
        }

        var playButton = FindAndValidateComponent<Button>(transform, "PlayBTN");
        playButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ClearCnavasesList();

            ScenesManager.Instance.LoadSceneAsync((SaveAndLoadManager.DunkLevelName +
                nextDunkLevel).Replace("_", ""), fadeAnimator);
        });

        var nextLevelText = FindAndValidateComponent<TextMeshProUGUI>(transform, "NextLevelNumberText");
        nextLevelText.text = $"{nextDunkLevel}";
    }
    #endregion

    private void OnUnlockSkin()
    {
        string unlockedSkinName = GameManager.UnlokedSkin;

        AddressablesUtility.LoadAsset<GameObject>(
            SaveAndLoadManager.GetStringValue(SaveAndLoadManager.CurrentWorldName) + "SkinUI", iGo =>
            {
                _unlockSkinsPopUp.InitializeWithIcon("unlockskin", iGo, "wantequipskin",
                () =>
                {
                    SaveAndLoadManager.SetStringValue(unlockedSkinName, SaveAndLoadManager.CurrentBallSkinName, true);
                    StoreManager.Instance.UpdateSkinsState?.Invoke();
                });
                _unlockSkinsPopUp.Show();
                GameManager.UnlokedSkin = null;
            });
    }

    #region DEBUG

    public static MenuManagerCanvas Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        SetSavedError(GameManager.SavedErrorText);
        SetSavedLanguageText(GameManager.SavedLanguageText);
        SetSavedMusicText(GameManager.SavedMusicText);
        SetSavedSoundText(GameManager.SavedSoundText);
        SetSavedSkinText(GameManager.SavedSkinText);

        SetLoadedError(GameManager.LoadedErrorText);
        SetLoadedLanguageText(GameManager.LoadedLanguageText);
        SetLoadedMusicText(GameManager.LoadedMusicText);
        SetLoadedSoundText(GameManager.LoadedSoundText);
        SetLoadedSkinText(GameManager.LoadedSkinText);
    }

    public TextMeshProUGUI SavedErrorText;
    public TextMeshProUGUI SavedLanguageText;
    public TextMeshProUGUI SavedMusicText;
    public TextMeshProUGUI SavedSoundText;
    public TextMeshProUGUI SavedSkinText;

    public TextMeshProUGUI LoadedErrorText;
    public TextMeshProUGUI LoadedLanguageText;
    public TextMeshProUGUI LoadedMusicText;
    public TextMeshProUGUI LoadedSoundText;
    public TextMeshProUGUI LoadedSkinText;

    void SetSavedError(string text) => SavedErrorText.text = $"'{text}'";
    void SetSavedLanguageText(string text) => SavedLanguageText.text = $"'{text}'";
    void SetSavedMusicText(string text) => SavedMusicText.text = $"'{text}'";
    void SetSavedSoundText(string text) => SavedSoundText.text = $"'{text}'";
    void SetSavedSkinText(string text) => SavedSkinText.text = $"'{text}'";

    void SetLoadedError(string text) => LoadedErrorText.text = $"'{text}'";
    void SetLoadedLanguageText(string text) => LoadedLanguageText.text = $"'{text}'";
    void SetLoadedMusicText(string text) => LoadedMusicText.text = $"'{text}'";
    void SetLoadedSoundText(string text) => LoadedSoundText.text = $"'{text}'";
    void SetLoadedSkinText(string text) => LoadedSkinText.text = $"'{text}'";
    #endregion
}