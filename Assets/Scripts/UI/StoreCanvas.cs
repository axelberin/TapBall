using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreCanvas : CanvasElementLocator
{
    public static StoreCanvas Instance { get; private set; }

    [SerializeField] private string _storeObjectPrefabName;

    private Transform _ballSkinsViewportContent;
    private Button _closeButton;
    private TextMeshProUGUI _coinsText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        _coinsText = FindAndValidateComponent<TextMeshProUGUI>(transform, "CoinsText");
        if (_coinsText != null && SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CoinsName))
            _coinsText.text = SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName).ToString();

        _ballSkinsViewportContent = FindAndValidateComponent<Transform>(transform, "BallSkinsContent");
        _closeButton = FindAndValidateComponent<Button>(transform, "StoreCloseButton");

        _closeButton.onClick.AddListener(() => UIManager.Instance.ChangeCanvas(
            "StoreCanvas", "MenuManagerCanvas"));

        UIManager.Instance.AddCanvas(gameObject, false);
    }

    public void UpdateCoinsText()
    {
        UIManager.Instance.SetText(_coinsText, SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName));
    }
}
