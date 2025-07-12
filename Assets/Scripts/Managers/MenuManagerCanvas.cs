using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerCanvas : CanvasElementLocator
{
    private GameObject _menuPanel;
    //private GameObject _selectModePanel;
    private GameObject _dunkLevelsPanel;

    void Start()
    {
        Application.targetFrameRate = 60;

        GameManager.Instance.SelectGameMode(1);     //TODO: Mandar un 0 y luego cambiar el modo desde el menu

        _menuPanel = FindAndValidateGameObjectComponent(transform, "MenuPanel");
        //_selectModePanel = FindAndValidateGameObjectComponent(transform, "SelectModePanel");
        _dunkLevelsPanel = FindAndValidateGameObjectComponent(transform, "DunkLevelsPanel");

        var levelsSelectorButton = FindAndValidateComponent<Button>(transform, "LevelsSelectorButton");
        levelsSelectorButton.onClick.AddListener(() =>
        {
            _dunkLevelsPanel.SetActive(true);
            _menuPanel.SetActive(false);
            GameManager.Instance.SelectGameMode(1);
        });

        if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CurrentBallSkinName))
        {
            SaveAndLoadManager.SetStringValue("BallBasicSkin", SaveAndLoadManager.CurrentBallSkinName);
            SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.ObtainedBallSkins + "BallBasicSkin", true);
        }

        var dunkCloseButton = FindAndValidateComponent<Button>(transform, "DunkCloseButton");
        dunkCloseButton.onClick.AddListener(() =>
        {
            _dunkLevelsPanel.SetActive(false);
            _menuPanel.SetActive(true);
            GameManager.Instance.SelectGameMode(0);
        });

        var storeButton = FindAndValidateComponent<Button>(transform, "StoreBTN");
        storeButton.onClick.AddListener(() =>
            UIManager.Instance.ChangeCanvas("MenuManagerCanvas", "StoreCanvas"));

        var configsButton = FindAndValidateComponent<Button>(transform, "ConfigsButton");
        configsButton.onClick.AddListener(() =>
            UIManager.Instance.ChangeCanvas("MenuManagerCanvas", "ConfigsCanvas"));

        PauseAndResumeManager.Instance.RestartResumeAction();
        PauseAndResumeManager.Instance.RestartPauseAction();

        UIManager.Instance.AddCanvas(gameObject, true);
        LevelManager.Instance.ResetCoins();
        AudioManager.Instance.PlayMusicByType(AudioManager.MusicClipType.MenuMusic);

        OnDunkLevelsClicked();
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

        for (int i = 1; i <= 100; i++)
        {
            var button = FindAndValidateComponent<Button>(transform, $"DunkLevel");

            if (button == null)
                break;

            button.name = $"DunkLevel{i}";
            if (SaveAndLoadManager.ContainsKey(SaveAndLoadManager.DunkLevelName + i))
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
            ScenesManager.Instance.LoadSceneAsync((SaveAndLoadManager.DunkLevelName +
                nextDunkLevel).Replace("_", ""), fadeAnimator));

        var nextLevelText = FindAndValidateComponent<TextMeshProUGUI>(transform, "NextLevelNumberText");
        nextLevelText.text = $"{nextDunkLevel}";
    }
    #endregion
}
