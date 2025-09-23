using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UtilityAddressables;

public class PlayerController : MonoBehaviour, IPauseble, ISkinLoader
{
    [SerializeField] float _jumpForce = 3;

    private string _deathPrefabName = "Death";
    private bool _death;
    private Vector2 _velocityOnPause;

    private List<AudioClip> _tapClips = new();
    private AudioClip _deathClip;
    private Rigidbody2D _rb;
    private Collider2D _collider;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private AudioSource _audioSource;
    private SpecialSkin _specialSkin;

    void Awake()
    {
        if (!_rb)
            _rb = GetComponent<Rigidbody2D>();

        if (_collider == null)
            _collider = GetComponent<Collider2D>();

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_animator == null)
            _animator = GetComponent<Animator>();

        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

        AddressablesUtility.LoadAsset<AudioClip>("Tap01Sound", clip => _tapClips.Add(clip));
        AddressablesUtility.LoadAsset<AudioClip>("Tap02Sound", clip => _tapClips.Add(clip));
        AddressablesUtility.LoadAsset<AudioClip>("Tap03Sound", clip => _tapClips.Add(clip));
        AddressablesUtility.LoadAsset<AudioClip>("DeathSound", clip => _deathClip = clip);
    }

    private void Start()
    {
        if (GameManager.Instance)
            GameManager.Instance.SetGetPlayer = this;

        if (PauseAndResumeManager.Instance)
        {
            PauseAndResumeManager.Instance.AddResumeAction(OnResume);
            PauseAndResumeManager.Instance.AddPauseAction(OnPause);
        }

        if (LevelManager.Instance)
        {
            LevelManager.Instance.OnWinLevel += OnWin;
            LevelManager.Instance.OnLoseLevel += OnLose;
        }

        Addressables.LoadAssetAsync<GameObject>(SaveAndLoadManager.GetStringValue(
            SaveAndLoadManager.CurrentBallSkinName)).Completed += OnPrefabLoaded;
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance)
        {
            LevelManager.Instance.OnWinLevel -= OnWin;
            LevelManager.Instance.OnLoseLevel -= OnLose;
        }
    }

    public void OnPrefabLoaded(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            if (handle.Result.TryGetComponent(out Animator animator))
                _animator.runtimeAnimatorController = animator.runtimeAnimatorController;
            else
                _animator.runtimeAnimatorController = null;

            _spriteRenderer.sprite = handle.Result.GetComponent<SpriteRenderer>().sprite;

            if (handle.Result.TryGetComponent(out SpecialSkin specialSkin))
            {
                _specialSkin = gameObject.AddComponent(specialSkin.GetType()) as SpecialSkin;
                _specialSkin.Initialize();
            }
        }
        else
            Debug.LogError("Failed to load prefab.");
    }


    public void OnTap(Vector3 touchPos)
    {
        AddForce(touchPos);

        if (_animator.runtimeAnimatorController != null && Random.Range(0, 10) < 3)
            _animator.SetTrigger("Flick");

        int randomIndex = Random.Range(0, _tapClips.Count);
        if (_audioSource && _tapClips[randomIndex])
            _audioSource.PlayOneShot(_tapClips[randomIndex]);

        _specialSkin?.OnTap();
    }

    private void AddForce(Vector3 touchPos)
    {
        _rb.linearVelocity = Vector3.zero;

        Vector3 dir = (transform.position - touchPos).normalized;

        float dirX;

        if (dir.x < 0f)
            dirX = Mathf.Max(dir.x, -0.2f);
        else
            dirX = Mathf.Min(dir.x, 0.2f);

        dir = new Vector3(dirX, 0.2f, dir.z);
        _rb.AddForce(dir * _jumpForce, ForceMode2D.Impulse);
    }

    public void Death()
    {
        transform.parent = null;
        _death = true;

        _collider.enabled = false;
        Addressables.InstantiateAsync(_deathPrefabName, transform.position, transform.rotation);

        transform.position = new Vector3(100, 0);
        GameManager.Instance.SetGetCameraController.StartShake();
        StartCoroutine(DelayToLose());
    }

    private IEnumerator DelayToLose()
    {
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.StopSound(false, true);
        if (_audioSource && _deathClip)
            _audioSource.PlayOneShot(_deathClip);

        yield return new WaitForSeconds(1);
        LevelManager.Instance.OnLose();
        _collider.enabled = true;
        transform.parent = null;
    }

    public void OnResume()
    {
        if (GameManager.Instance.SetGetWorldState.GetOnInitialPause)
            return;

        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.linearVelocity = _velocityOnPause;
        _velocityOnPause = Vector2.zero;
    }

    public void OnPause()
    {
        _velocityOnPause = _rb.linearVelocity;
        _rb.bodyType = RigidbodyType2D.Static;
    }

    public void OnWin()
    {
        _rb.bodyType = RigidbodyType2D.Static;
    }

    public void OnLose()
    {
        _rb.bodyType = RigidbodyType2D.Static;
        transform.position = GameManager.Instance.SetGetWorldState.GetInitalPos;
    }

    public bool HasDeath => _death;
    public Rigidbody2D GetRigidbody => _rb;
}
