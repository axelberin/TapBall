using UnityEngine.UI;

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
        var configsBackButton = FindAndValidateComponent<Button>(transform, "ConfigsBackButton");
        configsBackButton.onClick.AddListener(() =>
            UIManager.Instance.ChangeCanvas("ConfigsCanvas", GetCanvasFromGameMode(GameManager.Instance.GetCurrentGameMode)));

        var resetDataButton = FindAndValidateComponent<Button>(transform, "ResetDataBTN");
#if UNITY_EDITOR
        if (resetDataButton != null)
            resetDataButton.onClick.AddListener(() =>
            {
                SaveAndLoadManager.DeleteData();
                ScenesManager.Instance.LoadScene(ScenesManager.Instance.GetCurrentSceneName);
            });
#else
        resetDataButton.gameObject.SetActive(false);
#endif
        var soundImage = FindAndValidateComponent<Image>(transform, "SoundImage");
        var soundMuteImage = FindAndValidateComponent<Image>(transform, "SoundMuteImage");
        soundImage.gameObject.SetActive(!AudioManager.Instance.GetSoundIsMuted);
        soundMuteImage.gameObject.SetActive(AudioManager.Instance.GetSoundIsMuted);
        var musicImage = FindAndValidateComponent<Image>(transform, "MusicImage");
        var musicMuteImage = FindAndValidateComponent<Image>(transform, "MusicMuteImage");
        musicImage.gameObject.SetActive(!AudioManager.Instance.GetMusicIsMuted);
        musicMuteImage.gameObject.SetActive(AudioManager.Instance.GetMusicIsMuted);

        var soundMuteButton = FindAndValidateComponent<Button>(transform, "SoundMuteButton");
        soundMuteButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.SetSoundVolume(!AudioManager.Instance.GetSoundIsMuted);
            soundImage.gameObject.SetActive(!AudioManager.Instance.GetSoundIsMuted);
            soundMuteImage.gameObject.SetActive(AudioManager.Instance.GetSoundIsMuted);
        });

        var musicMuteButton = FindAndValidateComponent<Button>(transform, "MusicMuteButton");
        musicMuteButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.SetMusicVolume(!AudioManager.Instance.GetMusicIsMuted);
            musicImage.gameObject.SetActive(!AudioManager.Instance.GetMusicIsMuted);
            musicMuteImage.gameObject.SetActive(AudioManager.Instance.GetMusicIsMuted);
        });

        var leftArrowButton = FindAndValidateComponent<Button>(transform, "LeftArrowButton");
        leftArrowButton.onClick.AddListener(() => LanguageManager.Instance.ChangeLanguage(-1));

        var rightArrowButton = FindAndValidateComponent<Button>(transform, "RightArrowButton");
        rightArrowButton.onClick.AddListener(() => LanguageManager.Instance.ChangeLanguage(1));

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
