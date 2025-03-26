using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private AudioClip _buttonsSoundClip;

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

    public void PlayButtonsSound()
    {
        if (_buttonsSoundClip != null && _audioSource != null)
            _audioSource.PlayOneShot(_buttonsSoundClip);
    }
}
