using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoadingGameManager : CanvasElementLocator
{
    public static LoadingGameManager Instance;

    private List<ManagersManager> _managers = new();

    private Animator _fadeAnimator;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        _fadeAnimator = FindAndValidateGameObjectComponent(transform, "FadeController").GetComponent<Animator>();

        StartCoroutine(InitializeManagers());
    }

    private IEnumerator InitializeManagers()
    {
        yield return new WaitForSeconds(1f);
        foreach (var manager in _managers)
        {
            if (!manager.IsInitialized)
                yield return manager.InizializeManagers();
        }

        yield return new WaitForSeconds(0.5f);

        if (_managers.Any(m => !m.IsInitialized))
            StartCoroutine(InitializeManagers());
        else
            ScenesManager.Instance.LoadSceneAsync("Menu", _fadeAnimator);
    }

    public void AddManager(ManagersManager manager)
    {
        if (manager == null)
            return;

        if (!_managers.Contains(manager))
            _managers.Add(manager);
    }
}
