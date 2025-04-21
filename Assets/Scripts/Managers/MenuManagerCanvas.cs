using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerCanvas : CanvasElementLocator
{
    private Button[] _dunkLevelsButtons;
    private Image[] _dunkTouchesComplete;
    private Image[] _dunkWithoutDeath;
    private Image[] _dunkHasCoins;

    private GameObject _menuPanel;
    private GameObject _selectModePanel;
    private GameObject _dunkLevelsPanel;
    private GameObject _creditsPanel;
    private Button _dunkModeButton;
    private Button _dunkCloseButton;
    private Button _storeButton;
    private Button _configsButton;

    private int _maxDunkLevels;

    void Start()
    {
        Application.targetFrameRate = 60;

        GameManager.Instance.SelectGameMode(0);

        _menuPanel = FindAndValidateGameObjectComponent(transform, "MenuPanel");
        _dunkLevelsPanel = FindAndValidateGameObjectComponent(transform, "DunkLevelsPanel");
        _selectModePanel = FindAndValidateGameObjectComponent(transform, "SelectModePanel");
        _creditsPanel = FindAndValidateGameObjectComponent(transform, "CreditsPanel");

        if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CurrentBallSkinName))
        {
            SaveAndLoadManager.SetStringValue("BallBasicSkin", SaveAndLoadManager.CurrentBallSkinName);
            SaveAndLoadManager.SetIntValue(1, SaveAndLoadManager.ObtainedBallSkins + "BallBasicSkin", true);
        }

        var playButton = FindAndValidateButtonComponent(transform, "PlayBTN");
        playButton.onClick.AddListener(() =>
        {
            _selectModePanel.SetActive(true);
            _menuPanel.SetActive(false);
        });

        _dunkModeButton = FindAndValidateButtonComponent(transform, "DunkModeButton");
        _dunkModeButton.onClick.AddListener(() =>
        {
            _dunkLevelsPanel.SetActive(true);
            _selectModePanel.SetActive(false);
            GameManager.Instance.SelectGameMode(1);
        });

        var backModePanelButton = FindAndValidateButtonComponent(transform, "ModePanelBackButton");
        backModePanelButton.onClick.AddListener(() =>
        {
            _selectModePanel.SetActive(false);
            _menuPanel.SetActive(true);
        });

        _dunkCloseButton = FindAndValidateButtonComponent(transform, "DunkCloseButton");
        _dunkCloseButton.onClick.AddListener(() =>
        {
            _dunkLevelsPanel.SetActive(false);
            _selectModePanel.SetActive(true);
            GameManager.Instance.SelectGameMode(0);
        });

        var creditsButton = FindAndValidateButtonComponent(transform, "CreditsBTN");
        creditsButton.onClick.AddListener(() =>
        {
            _menuPanel.SetActive(false);
            _creditsPanel.SetActive(true);
        });

        var creditsCloseButton = FindAndValidateButtonComponent(transform, "CreditsCloseButton");
        creditsCloseButton.onClick.AddListener(() =>
        {
            _creditsPanel.SetActive(false);
            _menuPanel.SetActive(true);
        });

        _storeButton = FindAndValidateButtonComponent(transform, "StoreBTN");
        _storeButton.onClick.AddListener(() =>
            UIManager.Instance.ChangeCanvas("MenuManagerCanvas", "StoreCanvas"));

        _configsButton = FindAndValidateButtonComponent(transform, "ConfigsButton");
        _configsButton.onClick.AddListener(() =>
            UIManager.Instance.ChangeCanvas("MenuManagerCanvas", "ConfigsCanvas"));

        PauseAndResumeManager.Instance.RestartResumeAction();
        PauseAndResumeManager.Instance.RestartPauseAction();

        UIManager.Instance.AddCanvas(gameObject, true);
        LevelManager.Instance.ResetCoins();
        AudioManager.Instance.PlayMusicByType(AudioManager.MusicClipType.MenuMusic);

        OnDunkLevelsClicked();
    }

    #region DUNK
    private void OnDunkLevelsClicked()
    {
        #region UNLOCK LEVELS
        var dunkLevelsButtons = new List<Button>();
        for (int i = 1; i <= 100; i++)
        {
            var button = FindAndValidateButtonComponent(transform, $"DunkLevel{i}");

            if (button == null)
                break;

            dunkLevelsButtons.Add(button);
        }

        if (dunkLevelsButtons.Count > 0)
            _dunkLevelsButtons = dunkLevelsButtons.ToArray();

        var fadeAnimator = FindAndValidateGameObjectComponent(transform, "Fade").GetComponent<Animator>();

        for (int i = 0; i < _dunkLevelsButtons.Length; i++)
        {
            int levelIndex = i; // Variable temporal para capturar el valor actual de 'i'
            _dunkLevelsButtons[i].onClick.AddListener(() =>
            {
                _dunkLevelsButtons[levelIndex].interactable = false;
                UIManager.Instance.ClearCnavasesList();
                ScenesManager.Instance.LoadSceneAsync($"DunkLevel{levelIndex + 1}", fadeAnimator);
                AudioManager.Instance.StopMusic();
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.PlayLevelSound);
            });

            if (i == 0)
                _dunkLevelsButtons[i].interactable = true;
            else
                _dunkLevelsButtons[i].interactable = SaveAndLoadManager.ContainsKey(
                    SaveAndLoadManager.DunkLevelName + (i - 1));
        }

        _maxDunkLevels = _dunkLevelsButtons.Length;
        #endregion
        #region HAS COINS
        var dunkHasCoins = new List<Image>();
        for (int i = 1; i <= _maxDunkLevels; i++)
        {
            var hasCoinImage = FindAndValidateImageComponent(transform, $"DunkHasCoin{i}");

            if (hasCoinImage == null)
                break;

            dunkHasCoins.Add(hasCoinImage);
        }

        if (dunkHasCoins.Count > 0)
            _dunkHasCoins = dunkHasCoins.ToArray();

        for (int i = 0; i < _dunkHasCoins.Length; i++)
        {
            _dunkHasCoins[i].gameObject.SetActive(
                SaveAndLoadManager.ContainsKey(SaveAndLoadManager.DunkLevelName + i) &&
                SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinNameByLevel + GameManager.GameModes.Dunk + (i + 1)) == 1);
        }

        #endregion
        #region BEST
        var dunkTouchesComplete = new List<Image>();
        for (int i = 1; i <= _maxDunkLevels; i++)
        {
            var touchImage = FindAndValidateImageComponent(transform, $"DunkRecord{i}");

            if (touchImage == null)
                break;

            dunkTouchesComplete.Add(touchImage);
        }

        if (dunkTouchesComplete.Count > 0)
            _dunkTouchesComplete = dunkTouchesComplete.ToArray();

        for (int i = 0; i < _dunkTouchesComplete.Length; i++)
        {
            _dunkTouchesComplete[i].gameObject.SetActive(
                SaveAndLoadManager.ContainsKey(SaveAndLoadManager.DunkLevelName + i) &&
                SaveAndLoadManager.GetIntValue(SaveAndLoadManager.DunkTouchesCompleteName + i) == 1);
        }
        #endregion
        #region WITHOUT DEATH
        var dunkWithoutDeathImage = new List<Image>();
        for (int i = 1; i <= _maxDunkLevels; i++)
        {
            var image = FindAndValidateImageComponent(transform, $"DunkWithoutDeath{i}");

            if (image == null)
                break;

            dunkWithoutDeathImage.Add(image);
        }

        if (dunkWithoutDeathImage.Count > 0)
            _dunkWithoutDeath = dunkWithoutDeathImage.ToArray();

        for (int i = 0; i < _dunkWithoutDeath.Length; i++)
        {
            _dunkWithoutDeath[i].gameObject.SetActive(
                SaveAndLoadManager.ContainsKey(SaveAndLoadManager.DunkLevelName + i) &&
                SaveAndLoadManager.GetIntValue(SaveAndLoadManager.DunkWithoutDeathName + i) == 1);
        }
        #endregion
    }
    #endregion
}
