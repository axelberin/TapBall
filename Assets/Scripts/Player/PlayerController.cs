using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UtilityAddressables;
using static GameManager;

public class PlayerController : MonoBehaviour, IPauseble, ISkinLoader
{
    [SerializeField] float _jumpForce = 3;
    [SerializeField] float _dashForce = 6f;

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

    private Vector3 _deathPosition;

    private Animator _immunityAnimator;
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

        if(_immunityAnimator == null)
            _immunityAnimator = GetComponentInChildren<Animator>();

        AddressablesUtility.LoadAsset<AudioClip>("Tap01Sound", clip => _tapClips.Add(clip));
        AddressablesUtility.LoadAsset<AudioClip>("Tap02Sound", clip => _tapClips.Add(clip));
        AddressablesUtility.LoadAsset<AudioClip>("Tap03Sound", clip => _tapClips.Add(clip));
        AddressablesUtility.LoadAsset<AudioClip>("DeathSound", clip => _deathClip = clip);
    }

    private void Start()
    {
        if (Instance)
            Instance.SetGetPlayer = this;

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

        string key = SaveAndLoadManager.GetStringValue(
            SaveAndLoadManager.CurrentBallSkinName);

        if (!string.IsNullOrEmpty(key))
            Addressables.LoadAssetAsync<GameObject>(key).Completed += OnPrefabLoaded;


    }

    private void OnDestroy()
    {
        if (LevelManager.Instance)
        {
            LevelManager.Instance.OnWinLevel -= OnWin;
            LevelManager.Instance.OnLoseLevel -= OnLose;
        }
    }

    public void OnImmunityActivated(PowerUpManager.PowerUpType powerUp)
   {
        if (powerUp != PowerUpManager.PowerUpType.ImmunityPowerUp)
            return; 
       _immunityAnimator.SetTrigger("Activate");
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

    public void OnDash(Vector2 swipeDir)
    {
        if (swipeDir.sqrMagnitude < 0.0001f)
            return;

        swipeDir.Normalize();

        // Frenamos la velocidad actual para que el dash se sienta "seco"
        _rb.linearVelocity = Vector2.zero;

        // Impulso directo en la direcci�n del swipe, SIN salto forzado
        _rb.AddForce(swipeDir * _dashForce, ForceMode2D.Impulse);

        if (_animator.runtimeAnimatorController != null)
            _animator.SetTrigger("Flick");

        int randomIndex = Random.Range(0, _tapClips.Count);
        if (_audioSource && _tapClips[randomIndex])
            _audioSource.PlayOneShot(_tapClips[randomIndex]);

        _specialSkin?.OnTap();
    }

    private void AddForce(Vector3 touchPos)
    {
        _rb.linearVelocity = Vector3.zero;

        Vector3 dir = (touchPos - transform.position).normalized;

        float dirX;

        if (dir.x < 0f)
            dirX = Mathf.Max(dir.x, -0.1f);
        else
            dirX = Mathf.Min(dir.x, 0.1f);

        dir = new Vector3(dirX, 0.2f, dir.z);
        _rb.AddForce(dir * _jumpForce, ForceMode2D.Impulse);
    }

    public void Death()
    {
        if (PowerUpManager.Instance.PowerUpImmunityEnabled)
            return;

        transform.parent = null;
        _death = true;

        _collider.enabled = false;
        Addressables.InstantiateAsync(_deathPrefabName, transform.position, transform.rotation);
        _deathPosition = transform.position;

        transform.position = new Vector3(100, 0);
        Instance.SetGetCameraController.StartShake();
        LevelManager.Instance.OnPreLoseLevel?.Invoke();
        Instance.SetGetTapController.SetGetTapEnabled = false;
        StartCoroutine(DelayToLose());
    }

    private IEnumerator DelayToLose()
    {
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.StopSound(false, true);
        if (_audioSource && _deathClip)
            _audioSource.PlayOneShot(_deathClip);
        LevelCanvas.Instance.GetSetImmunityButton(false);
        LevelCanvas.Instance.DeactivateInteractablePowerUpButtons(false);

        yield return new WaitForSeconds(1);
        LevelCanvas.Instance.ActivateRevivePowerUI();

        yield return (StartCoroutine(RejectRevivalPowerUp(3)));
        LevelCanvas.Instance.DeactivateRevivePowerUI();
    }

    private IEnumerator RejectRevivalPowerUp(float time)
    {
        while (time > 0)
        {
            LevelCanvas.Instance.UpdateTextPowerUpPopUpTimeCounter(time);
            time -= Time.deltaTime;
            yield return null;
        }

        LevelManager.Instance.OnRejectRevival?.Invoke();
        Debug.Log("Rejected");
        LevelManager.Instance.OnLose();
        _collider.enabled = true;
        transform.parent = null;
        Instance.SetGetTapController.SetGetTapEnabled = true;
        LevelCanvas.Instance.GetSetImmunityButton(true);
        LevelCanvas.Instance.DeactivateInteractablePowerUpButtons(true);
    }

    public void OnRejectRevivalPowerUp()
    {
        StartCoroutine(RejectRevivalPowerUp(2));
        
    }

    public void AcceptRevivalPowerUp()
    {
        StopAllCoroutines();

        switch (Instance.GetCurrentGameMode)
        {
            case GameModes.Time:
                if (Instance.SetGetWorldState.GetRemainingTime <= 3)
                {
                    Instance.SetGetWorldState.AddCountToTimer(3);
                }
                break;
            case GameModes.OneTouch:
                if (Instance.SetGetTapController.SetGetTapCount <= 3)
                {
                    Instance.SetGetTapController.AddTouchesFromBubbles(3);
                }
                break;
        }

        PowerUpManager.Instance.SelectPowerUp(PowerUpManager.PowerUpType.ImmunityPowerUp);
        _death = false;
        transform.position = _deathPosition;
        _collider.enabled = true;

        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.linearVelocity = _velocityOnPause;
        _velocityOnPause = Vector2.zero;
        Instance.SetGetTapController.SetGetTapEnabled = true;
        LevelManager.Instance.OnAcceptRevival?.Invoke();
        Debug.Log("Reviving");
        LevelCanvas.Instance.DeactivateInteractablePowerUpButtons(true);
    }

    public void OnResume()
    {
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
        transform.position = Instance.SetGetWorldState.GetInitalPos;
    }

    public bool HasDeath => _death;
    public Rigidbody2D GetRigidbody => _rb;
}
