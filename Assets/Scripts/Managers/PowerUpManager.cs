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

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            //SelectPowerUp(PowerUpType.TimeStopPowerUp);
            AddPowerUp(PowerUpType.TimeStopPowerUp, 1);
            LevelCanvas.Instance.UpdatePowerUpTexts(PowerUpType.TimeStopPowerUp);
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            //SelectPowerUp(PowerUpType.StopTouchCounterPowerUp);
            AddPowerUp(PowerUpType.StopTouchCounterPowerUp, 1);
            LevelCanvas.Instance.UpdatePowerUpTexts(PowerUpType.StopTouchCounterPowerUp);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            //SelectPowerUp(PowerUpType.ImmunityPowerUp);
            AddPowerUp(PowerUpType.ImmunityPowerUp, 1);
            LevelCanvas.Instance.UpdatePowerUpTexts(PowerUpType.ImmunityPowerUp);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            AddPowerUp(PowerUpType.RevivePowerUp, 1);
            LevelCanvas.Instance.UpdatePowerUpTexts(PowerUpType.RevivePowerUp);

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
            powerUp.ToString()) + amount, SaveAndLoadManager.PowerUpPrefix + powerUp.ToString(), true, true);
    }

    public bool HasPowerUp(PowerUpType powerUp)
    {
        return SaveAndLoadManager.GetIntValue(SaveAndLoadManager.PowerUpPrefix + powerUp.ToString()) > 0;
    }

    public void RestPowerUpFromText(PowerUpType powerUp)
    {
        SaveAndLoadManager.SetIntValue(Mathf.Max(SaveAndLoadManager.GetIntValue(SaveAndLoadManager.PowerUpPrefix +
             powerUp.ToString()) - 1, 0), SaveAndLoadManager.PowerUpPrefix + powerUp.ToString(), true, true);
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
