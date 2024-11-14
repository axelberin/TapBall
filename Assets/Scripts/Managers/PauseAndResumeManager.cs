using System;
using UnityEngine;

public class PauseAndResumeManager : MonoBehaviour
{
    public static PauseAndResumeManager Instance;

    private Action OnPauseGame = delegate { };
    private Action OnResumeGame = delegate { };

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void InvokeResume()
    {
        OnResumeGame?.Invoke();
    }

    public void InvokePause()
    {
        OnPauseGame?.Invoke();
    }

    public void AddResumeAction(Action action)
    {
        OnResumeGame += action;
    }

    public void RemoveResumeAction(Action action)
    {
        OnResumeGame -= action;
    }

    public void AddPauseAction(Action action)
    {
        OnPauseGame += action;
    }

    public void RemovePauseAction(Action action)
    {
        OnPauseGame -= action;
    }

    public void RestartResumeAction()
    {
        OnResumeGame = null;
    }

    public void RestartPauseAction()
    {
        OnPauseGame = null;
    }
}
