using System;
using System.Collections;
using UnityEngine;
using static GameManager;

public class PowerUpManager : MonoBehaviour
{
    public Action<PowerUpType> OnPowerUpActivated = delegate { };
    public Action<PowerUpType> OnPowerUpDeactivated = delegate { };
    public Action OnUpdate = delegate { };
    public static PowerUpManager Instance { get; private set; }

    private bool _powerUpTapsCounterEnabled = false;
    private bool _powerUpImmunityEnabled = false;

    [SerializeField] private float _timeStopTime = 3f;
    [SerializeField] private float _stopTouchCounterTime = 3f;
    [SerializeField] private float _immunityTime = 3f;

    private float _timeStopTimer = 0f;
    private float _stopTouchCounterTimer = 0f;
    private float _immunityTimer = 0f;
    private float _reviveTimer = 0f;
    private bool _isShowingReviveUI;
    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnWinLevel += ForceStopAllPowerUp;
            LevelManager.Instance.OnPreLoseLevel += ForceStopAllPowerUp;
        }
    }

    private void OnDisable()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnWinLevel -= ForceStopAllPowerUp;
            LevelManager.Instance.OnPreLoseLevel -= ForceStopAllPowerUp;
        }
    }

    private void Update()
    {
        OnUpdate?.Invoke();
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.L))
        {
            //SelectPowerUp(PowerUpType.TimeStopPowerUp);
            AddPowerUp(PowerUpType.TimeStopPowerUp, 1);
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            //SelectPowerUp(PowerUpType.StopTouchCounterPowerUp);
            AddPowerUp(PowerUpType.StopTouchCounterPowerUp, 1);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            //SelectPowerUp(PowerUpType.ImmunityPowerUp);
            AddPowerUp(PowerUpType.ImmunityPowerUp, 1);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            AddPowerUp(PowerUpType.RevivePowerUp, 1);

        }
