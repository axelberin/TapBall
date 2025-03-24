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

        string skinName = SaveAndLoadManager.GetStringValue(SaveAndLoadManager.CurrentBallSkinName);
        Debug.Log($"Intentando cargar Texture2D con clave: {skinName}");
        Addressables.LoadAssetAsync<Texture2D>(SaveAndLoadManager.GetStringValue(
            SaveAndLoadManager.CurrentBallSkinName)).Completed += (operation) =>
            {
                Debug.Log("Evento 'Completed' ejecutado"); // Este log debe aparecer

                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log($"Texture2D '{skinName}' cargado correctamente.");

                    Texture2D texture = operation.Result;
                    Debug.Log($"Texture2D cargada: {texture.name}");
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    Debug.Log($"Sprite creado correctamente: {sprite}");
                    Debug.Log($"Sprite Renderer: {_spriteRenderer}");
                    _spriteRenderer.sprite = sprite;
                    Debug.Log("Sprite asignado correctamente.");
                }
                else
                {
                    Debug.LogError($"Error al cargar la textura '{skinName}' con Addressables.");
                }
            };
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance)
        {
            LevelManager.Instance.OnWinLevel -= OnWin;
            LevelManager.Instance.OnLoseLevel -= OnLose;
        }
    }

    public void OnSpriteLoaded(AsyncOperationHandle<Texture2D> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log($"Sprite cargado correctamente: {handle.Result.name}");
            Texture2D texture = handle.Result;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            _spriteRenderer.sprite = sprite;

            // Verificamos si el SpriteRenderer tiene el sprite asignado
            if (_spriteRenderer.sprite == null)
            {
                Debug.LogError("Error: El sprite cargado es nulo.");
            }
            else
            {
                Debug.Log($"Sprite asignado a {_spriteRenderer.gameObject.name}");
            }
        }
        else
        {
            Debug.LogError("Error al cargar el sprite con Addressables.");
        }
    }


    public void OnTap(Vector3 touchPos)
    {
        AddForce(touchPos);

        if (Random.Range(0, 10) < 3)
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
