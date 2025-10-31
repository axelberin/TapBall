using System;
using System.Collections;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public Action<PowerUpType> OnPowerUpActivated = delegate { };
    public Action<PowerUpType> OnPowerUpDeactivated = delegate { };
    public static PowerUpManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public void SelectPowerUp(PowerUpType powerUp, float timeActive)
    {
        switch (powerUp)
        {
            case PowerUpType.TimeStopPowerUp:
                GameManager.Instance.SetGetWorldState.StopCountTimerMode();
                StartCoroutine(StopPowerUp(powerUp, timeActive));
                break;
            case PowerUpType.StopTouchCounterPowerUp:

                break;
            case PowerUpType.ImmunityPowerUp:

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

                break;
            case PowerUpType.ImmunityPowerUp:

                break;
            case PowerUpType.RevivePowerUp:

                break;
        }

        OnPowerUpDeactivated?.Invoke(powerUp);
    }

    public enum PowerUpType
    {
        TimeStopPowerUp,
        StopTouchCounterPowerUp,
        ImmunityPowerUp,
        RevivePowerUp
    }
}
