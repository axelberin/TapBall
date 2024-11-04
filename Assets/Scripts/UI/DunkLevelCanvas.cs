using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DunkLevelCanvas : ACanvas
{
    public static DunkLevelCanvas Instance;

    private TextMeshProUGUI _tapCountText;
    private TextMeshProUGUI _winTime;
    private TextMeshProUGUI _winText;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        _tapCountText = FindAndValidateTextComponent(transform, "PointsText");
        _winText = FindAndValidateTextComponent(transform, "WinTime");
        _winText = FindAndValidateTextComponent(transform, "WinUI");
    }

    public void OnWin()
    {
        UIManager.Instance.ActivateUI(_winTime.gameObject, false);
        UIManager.Instance.ActivateUI(_winText.gameObject, transform);
    }

    public void OnLose()
    {
        UIManager.Instance.SetText(_tapCountText, 0);
    }

    public void OnTap(int tapCount)
    {
        UIManager.Instance.SetText(_tapCountText, tapCount);
    }

    public void OnExitWinBase()
    {
        UIManager.Instance.ActivateUI(_winTime.gameObject, false);
    }

    public void OnCountTime(float time)
    {
        UIManager.Instance.SetText(_winTime, (int)(time + 1));
    }
}
