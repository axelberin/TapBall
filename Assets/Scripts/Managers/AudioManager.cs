using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioMixer _audioMixer;

    private string _mixerMusic = "MusicVolume";
    private string _mixerSFX = "SFXVolume";

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
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
        Debug.Log(value);
        _audioMixer.SetFloat(_mixerSFX, Mathf.Log10(value) * 20);
        SaveAndLoadManager.SetFloatValue(value, SaveAndLoadManager.SoundsVolumeName, true);
    }

    public void SetMusicVolume(float value)
    {
        Debug.Log(value);
        _audioMixer.SetFloat(_mixerMusic, Mathf.Log10(value) * 20);
        SaveAndLoadManager.SetFloatValue(value, SaveAndLoadManager.MusicVolumeName, true);
    }
}
