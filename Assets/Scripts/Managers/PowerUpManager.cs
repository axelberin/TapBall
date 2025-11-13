using System;
using System.Collections;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public Action<PowerUpType> OnPowerUpActivated = delegate { };
    public Action<PowerUpType> OnPowerUpDeactivated = delegate { };
    public static PowerUpManager Instance { get; private set; }

    private bool _powerUpTapsCounterEnabled = false;
    private bool _powerUpImmunityEnabled = false;

    [SerializeField] private float _timeStopTime = 3f;
    [SerializeField] private float _stopTouchCounterTime = 3f;
    [SerializeField] private float _immunityTime = 3f;

    private void Awake()
    {
        Instance = this;
    }
    //private void Start()
    //{
    //    LevelManager.Instance.OnWinLevel += ForceStopAllPowerUp;
    //    LevelManager.Instance.OnLoseLevel += ForceStopAllPowerUp;
    //}

    //private void OnDestroy()
    //{
    //    LevelManager.Instance.OnWinLevel -= ForceStopAllPowerUp;
    //    LevelManager.Instance.OnLoseLevel -= ForceStopAllPowerUp;
    //}

#if UNITY_EDITOR
    private void Update()
    {
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
    }
#endif
    public void SelectPowerUp(PowerUpType powerUp)
    {
        switch (powerUp)
        {
            case PowerUpType.TimeStopPowerUp:
                GameManager.Instance.SetGetWorldState.StopCountTimerMode();
                StartCoroutine(StopPowerUp(powerUp, _timeStopTime));
                break;
            case PowerUpType.StopTouchCounterPowerUp:
                _powerUpTapsCounterEnabled = true;
                StartCoroutine(StopPowerUp(powerUp, _stopTouchCounterTime));
                break;
            case PowerUpType.ImmunityPowerUp:
                _powerUpImmunityEnabled = true;
                StartCoroutine(StopPowerUp(powerUp, _immunityTime));
                break;
            case PowerUpType.RevivePowerUp:
                GameManager.Instance.SetGetPlayer.AcceptRevivalPowerUp();
                break;
        }

        OnPowerUpActivated?.Invoke(powerUp);
    }

    public IEnumerator StopPowerUp(PowerUpType powerUp, float timeToStop)
    {
        yield return new WaitForSecondsRealtime(timeToStop);
        switch (powerUp)
        {
            case PowerUpType.TimeStopPowerUp:
                if(GameManager.Instance.SetGetPlayer.HasDeath == false)
                GameManager.Instance.SetGetWorldState.ResumeCountTimerMode();
                break;
            case PowerUpType.StopTouchCounterPowerUp:
                _powerUpTapsCounterEnabled = false;
                break;
            case PowerUpType.ImmunityPowerUp:
                _powerUpImmunityEnabled = false;
                if ((GameManager.Instance.GetCurrentGameMode == GameManager.GameModes.Time &&
                    GameManager.Instance.SetGetWorldState.GetRemainingTime <= 0) ||
                    (GameManager.Instance.GetCurrentGameMode == GameManager.GameModes.OneTouch &&
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
        StartCoroutine(StopPowerUp(PowerUpType.TimeStopPowerUp, 0));
        StartCoroutine(StopPowerUp(PowerUpType.StopTouchCounterPowerUp, 0));
        //StartCoroutine(StopPowerUp(PowerUpType.ImmunityPowerUp, 0));
        //StartCoroutine(StopPowerUp(PowerUpType.RevivePowerUp, 0));
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
    #endregion
    public enum PowerUpType
    {
        TimeStopPowerUp,
        StopTouchCounterPowerUp,
        ImmunityPowerUp,
        RevivePowerUp
    }
}
