using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityAddressables;
using static GameManager;

public class MenuManagerCanvas : CanvasElementLocator
{
    private GameObject _menuPanel;
    private GameObject _dunkLevelsPanel;

    private TextMeshProUGUI _coinsText;
    private PopUp _unlockSkinsPopUp;

    // Mundo actual - temporal hasta que implementes el sistema completo
    private string _currentWorld = "Neon";

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
            SaveAndLoadManager.SetStringValue("BallBasicSkin", SaveAndLoadManager.CurrentBallSkinName);
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
                SaveAndLoadManager.SetStringValue("BallBasicSkin", SaveAndLoadManager.CurrentBallSkinName);
            }
        }

        if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CurrentWorldName))
            SaveAndLoadManager.SetStringValue("Neon", SaveAndLoadManager.CurrentWorldName);

        // Obtener mundo actual del save
        _currentWorld = SaveAndLoadManager.GetStringValue(SaveAndLoadManager.CurrentWorldName);
        if (string.IsNullOrEmpty(_currentWorld))
            _currentWorld = "Neon";

        StoreManager.Instance.UpdateSkinsState?.Invoke();
        SaveAndLoadManager.Save();

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

    private void Update()
    {
#if UNITY_ANDROID
        if (Input.GetKeyUp(KeyCode.Escape))
            Application.Quit();
#endif
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

            // Usar nuevo sistema para determinar el siguiente nivel
            if (SaveAndLoadManager.HasLevelData(GameModes.Dunk, _currentWorld, i) && nextDunkLevel <= 49)
                nextDunkLevel = i + 1;

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
                // Usar nuevo sistema para verificar si el nivel anterior está completado
                button.interactable = SaveAndLoadManager.HasLevelData(GameModes.Dunk, _currentWorld, i - 1);
            #endregion

            #region HAS COINS
            var hasCoinImage = FindAndValidateComponent<Image>(button.transform, $"DunkHasCoin");
            // Usar nuevo sistema para verificar monedas
            hasCoinImage.gameObject.SetActive(
                SaveAndLoadManager.HasLevelData(GameModes.Dunk, _currentWorld, i) &&
                SaveAndLoadManager.GetLevelCoinObtained(GameModes.Dunk, _currentWorld, i));
            #endregion

            #region BEST (Objetivo del modo - en Dunk son los toques)
            var touchImage = FindAndValidateComponent<Image>(button.transform, $"DunkRecord");
            // Usar nuevo sistema para verificar objetivo completado
            touchImage.gameObject.SetActive(
                SaveAndLoadManager.HasLevelData(GameModes.Dunk, _currentWorld, i) &&
                SaveAndLoadManager.GetLevelObjectiveComplete(GameModes.Dunk, _currentWorld, i));
            #endregion

            #region WITHOUT DEATH
            var noDeathImage = FindAndValidateComponent<Image>(button.transform, $"DunkWithoutDeath");
            // Usar nuevo sistema para verificar sin muerte
            noDeathImage.gameObject.SetActive(
                SaveAndLoadManager.HasLevelData(GameModes.Dunk, _currentWorld, i) &&
                SaveAndLoadManager.GetLevelWithoutDeath(GameModes.Dunk, _currentWorld, i));
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
            ScenesManager.Instance.LoadSceneAsync($"DunkLevel{nextDunkLevel}", fadeAnimator);
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

    #region Migration Helper - Solo para desarrollo/testing
    [ContextMenu("Migrate Legacy Data to New System")]
    public void MigrateLegacyDataFromMenu()
    {
        SaveAndLoadManager.MigrateLegacyData();

        // Refrescar la UI después de la migración
        OnDunkLevelsClicked();

        Debug.Log("Legacy data migration completed from Menu. UI refreshed.");
    }
    #endregion

    #region Utility Methods
    /// <summary>
    /// Cambia el mundo actual y actualiza la UI
    /// Usar cuando implementes el selector de mundos
    /// </summary>
    public void ChangeCurrentWorld(string worldName)
    {
        _currentWorld = worldName;
        SaveAndLoadManager.SetStringValue(worldName, SaveAndLoadManager.CurrentWorldName, true);

        // Refrescar la UI de niveles
        OnDunkLevelsClicked();

        Debug.Log($"World changed to: {worldName}");
    }

    /// <summary>
    /// Obtiene el mundo actual
    /// </summary>
    public string GetCurrentWorld()
    {
        return _currentWorld;
    }

    /// <summary>
    /// Obtiene estadísticas del mundo actual
    /// </summary>
    public (int completed, int withCoins, int withoutDeath, int objectiveComplete) GetWorldStats()
    {
        int completed = 0;
        int withCoins = 0;
        int withoutDeath = 0;
        int objectiveComplete = 0;

        for (int i = 1; i <= 50; i++)
        {
            if (SaveAndLoadManager.HasLevelData(GameModes.Dunk, _currentWorld, i))
            {
                completed++;

                if (SaveAndLoadManager.GetLevelCoinObtained(GameModes.Dunk, _currentWorld, i))
                    withCoins++;

                if (SaveAndLoadManager.GetLevelWithoutDeath(GameModes.Dunk, _currentWorld, i))
                    withoutDeath++;

                if (SaveAndLoadManager.GetLevelObjectiveComplete(GameModes.Dunk, _currentWorld, i))
                    objectiveComplete++;
            }
        }

        return (completed, withCoins, withoutDeath, objectiveComplete);
    }
    #endregion

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