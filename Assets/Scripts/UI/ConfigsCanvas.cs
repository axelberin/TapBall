
public class ConfigsCanvas : CanvasElementLocator
{
    public static ConfigsCanvas Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        var configsBackButton = FindAndValidateButtonComponent(transform, "ConfigsBackButton");
        configsBackButton.onClick.AddListener(() =>
            UIManager.Instance.ChangeCanvas("ConfigsCanvas", GetCanvasFromGameMode(GameManager.Instance.GetCurrentGameMode)));

        var resetDataButton = FindAndValidateButtonComponent(transform, "ResetDataBTN");
        resetDataButton.onClick.AddListener(() => SaveAndLoadManager.DeleteData());

        UIManager.Instance.AddCanvas(gameObject, false);
    }

    private string GetCanvasFromGameMode(GameManager.GameModes gameMode)
    {
        return gameMode switch
        {
            GameManager.GameModes.Null => "MenuManagerCanvas",
            GameManager.GameModes.Dunk => "DunkCanvas",
            _ => "MenuManagerCanvas",
        };
    }
}