#endif
    }
    public void SelectPowerUp(PowerUpType powerUp)
    {
        switch (powerUp)
        {
            case PowerUpType.TimeStopPowerUp:
                GameManager.Instance.SetGetWorldState.StopCountTimerMode();
                _timeStopTimer = _timeStopTime;
                OnUpdate += TimeStopCounter;
                break;
            case PowerUpType.StopTouchCounterPowerUp:
                _powerUpTapsCounterEnabled = true;
                _stopTouchCounterTimer = _stopTouchCounterTime;
                OnUpdate += StopTouchCounter;
                break;
            case PowerUpType.ImmunityPowerUp:
                LevelCanvas.Instance.SetImmunityButton(false);
                _immunityTimer = _immunityTime;
                _powerUpImmunityEnabled = true;
                OnUpdate += ImmunityCounter;
                break;
            case PowerUpType.RevivePowerUp:
                AcceptRevivalPowerUp();
                break;
        }

        OnPowerUpActivated?.Invoke(powerUp);
    }

    private void TimeStopCounter()
    {
        _timeStopTimer -= Time.deltaTime;

        if (_timeStopTimer <= 0f)
        {
            OnUpdate -= TimeStopCounter;
            StopPowerUp(PowerUpType.TimeStopPowerUp);
        }
    }

    private void StopTouchCounter()
    {
        _stopTouchCounterTimer -= Time.deltaTime;

        if (_stopTouchCounterTimer <= 0f)
        {
            OnUpdate -= StopTouchCounter;
            StopPowerUp(PowerUpType.StopTouchCounterPowerUp);
        }
    }

    private void ImmunityCounter()
    {
        _immunityTimer -= Time.deltaTime;

        if (_immunityTimer <= 0f)
        {
            OnUpdate -= ImmunityCounter;
            StopPowerUp(PowerUpType.ImmunityPowerUp);
        }
    }
    public void AcceptRevivalPowerUp()
    {
        OnUpdate -= RejectRevivalCounter;
        _reviveTimer = 0f;

        switch (GameManager.Instance.GetCurrentGameMode)
        {
            case GameModes.Time:
                if ((GameManager.Instance.SetGetWorldState.GetRemainingTime <= 3))
                {
                    GameManager.Instance.SetGetWorldState.AddCountToTimer(3);
                    AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.TimeAlertSound, true);
                }
                break;
            case GameModes.OneTouch:
                if (GameManager.Instance.SetGetTapController.SetGetTapCount <= 3)
                {
                    GameManager.Instance.SetGetTapController.AddTouchesFromBubbles(3);
                }
                break;
        }

        SelectPowerUp(PowerUpType.ImmunityPowerUp);
        GameManager.Instance.SetGetPlayer.PlayerPhysicsRevival();
        _isShowingReviveUI = false;
        Debug.Log("Reviving");
    }
    public void RejectRevivalPowerUp(float time)
    {
        _reviveTimer = time;
        OnUpdate += RejectRevivalCounter;
        _isShowingReviveUI = false;
    }

    private void RejectRevivalCounter()
    {
        if (_reviveTimer <= 0f) return;
        _reviveTimer -= Time.deltaTime;
        LevelCanvas.Instance.UpdateTextPowerUpPopUpTimeCounter(_reviveTimer);

        if (_reviveTimer <= 0)
        {
            OnUpdate -= RejectRevivalCounter;

            LevelManager.Instance.OnRejectRevival?.Invoke();
            Debug.Log("Rejected");
            LevelManager.Instance.OnLose();
            GameManager.Instance.SetGetPlayer.PlayerPhysicsRejectRevival();
        }
    }

    public void StopPowerUp(PowerUpType powerUp)
    {
        switch (powerUp)
        {
            case PowerUpType.TimeStopPowerUp:
                GameManager.Instance.SetGetWorldState.ResumeCountTimerMode();
                break;
            case PowerUpType.StopTouchCounterPowerUp:
                _powerUpTapsCounterEnabled = false;
                break;
            case PowerUpType.ImmunityPowerUp:
                LevelCanvas.Instance.SetImmunityButton(!_isShowingReviveUI);
                _powerUpImmunityEnabled = false;
                if ((GameManager.Instance.GetCurrentGameMode == GameModes.Time &&
                    GameManager.Instance.SetGetWorldState.GetRemainingTime <= 0) ||
                    (GameManager.Instance.GetCurrentGameMode == GameModes.OneTouch &&
                    GameManager.Instance.SetGetTapController.SetGetTapCount <= 0))
                    GameManager.Instance.SetGetPlayer.Death();
                break;
            case PowerUpType.RevivePowerUp:

                break;
        }

        OnPowerUpDeactivated?.Invoke(powerUp);
    }

    private void ForceStopAllPowerUp()
    {
        OnUpdate = delegate { };

        _powerUpTapsCounterEnabled = false;
        _powerUpImmunityEnabled = false;

        _timeStopTimer = 0f;
        _stopTouchCounterTimer = 0f;
        _immunityTimer = 0f;
        _reviveTimer = 0f;
    }

    public void AddPowerUp(PowerUpType powerUp, int amount)
    {
        SaveAndLoadManager.SetIntValue(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.PowerUpPrefix +
            powerUp.ToString()) + amount, SaveAndLoadManager.PowerUpPrefix + powerUp.ToString(), true, true);

        LevelCanvas.Instance.UpdatePowerUpTexts(powerUp);
    }

    public bool HasPowerUp(PowerUpType powerUp)
    {
        return SaveAndLoadManager.GetIntValue(SaveAndLoadManager.PowerUpPrefix + powerUp.ToString()) > 0;
    }

    public void RestPowerUpFromText(PowerUpType powerUp)
    {
        SaveAndLoadManager.SetIntValue(Mathf.Max(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.PowerUpPrefix +
             powerUp.ToString()) - 1, 0), SaveAndLoadManager.PowerUpPrefix + powerUp.ToString(), true, true);
        LevelCanvas.Instance.UpdatePowerUpTexts(powerUp);
    }

    #region UTILITY METHODS
    public bool PowerUpTapsEnabled => _powerUpTapsCounterEnabled;

    public bool PowerUpImmunityEnabled => _powerUpImmunityEnabled;

    public float GetStopPowerUpTimeActive => _timeStopTime;

    public float GetStopTouchCounterPowerUpTimeActive => _stopTouchCounterTime;

    public float GetImmunityTimeActive => _immunityTime;

    public bool GetSetIsShowingReviveUI
    {
        set => _isShowingReviveUI = value;
        get => _isShowingReviveUI;
    }
    #endregion

    public enum PowerUpType
    {
        TimeStopPowerUp,
        StopTouchCounterPowerUp,
        ImmunityPowerUp,
        RevivePowerUp
    }
}
