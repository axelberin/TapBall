using System.Collections;
using UnityEngine;
using static AudioManager;
using UtilityAddressables;

public class Spikes : ObstaclesManager
{
    [SerializeField] private float _animatorSpeed = 1;
    [SerializeField] private float _animationDelay = 3;

    private Animator _animator;
    private AudioSource _audioSource;
    private AudioClip _inSpikeClip;
    private AudioClip _upSpikeClip;
    private AudioClip _fullUpSpikeClip;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();

        AddressablesUtility.LoadAsset<AudioClip>("SpikeDownSound", clip => _inSpikeClip = clip);
        AddressablesUtility.LoadAsset<AudioClip>("SpikeUpSound", clip => _upSpikeClip = clip);
        AddressablesUtility.LoadAsset<AudioClip>("SpikeFullUpSound", clip => _fullUpSpikeClip = clip);

        if (_animator != null)
        {
            _animator.speed = _animatorSpeed;
            StartCoroutine(StartAnim(_animationDelay +
                (_animator.runtimeAnimatorController.animationClips[0].length / _animatorSpeed)));
        }
    }

    private IEnumerator StartAnim(float delay)
    {
        _animator.SetTrigger("Out");
        yield return new WaitForSeconds(delay);
        _animator.SetTrigger("In");
        yield return new WaitForSeconds(delay);

        StartCoroutine(StartAnim(delay));
    }

    public void PlaySoundFromAnimation(int soundIndex)
    {
        if (_audioSource == null)
            return;

        _audioSource.volume = SetSoundByPlayerDistance();

        if (_audioSource.volume == 0)
            return;

        AudioClip clip = null;
        switch (soundIndex)
        {
            case 0:
                if (_inSpikeClip)
                    clip = _inSpikeClip;
                break;
            case 1:
                if (_upSpikeClip)
                    clip = _upSpikeClip;
                break;
            case 2:
                if (_fullUpSpikeClip)
                    clip = _fullUpSpikeClip;
                break;
            default:
                clip = _upSpikeClip;
                break;
        }

        _audioSource.PlayOneShot(clip);
    }

    private float SetSoundByPlayerDistance()
    {
        Vector3 playerPosition = GameManager.Instance.SetGetPlayer.transform.position;
        Vector3 soundPosition = transform.position;

        float distance = Vector3.Distance(soundPosition, playerPosition);

        if (distance <= 1f)
            return 0.8f;
        else if (distance > 1.5f)
            return 0f;
        else
        {
            float normalizedDistance = (distance - 1f) / (2f - 1f); // Esto da un valor entre 0 y 1
            return Mathf.Lerp(0.8f, 0f, normalizedDistance);
        }
    }
}
