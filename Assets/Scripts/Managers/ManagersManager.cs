using System.Collections;
using UnityEngine;

public abstract class ManagersManager : MonoBehaviour
{
    protected bool _isInitialized = false;
    public abstract IEnumerator InizializeManagers();

    public bool IsInitialized
    {
        set => _isInitialized = value;
        get => _isInitialized;
    }
}
