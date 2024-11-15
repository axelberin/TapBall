using System;
using UnityEngine;
using System.Linq;

public class WorldStateController : MonoBehaviour, IPauseble
{
    private Action OnUpdate = delegate { };

    private int _level;
    private float _timeToWin = 3;
    private float _timeToStart = 0;
    private bool _onPause = false;
    private Vector3 _playerInitialPos;

    private PlayerController _playerController;
    private BaseController _baseController;

    private void Start()
    {
        GameManager.Instance.SetGetWorldState = this;

        if (PauseAndResumeManager.Instance)
        {
            PauseAndResumeManager.Instance.AddResumeAction(OnResume);
            PauseAndResumeManager.Instance.AddPauseAction(OnPause);
        }

        _level = ScenesManager.Instance.GetLevelByCurrentScene();

        if (!_baseController)
            _baseController = GetComponentInParent<BaseController>();
        if (_baseController)
            _baseController.StopMovement();

        if (!_playerController)
        {
            if (GameManager.Instance.SetGetPlayer)
                _playerController = GameManager.Instance.SetGetPlayer;
            else
                _playerController = FindObjectOfType<PlayerController>();
        }

        if (_playerInitialPos == Vector3.zero)
            _playerInitialPos = _playerController.transform.position;

        _playerController.GetRigidbody.bodyType = RigidbodyType2D.Static;

        OnUpdate = StartCount;
    }

    private void Update()
    {
        OnUpdate?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() && !_onPause)
            OnUpdate = WinCount;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() && !_onPause)
        {
            OnUpdate -= WinCount;
            _timeToWin = 3;
            if (DunkLevelCanvas.Instance)
                DunkLevelCanvas.Instance.OnExitWinBase();
        }
    }

    void WinCount()
    {
        if (_onPause)
            return;

        if (_timeToWin > 0)
        {
            _timeToWin -= Time.deltaTime;
            if (DunkLevelCanvas.Instance)
                DunkLevelCanvas.Instance.OnCountTime(MathF.Max(_timeToWin + 1, 0f));
        }
        else
        {
            if (_baseController)
                _baseController.StopMovement();

            LevelManager.Instance.OnWin();
            OnUpdate -= WinCount;
        }
    }

    public void StartCount()
    {
        if (_onPause)
            return;

        if (_timeToStart < 3)
        {
            _timeToStart += Time.deltaTime;

            if (DunkLevelCanvas.Instance)
                DunkLevelCanvas.Instance.OnCountTime(MathF.Min(_timeToStart + 1, 3));
        }
        else
        {
            _playerController.GetRigidbody.bodyType = RigidbodyType2D.Dynamic;
            if (_baseController)
                _baseController.PlayMovement();
            if (DunkLevelCanvas.Instance)
                DunkLevelCanvas.Instance.OnExitWinBase();

            _timeToStart = 0;
            OnUpdate = null;
        }
    }

    public void SetOnUpdate(Action action)
    {
        OnUpdate = action;
    }

    public void OnResume()
    {
        _onPause = false;
    }

    public void OnPause()
    {
        _onPause = true;
    }

    public int GetLevel => _level;
    public bool GetOnInitialPause => _timeToStart > 0 && _timeToStart < 3;
    public Vector3 GetInitalPos => _playerInitialPos;
    public BaseController GetBaseController => _baseController;
}
