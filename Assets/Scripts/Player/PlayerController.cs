using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerController : MonoBehaviour, IPauseble, ISkinLoader
{
    [SerializeField] float _jumpForce = 3;
    [SerializeField] string _deathPrefabName = "Death";
    [SerializeField] List<AudioClip> _tapClips = new List<AudioClip>();
    [SerializeField] AudioClip _deathClip;

    private bool _death;
    private Vector2 _velocityOnPause;

    private Rigidbody2D _rb;
    private Collider2D _collider;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private AudioSource _audioSource;

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
            _spriteRenderer.sprite = handle.Result.GetComponent<SpriteRenderer>().sprite;
            _animator.runtimeAnimatorController = handle.Result.GetComponent<Animator>().runtimeAnimatorController;
        }
        else
            Debug.LogError("Failed to load prefab.");
    }


    public void OnTap(Vector3 touchPos)
    {
        AddForce(touchPos);

        if (Random.Range(0, 10) < 3)
            _animator.SetTrigger("Flick");

        int randomIndex = Random.Range(0, _tapClips.Count);
        if (_audioSource && _tapClips[randomIndex])
            _audioSource.PlayOneShot(_tapClips[randomIndex]);
    }

    private void AddForce(Vector3 touchPos)
    {
        _rb.velocity = Vector3.zero;

        Vector3 dir = (transform.position - touchPos).normalized;

        float dirX;

        if (dir.x < 0f)
            dirX = Mathf.Max(dir.x, -0.2f);
        else
            dirX = Mathf.Min(dir.x, 0.2f);

        dir = new Vector3(dirX, dir.y < 0 ? -0.2f : 0.2f, dir.z);
        _rb.AddForce(dir * _jumpForce, ForceMode2D.Impulse);
    }

    public void Death()
    {
        transform.parent = null;
        _death = true;

        _collider.enabled = false;
        Addressables.InstantiateAsync(_deathPrefabName, transform.position, transform.rotation);

        LevelManager.Instance.OnLose();
        _collider.enabled = true;

        if (_audioSource && _deathClip)
            _audioSource.PlayOneShot(_deathClip);
    }

    public void OnResume()
    {
        if (GameManager.Instance.SetGetWorldState.GetOnInitialPause)
            return;

        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.velocity = _velocityOnPause;
        _velocityOnPause = Vector2.zero;
    }

    public void OnPause()
    {
        _velocityOnPause = _rb.velocity;
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
