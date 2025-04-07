using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public enum AudioClipType
    {
        ButtonsSound,
        PlayLevelSound,
        WinSound,
        PurchaseSound,
        EquipSound,
        RejectionSound,
        AchivmentSound
    };

    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private AudioClip _buttonsSoundClip;
    [SerializeField] private AudioClip _playLevelSoundClip;
    [SerializeField] private AudioClip _winSoundClip;
    [SerializeField] private AudioClip _purchaseSoundClip;
    [SerializeField] private AudioClip _equipSoundClip;
    [SerializeField] private AudioClip _rejectionSoundClip;
    [SerializeField] private AudioClip _achivmentSoundClip;

    private string _mixerMusic = "MusicVolume";
    private string _mixerSFX = "SFXVolume";

    private AudioSource _audioSource;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (SaveAndLoadManager.ContainsKey(SaveAndLoadManager.SoundsVolumeName))
            SetSoundVolume(SaveAndLoadManager.GetFloatValue(SaveAndLoadManager.SoundsVolumeName));
        else
            SetSoundVolume(1);

        if (SaveAndLoadManager.ContainsKey(SaveAndLoadManager.MusicVolumeName))
            SetMusicVolume(SaveAndLoadManager.GetFloatValue(SaveAndLoadManager.MusicVolumeName));
        else
            SetMusicVolume(1);

        PauseAndResumeManager.Instance.AddPauseAction(() => _audioSource.Pause());
        PauseAndResumeManager.Instance.AddResumeAction(() => _audioSource.UnPause());
    }

    public void SetSoundVolume(float value)
    {
        _audioMixer.SetFloat(_mixerSFX, Mathf.Log10(value) * 20);
        SaveAndLoadManager.SetFloatValue(value, SaveAndLoadManager.SoundsVolumeName, true);
    }

    public void SetMusicVolume(float value)
    {
        _audioMixer.SetFloat(_mixerMusic, Mathf.Log10(value) * 20);
        SaveAndLoadManager.SetFloatValue(value, SaveAndLoadManager.MusicVolumeName, true);
    }

    public void PlaySoundByType(AudioClipType clipType)
    {
        var clip = GetClipByClipType(clipType);

        if (clip != null && _audioSource != null)
            _audioSource.PlayOneShot(clip);
        else
            Debug.LogError("Audio source or audio clip not found.");
    }

    private AudioClip GetClipByClipType(AudioClipType clipType)
    {
        return clipType switch
        {
            AudioClipType.ButtonsSound => _buttonsSoundClip,
            AudioClipType.PlayLevelSound => _playLevelSoundClip,
            AudioClipType.WinSound => _winSoundClip,
            AudioClipType.PurchaseSound => _purchaseSoundClip,
            AudioClipType.EquipSound => _equipSoundClip,
            AudioClipType.RejectionSound => _rejectionSoundClip,
            AudioClipType.AchivmentSound => _achivmentSoundClip,
            _ => null,
        };
    }
}
