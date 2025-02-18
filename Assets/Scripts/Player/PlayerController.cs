using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerController : MonoBehaviour, IPauseble, ISkinLoader
{
    [SerializeField] float _jumpForce = 3;
    [SerializeField] string _deathPrefabName = "Death";

    private bool _death;
    private Vector2 _velocityOnPause;

    private Rigidbody2D _rb;
    private Collider2D _collider;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

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

        if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CurrentBallSkin))
            SaveAndLoadManager.SetStringValue(SaveAndLoadManager.CurrentBallSkin, SaveAndLoadManager.CurrentBallSkin);

        Addressables.LoadAssetAsync<Sprite>(SaveAndLoadManager.GetStringValue(
            SaveAndLoadManager.CurrentBallSkin)).Completed += OnSpriteLoaded;
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance)
        {
            LevelManager.Instance.OnWinLevel -= OnWin;
            LevelManager.Instance.OnLoseLevel -= OnLose;
        }
    }

    public void OnSpriteLoaded(AsyncOperationHandle<Sprite> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _spriteRenderer.sprite = handle.Result;
        }
    }

    public void OnTap(Vector3 touchPos)
    {
        AddForce(touchPos);

        if (Random.Range(0, 5) > 2)
            _animator.SetTrigger("Flick");
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
        _death = true;

        _collider.enabled = false;
        Addressables.InstantiateAsync(_deathPrefabName, transform.position, transform.rotation);

        LevelManager.Instance.OnLose();
        _collider.enabled = true;
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
