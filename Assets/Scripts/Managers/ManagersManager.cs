using System.Collections;
using UnityEngine;

public abstract class ManagersManager : MonoBehaviour
{
    [SerializeField] private int _priorityIndex = -1;
    protected bool _isInitialized = false;

    protected virtual void Start()
    {
        if (LoadingGameManager.Instance)
            LoadingGameManager.Instance.AddManager(this, _priorityIndex);
        else
            StartCoroutine(InizializeManagers());
    }

    public abstract IEnumerator InizializeManagers();

    public bool IsInitialized
    {
        set => _isInitialized = value;
        get => _isInitialized;
    }
}
