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
            _movableObjectsInLevel.ForEach(obj => obj.StopMovement());
            LevelManager.Instance.OnWin();
            OnUpdate = null;
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

    public void StopCountTimerMode()
    {
        OnUpdate -= ControlTimerMode;
        if (LevelCanvas.Instance)
            LevelCanvas.Instance.ShowTimerText(GetRemainingTime);

        _playOnce = false;
    }

    public void StartGame()
    {
        if (_onPause)
            return;

        _playerController.GetRigidbody.bodyType = RigidbodyType2D.Dynamic;
        _movableObjectsInLevel.ForEach(obj => obj.PlayMovement());

        if (GameManager.Instance.GetCurrentGameMode == GameManager.GameModes.Time)
            OnUpdate += ControlTimerMode;
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
    public Vector3 GetInitalPos => _playerInitialPos;
}
