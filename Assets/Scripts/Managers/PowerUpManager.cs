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

    private int _timeStopPowerUpAmount = 0;
    private int _stopTouchCounterPowerUpAmount = 0;
    private int _immunityPowerUpAmount = 0;
    private void Awake()
    {
        Instance = this;
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            SelectPowerUp(PowerUpType.TimeStopPowerUp, 3f);

        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            SelectPowerUp(PowerUpType.StopTouchCounterPowerUp, 3f);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            SelectPowerUp(PowerUpType.ImmunityPowerUp, 3f);
        }
    }
#endif
    public void SelectPowerUp(PowerUpType powerUp, float timeActive)
    {
        switch (powerUp)
        {
            case PowerUpType.TimeStopPowerUp:
                GameManager.Instance.SetGetWorldState.StopCountTimerMode();
                StartCoroutine(StopPowerUp(powerUp, timeActive));
                break;
            case PowerUpType.StopTouchCounterPowerUp:
                _powerUpTapsCounterEnabled = true;
                StartCoroutine(StopPowerUp(powerUp, timeActive));
                break;
            case PowerUpType.ImmunityPowerUp:
                _powerUpImmunityEnabled = true;
                StartCoroutine(StopPowerUp(powerUp, timeActive));
                break;
            case PowerUpType.RevivePowerUp:

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
                GameManager.Instance.SetGetWorldState.ResumeCountTimerMode();
                break;
            case PowerUpType.StopTouchCounterPowerUp:
                _powerUpTapsCounterEnabled = false;
                break;
            case PowerUpType.ImmunityPowerUp:
                _powerUpImmunityEnabled = false;
                break;
            case PowerUpType.RevivePowerUp:

                break;
        }

        OnPowerUpDeactivated?.Invoke(powerUp);
    }

    public void AddPowerUp(PowerUpType powerUp, int amount)
    {
        SaveAndLoadManager.SetIntValue(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.PowerUpPrefix +
            powerUp.ToString()) + amount, SaveAndLoadManager.PowerUpPrefix + powerUp.ToString());
    }

    public bool HasPowerUp(PowerUpType powerUp)
    {
        return SaveAndLoadManager.GetIntValue(SaveAndLoadManager.PowerUpPrefix + powerUp.ToString()) > 0;
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
