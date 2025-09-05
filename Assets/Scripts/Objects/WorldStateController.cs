using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class WorldStateController : MonoBehaviour, IPauseble
{
    private Action OnUpdate = delegate { };

    [SerializeField] private int _limitTouches = 1;
    [SerializeField] private float _limitTime = 30f;
    private float _timerCounter;
    private int _level;
    private float _timeToWin = 3;
    private float _timeToStart = 0;
    private bool _onPause = false;
    private Vector3 _playerInitialPos;
    private bool _playOnce;

    private PlayerController _playerController;
    private List<MovableObjects> _movableObjectsInLevel = new List<MovableObjects>();

    private void Awake()
    {
        _timerCounter = _limitTime;
    }

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
                _playerController = FindAnyObjectByType<PlayerController>();
        }

        if (_playerInitialPos == Vector3.zero)
            _playerInitialPos = _playerController.transform.position;

        _playerController.GetRigidbody.bodyType = RigidbodyType2D.Static;

        _movableObjectsInLevel = FindObjectsByType<MovableObjects>(FindObjectsSortMode.None).ToList();
        _movableObjectsInLevel.ForEach(obj => obj.StopMovement());

        SetOnUpdate(StartCount);
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance)
            LevelManager.Instance.OnLoseLevel -= OnLose;
    }

    private void OnLose()
    {
        SetOnUpdate(StartCount);
        OnUpdate -= ControlTimerMode;
        _timerCounter = _limitTime;
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
            if (_timeToWin >= 3f)
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.CountDownSound);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() && !_onPause)
        {
            OnUpdate -= WinCount;
            if (DunkLevelCanvas.Instance)
                DunkLevelCanvas.Instance.OnExitWinBase();

            if (_timeToWin > 0f)
                AudioManager.Instance.StopSound();
            _timeToWin = 3;
        }
    }

    public void ControlTimerMode()
    {
        if (_timerCounter > 0)
        {
            _timerCounter -= Time.deltaTime;
            DunkLevelCanvas.Instance.ShowTimerText(_timerCounter + 1);
        }
        else if (_timerCounter <= 0)
        {
            GameManager.Instance.SetGetPlayer.Death();
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
            _timeToWin = 0;
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
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.CountDownSound);
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

            if (GameManager.Instance.GetCurrentGameMode == GameManager.GameModes.Time)
            {
                OnUpdate += ControlTimerMode;
            }
        }
    }

    public void SetOnUpdate(Action action = null)
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
    public float GetLimitTime => _limitTime;


    public bool GetOnInitialPause => _timeToStart > 0 && _timeToStart < 3;
    public Vector3 GetInitalPos => _playerInitialPos;
}
