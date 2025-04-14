using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour, IPauseble
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
        AchivmentSound,
        CountDownSound
    };

    public enum MusicClipType
    {
        MenuMusic,
        DunkMusic
    };

    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private List<AudioClip> _audioClipList;
    [SerializeField] private List<AudioClip> _musicClipList;

    private string _mixerMusic = "MusicVolume";
    private string _mixerSFX = "SFXVolume";
    private string _mixerUI = "UIVolume";

    private AudioSource _soundsAudioSource;
    private AudioSource _musicAudioSource;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        if (_soundsAudioSource == null || _musicAudioSource == null)
        {
            _soundsAudioSource = GetComponentsInChildren<AudioSource>().
                FirstOrDefault(a => a.gameObject.name == "SoundsAudioSource");

            _musicAudioSource = GetComponentsInChildren<AudioSource>().
                FirstOrDefault(a => a.gameObject.name == "MusicAudioSource");
        }
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

        LevelManager.Instance.OnWinLevel += StopMusic;
    }

    public void SetSoundVolume(float value)
    {
        _audioMixer.SetFloat(_mixerSFX, Mathf.Log10(value) * 20);
        _audioMixer.SetFloat(_mixerUI, Mathf.Log10(value) * 20);
        SaveAndLoadManager.SetFloatValue(value, SaveAndLoadManager.SoundsVolumeName, true);
    }

    public void SetMusicVolume(float value)
    {
        _audioMixer.SetFloat(_mixerMusic, Mathf.Log10(value) * 20);
        SaveAndLoadManager.SetFloatValue(value, SaveAndLoadManager.MusicVolumeName, true);
    }

    public void PlaySoundByType(AudioClipType clipType)
    {
        var clip = _audioClipList[(int)clipType];

        if (clip != null && _soundsAudioSource != null)
            _soundsAudioSource.PlayOneShot(clip);
        else
            Debug.LogError("Audio source or audio clip not found.");
    }

    public void PlayMusicByType(MusicClipType musicType)
    {
        var clip = _musicClipList[(int)musicType];

        if (clip != null && _musicAudioSource != null)
        {
            _musicAudioSource.clip = clip;
            _musicAudioSource.Play();
        }
        else
            Debug.LogError("Audio source or audio clip not found.");
    }

    public void StopSound()
    {
        if (_soundsAudioSource != null)
            _soundsAudioSource.Stop();
    }

    public void StopMusic()
    {
        if (_musicAudioSource != null)
            _musicAudioSource.Stop();
    }

    public void OnResume()
    {
        if (_soundsAudioSource != null)
            _soundsAudioSource.UnPause();
    }

    public void OnPause()
    {
        if (_soundsAudioSource != null)
            _soundsAudioSource.Pause();
    }
}
