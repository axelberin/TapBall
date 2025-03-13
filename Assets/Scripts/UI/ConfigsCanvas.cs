
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
        if (resetDataButton != null)
            resetDataButton.onClick.AddListener(() => SaveAndLoadManager.DeleteData());

        var soundsSlide = FindAndValidateScrollbarComponent(transform, "SoundsSlider");
        soundsSlide.onValueChanged.AddListener((value) => AudioManager.Instance.SetSoundVolume(soundsSlide.value));
        soundsSlide.value = SaveAndLoadManager.GetFloatValue(SaveAndLoadManager.SoundsVolumeName);

        var musicSlide = FindAndValidateScrollbarComponent(transform, "MusicSlider");
        musicSlide.onValueChanged.AddListener((value) => AudioManager.Instance.SetMusicVolume(musicSlide.value));
        musicSlide.value = SaveAndLoadManager.GetFloatValue(SaveAndLoadManager.MusicVolumeName);

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
