using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class WorldStateController : MonoBehaviour, IPauseble
{
    private Action OnUpdate = delegate { };

    [SerializeField] private int _limitTouches = 1;
    private int _level;
    private float _timeToWin = 3;
    private float _timeToStart = 0;
    private bool _onPause = false;
    private Vector3 _playerInitialPos;
    private bool _playOnce;

    private AudioSource _audioSource;
    private PlayerController _playerController;
    private List<MovableObjects> _movableObjectsInLevel = new List<MovableObjects>();

    private void Start()
    {
        GameManager.Instance.SetGetWorldState = this;

        if (PauseAndResumeManager.Instance)
        {
            PauseAndResumeManager.Instance.AddResumeAction(OnResume);
            PauseAndResumeManager.Instance.AddPauseAction(OnPause);
        }

        if (LevelManager.Instance)
            LevelManager.Instance.OnLoseLevel += OnLose;

        _level = ScenesManager.Instance.GetLevelByCurrentScene();

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

        _movableObjectsInLevel = FindObjectsByType<MovableObjects>(FindObjectsSortMode.None).ToList();
        _movableObjectsInLevel.ForEach(obj => obj.StopMovement());

        SetOnUpdate(StartCount);
        if (!_audioSource)
            _audioSource = GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance)
            LevelManager.Instance.OnLoseLevel -= OnLose;
    }

    private void OnLose()
    {
        SetOnUpdate(StartCount);
    }

    private void Update()
    {
        if (TutorialManager.Instance && TutorialManager.Instance.GetInTutorial)
            return;

        OnUpdate?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() && !_onPause)
        {
            SetOnUpdate(WinCount);
            _audioSource.Play();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() && !_onPause)
        {
            OnUpdate -= WinCount;
            _timeToWin = 3;
            if (DunkLevelCanvas.Instance)
                DunkLevelCanvas.Instance.OnExitWinBase();

            _audioSource.Stop();
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
            _movableObjectsInLevel.ForEach(obj => obj.StopMovement());

            LevelManager.Instance.OnWin();
            SetOnUpdate();
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

            if (!_playOnce)
            {
                _audioSource.Play();
                _playOnce = true;
            }
        }
        else
        {
            _playerController.GetRigidbody.bodyType = RigidbodyType2D.Dynamic;
            _movableObjectsInLevel.ForEach(obj => obj.PlayMovement());

            if (DunkLevelCanvas.Instance)
                DunkLevelCanvas.Instance.OnExitWinBase();

            _timeToStart = 0;
            SetOnUpdate();
            _playOnce = false;
        }
    }

    public void SetOnUpdate(Action action = null)
    {
        OnUpdate = action;
    }

    public void OnResume()
    {
        _onPause = false;
        _audioSource.UnPause();
    }

    public void OnPause()
    {
        _onPause = true;
        _audioSource.Pause();
    }

    public int GetLevel
    {
        get
        {
            if (_level == 0)
                _level = ScenesManager.Instance.GetLevelByCurrentScene();
            return _level;
        }
    }

    public int GetLimitTouches => _limitTouches;

    public bool GetOnInitialPause => _timeToStart > 0 && _timeToStart < 3;
    public Vector3 GetInitalPos => _playerInitialPos;
}
