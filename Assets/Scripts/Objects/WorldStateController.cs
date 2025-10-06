using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class WorldStateController : MonoBehaviour, IPauseble
{
    private Action OnUpdate = delegate { };

    [SerializeField] private int _limitTouches = 1;
    [SerializeField] private float _limitTime = 30f;
    [SerializeField] private int _limitTapsOneTouch = 30;
    private float _timerCounter;
    private int _level;
    private float _timeToWin = 3;
    private float _timeToStart = 0;
    private bool _onPause = false;
    private Vector3 _playerInitialPos;
    private bool _playOnce;

    private PlayerController _playerController;
    private List<MovableObjects> _movableObjectsInLevel = new();

    private void Awake()
    {
        GameManager.Instance.SetGetWorldState = this;
        _timerCounter = _limitTime;
    }

    private void Start()
    {
        if (PauseAndResumeManager.Instance)
        {
            PauseAndResumeManager.Instance.AddResumeAction(OnResume);
            PauseAndResumeManager.Instance.AddPauseAction(OnPause);
        }

        if (LevelManager.Instance)
        {
            LevelManager.Instance.OnLoseLevel += OnLose;
            LevelManager.Instance.OnPreLoseLevel += OnTimerPreLose;
        }

        _level = ScenesManager.Instance.GetLevelByCurrentScene();

        if (!_playerController)
        {
            if (GameManager.Instance.SetGetPlayer)
                _playerController = GameManager.Instance.SetGetPlayer;
            else
                _playerController = FindAnyObjectByType<PlayerController>();
        }

        if (_playerInitialPos == Vector3.zero)
            _playerInitialPos = _playerController.transform.position;

        _playerController.GetRigidbody.bodyType = RigidbodyType2D.Static;

        _movableObjectsInLevel = FindObjectsByType<MovableObjects>(FindObjectsSortMode.None).ToList();
        _movableObjectsInLevel.ForEach(obj => obj.StopMovement());

        OnUpdate += StartCount;
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance)
        {
            LevelManager.Instance.OnLoseLevel -= OnLose;
            LevelManager.Instance.OnPreLoseLevel -= OnTimerPreLose;
        }
        OnUpdate = null;
    }

    public void ResetTimer()
    {
        _timerCounter = _limitTime;
    }

    private void OnLose()
    {
        _playOnce = false;
        OnUpdate = null;
        OnUpdate = StartCount;
    }

    private void OnTimerPreLose()
    {
        _timerCounter = 0;
        OnUpdate -= ControlTimerMode;
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
            OnUpdate += WinCount;
            if (_timeToWin >= 3f)
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.CountDownSound);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() && !_onPause)
        {
            OnUpdate -= WinCount;
            if (LevelCanvas.Instance)
                LevelCanvas.Instance.OnExitWinBase();

            if (_timeToWin > 0f)
                AudioManager.Instance.StopSound(true, false);
            _timeToWin = 3;
        }
    }

    private void ControlTimerMode()
    {
        if (_onPause)
            return;

        if (_timerCounter > 0)
        {
            _timerCounter -= Time.deltaTime;
            if (_timerCounter <= 3 && !_playOnce)
            {
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.TimeAlertSound, true);
                _playOnce = true;
            }
            LevelCanvas.Instance.ShowTimerText(_timerCounter);
        }
        else
        {
            _timerCounter = 0;
            OnUpdate -= ControlTimerMode;
            _playOnce = false;
            ResetTimer();
            GameManager.Instance.SetGetPlayer.Death();
        }
    }

    private void WinCount()
    {
        if (_onPause)
            return;

        if (_timeToWin > 0)
        {
            _timeToWin -= Time.deltaTime;
            if (LevelCanvas.Instance)
                LevelCanvas.Instance.OnCountTime(MathF.Max(_timeToWin + 1, 0f));
        }
        else
        {
            _movableObjectsInLevel.ForEach(obj => obj.StopMovement());
            _timeToWin = 0;
            LevelManager.Instance.OnWin();
            OnUpdate = null;
        }
    }

    private void StartCount()
    {
        if (_onPause)
            return;

        if (_timeToStart < 3)
        {
            _timeToStart += Time.deltaTime;

            if (LevelCanvas.Instance)
                LevelCanvas.Instance.OnCountTime(MathF.Min(_timeToStart + 1, 3));

            if (!_playOnce)
            {
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.CountDownSound);
                _playOnce = true;
            }
        }
        else
        {
            _playerController.GetRigidbody.bodyType = RigidbodyType2D.Dynamic;
            _movableObjectsInLevel.ForEach(obj => obj.PlayMovement());

            if (LevelCanvas.Instance)
                LevelCanvas.Instance.OnExitWinBase();

            _timeToStart = 0;
            OnUpdate -= StartCount;
            _playOnce = false;

            if (GameManager.Instance.GetCurrentGameMode == GameManager.GameModes.Time)
                OnUpdate += ControlTimerMode;
        }
    }

    public void OnResume()
    {
        _onPause = false;
    }

    public void OnPause()
    {
        _onPause = true;
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

    public float GetElapsedTime => _limitTime - _timerCounter;
    public int GetLimitTouches => _limitTouches;
    public float GetLimitTime => _limitTime;
    public int GetLimitTapsOneTouch => _limitTapsOneTouch;
    public float GetRemainingTime => MathF.Round(_timerCounter * 100f) / 100f;
    public bool GetOnInitialPause => _timeToStart > 0 && _timeToStart < 3;
    public Vector3 GetInitalPos => _playerInitialPos;
}
