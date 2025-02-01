using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerController : MonoBehaviour, IPauseble
{
    [SerializeField] float _jumpForce = 3;
    [SerializeField] string _deathPrefabName = "Death";

    private bool _death;
    private Vector2 _velocityOnPause;

    private Rigidbody2D _rb;
    private Collider2D _collider;
    private SpriteRenderer _spriteRenderer;

    void Awake()
    {
        if (!_rb)
            _rb = GetComponent<Rigidbody2D>();

        if (_collider == null)
            _collider = GetComponent<Collider2D>();

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
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

        if (SaveAndLoadManager.GetStringValue(SaveAndLoadManager.CurrentBallSkin) == default)
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

    private void OnSpriteLoaded(AsyncOperationHandle<Sprite> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _spriteRenderer.sprite = handle.Result;
        }
    }

    public void AddForce(Vector3 touchPos)
    {
        _rb.velocity = Vector3.zero;

        Vector3 dir = (transform.position - touchPos).normalized;

        float clampY;

        if (dir.y >= 0)
            clampY = Mathf.Clamp(dir.y, 0.2f, 0.3f);
        else
            clampY = Mathf.Clamp(dir.y, -0.3f, -0.2f);

        dir = new Vector3(dir.x, clampY, dir.z);
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
