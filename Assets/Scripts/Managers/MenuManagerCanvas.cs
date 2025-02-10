using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerCanvas : CanvasElementLocator
{
    [SerializeField] bool _deleteDataOnStart = false;
    private Button[] _dunkLevelsButtons;
    private TextMeshProUGUI[] _dunkLevelsRecords;
    private Image[] _dunkWithoutDeath;

    private GameObject _menuPanel;
    private GameObject _dunkLevelsPanel;
    private Button _dunkLevelsButton;
    private Button _dunkCloseButton;
    private Button _storeButton;

    private int _maxDunkLevels;

    void Start()
    {
        Application.targetFrameRate = 60;
#if UNITY_EDITOR
        if (_deleteDataOnStart)
            SaveAndLoadManager.DeleteData();
#endif

        var coinsText = FindAndValidateTextComponent(transform, "CoinsText");
        if (coinsText != null && SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CoinsName))
            coinsText.text = SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName).ToString();

        _menuPanel = FindAndValidateGameObjectComponent(transform, "MenuPanel");
        _dunkLevelsPanel = FindAndValidateGameObjectComponent(transform, "DunkLevelsPanel");

        _dunkLevelsButton = FindAndValidateButtonComponent(transform, "DunkBTN");
        _dunkLevelsButton.onClick.AddListener(() =>
        {
            _dunkLevelsPanel.SetActive(true);
            _menuPanel.SetActive(false);
            GameManager.Instance.SelectGameMode(0);
        });

        _dunkCloseButton = FindAndValidateButtonComponent(transform, "DunkCloseButton");
        _dunkCloseButton.onClick.AddListener(() =>
        {
            _dunkLevelsPanel.SetActive(false);
            _menuPanel.SetActive(true);
        });

        _storeButton = FindAndValidateButtonComponent(transform, "StoreBTN");
        _storeButton.onClick.AddListener(() =>
        {
            _menuPanel.SetActive(false);
            UIManager.Instance.ChangeCanvas("MenuManagerCanvas", "StoreCanvas");
        });

        PauseAndResumeManager.Instance.RestartResumeAction();
        PauseAndResumeManager.Instance.RestartPauseAction();
        UIManager.Instance.AddCanvas(gameObject, true);

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

        for (int i = 0; i < _dunkLevelsButtons.Length; i++)
        {
            int levelIndex = i; // Variable temporal para capturar el valor actual de 'i'
            _dunkLevelsButtons[i].onClick.AddListener(() =>
                ScenesManager.Instance.LoadScene($"DunkLevel{levelIndex + 1}"));

            if (i == 0)
                _dunkLevelsButtons[i].interactable = true;
            else
                _dunkLevelsButtons[i].interactable = SaveAndLoadManager.ContainsKey(
                    SaveAndLoadManager.DunkLevelName + (i - 1));
        }

        _maxDunkLevels = _dunkLevelsButtons.Length;
        #endregion
        #region BEST
        var dunkBestTexts = new List<TextMeshProUGUI>();
        for (int i = 1; i <= _maxDunkLevels; i++)
        {
            var text = FindAndValidateTextComponent(transform, $"DunkRecord{i}");

            if (text == null)
                break;

            dunkBestTexts.Add(text);
        }

        if (dunkBestTexts.Count > 0)
            _dunkLevelsRecords = dunkBestTexts.ToArray();

        for (int i = 0; i < _dunkLevelsRecords.Length; i++)
        {
            if (_dunkLevelsRecords[i] != null && SaveAndLoadManager.ContainsKey(
                SaveAndLoadManager.DunkLevelName + i))
                _dunkLevelsRecords[i].text = SaveAndLoadManager.GetIntValue(
                    SaveAndLoadManager.DunkBestName + i).ToString();
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
            _dunkWithoutDeath[i].gameObject.SetActive(_dunkWithoutDeath[i]
                && SaveAndLoadManager.ContainsKey(SaveAndLoadManager.DunkLevelName + i) &&
                SaveAndLoadManager.GetIntValue(SaveAndLoadManager.DunkWithoutDeathName + i) == 1);
        }
        #endregion
    }
    #endregion
}
