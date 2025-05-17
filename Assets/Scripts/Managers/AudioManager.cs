using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UtilityAddressables;

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

    private AudioMixer _audioMixer;
    private Dictionary<AudioClipType, AudioClip> _soundClipsByEnum = new();
    private Dictionary<MusicClipType, AudioClip> _musicClipsByEnum = new();

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

        AddressablesUtility.LoadAsset<AudioMixer>("AudioMixer", mixer => _audioMixer = mixer);

        AddressablesUtility.LoadAsset<AudioClip>(AudioClipType.ButtonsSound.ToString(),
            clip => _soundClipsByEnum.Add(AudioClipType.ButtonsSound, clip));
        AddressablesUtility.LoadAsset<AudioClip>(AudioClipType.PlayLevelSound.ToString(),
            clip => _soundClipsByEnum.Add(AudioClipType.PlayLevelSound, clip));
        AddressablesUtility.LoadAsset<AudioClip>(AudioClipType.WinSound.ToString(),
            clip => _soundClipsByEnum.Add(AudioClipType.WinSound, clip));
        AddressablesUtility.LoadAsset<AudioClip>(AudioClipType.PurchaseSound.ToString(),
            clip => _soundClipsByEnum.Add(AudioClipType.PurchaseSound, clip));
        AddressablesUtility.LoadAsset<AudioClip>(AudioClipType.EquipSound.ToString(),
            clip => _soundClipsByEnum.Add(AudioClipType.EquipSound, clip));
        AddressablesUtility.LoadAsset<AudioClip>(AudioClipType.RejectionSound.ToString(),
            clip => _soundClipsByEnum.Add(AudioClipType.RejectionSound, clip));
        AddressablesUtility.LoadAsset<AudioClip>(AudioClipType.AchivmentSound.ToString(),
            clip => _soundClipsByEnum.Add(AudioClipType.AchivmentSound, clip));
        AddressablesUtility.LoadAsset<AudioClip>(AudioClipType.CountDownSound.ToString(),
            clip => _soundClipsByEnum.Add(AudioClipType.CountDownSound, clip));
        AddressablesUtility.LoadAsset<AudioClip>(MusicClipType.MenuMusic.ToString()
            , clip => _musicClipsByEnum.Add(MusicClipType.MenuMusic, clip));
        AddressablesUtility.LoadAsset<AudioClip>(MusicClipType.DunkMusic.ToString(),
            clip => _musicClipsByEnum.Add(MusicClipType.DunkMusic, clip));
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
        if (_audioMixer == null)
            return;

        _audioMixer.SetFloat(_mixerSFX, Mathf.Log10(value) * 20);
        _audioMixer.SetFloat(_mixerUI, Mathf.Log10(value) * 20);
        SaveAndLoadManager.SetFloatValue(value, SaveAndLoadManager.SoundsVolumeName, true);
    }

    public void SetMusicVolume(float value)
    {
        if (_audioMixer == null)
            return;

        _audioMixer.SetFloat(_mixerMusic, Mathf.Log10(value) * 20);
        SaveAndLoadManager.SetFloatValue(value, SaveAndLoadManager.MusicVolumeName, true);
    }

    public void PlaySoundByType(AudioClipType clipType)
    {
        if (_soundClipsByEnum == null || !_soundClipsByEnum.ContainsKey(clipType)
            || _soundClipsByEnum[clipType] == null)
            return;

        var clip = _soundClipsByEnum[clipType];

        if (clip != null && _soundsAudioSource != null)
            _soundsAudioSource.PlayOneShot(clip);
        else
            Debug.LogError("Audio source or audio clip not found.");
    }

    public void PlayMusicByType(MusicClipType musicType)
    {
        if (_musicClipsByEnum == null || !_musicClipsByEnum.ContainsKey(musicType) ||
            _musicClipsByEnum[musicType] == null)
            return;

        var clip = _musicClipsByEnum[musicType];

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
