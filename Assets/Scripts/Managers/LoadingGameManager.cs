using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LoadingGameManager : CanvasElementLocator
{
    public static LoadingGameManager Instance;

    private bool _canShowTexts = false;
    private List<ManagersManager> _managers = new();

    private Animator _fadeAnimator;
    private Image _loadingBarImage;
    private TextMeshProUGUI _loadingText;
    private PopUp _popUp;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);

        _fadeAnimator = FindAndValidateGameObjectComponent(transform, "FadeController").GetComponent<Animator>();
        _loadingBarImage = FindAndValidateComponent<Image>(transform, "LoadingBarImage");
        _loadingText = FindAndValidateComponent<TextMeshProUGUI>(transform, "LoadingText");
        _loadingBarImage.fillAmount = 0;
    }

    private void Start()
    {
        _popUp = FindAndValidateComponent<PopUp>(transform, "ErrorConectingPopUp");
        _popUp.Initialize("conectionfail", "cantconnect");

        SaveAndLoadManager.MigrateLegacyData();
        StartCoroutine(InitializeManagers());
    }

    private IEnumerator InitializeManagers()
    {
        yield return new WaitForSeconds(1f);
        foreach (var manager in _managers)
        {
            if (manager == null)
                continue;

            StartCoroutine(SmoothFill((_managers.IndexOf(manager) + 1) / (float)_managers.Count));

            if (!manager.IsInitialized)
            {
                yield return manager.InizializeManagers();
                if (manager.IsInitialized /*&& manager is SaveAndLoadOnCloudManager*/)
                    _canShowTexts = true;
            }

            ShowRandomLoadingText();
        }

        yield return new WaitForSeconds(0.5f);
        ShowRandomLoadingText();

        if (_managers.Any(m => !m.IsInitialized))
            StartCoroutine(InitializeManagers());
        else
        {
            StartCoroutine(SmoothFill(1f));
            yield return new WaitForSeconds(1f);
            ScenesManager.Instance.LoadSceneAsync("Menu", _fadeAnimator);
        }
    }

    private void ShowRandomLoadingText()
    {
        if (!_canShowTexts)
            return;

        int randomIndex = UnityEngine.Random.Range(1, 5);
        UIManager.Instance.SetText(_loadingText, LanguageManager.Instance.GetLocalizedText("loadingText" + randomIndex));
    }

    public void AddManager(ManagersManager manager, int index = -1)
    {
        if (manager == null)
            return;

        if (!_managers.Contains(manager))
        {
            if (index < 0)
                _managers.Add(manager); // Añadir al final
            else
            {
                // Expandir la lista si es necesario
                while (_managers.Count <= index)
                {
                    _managers.Add(null); // O un valor por defecto
                }
                _managers[index] = manager;
            }
        }
    }

    private IEnumerator SmoothFill(float target)
    {
        float start = _loadingBarImage.fillAmount;
        float t = 0f;
        float speed = 1f;

        while (t < 1.5f)
        {
            t += Time.deltaTime * (speed + t);
            _loadingBarImage.fillAmount = Mathf.Lerp(start, target, t);
            yield return null;
        }

        _loadingBarImage.fillAmount = target;
    }

    public void ShowCantSignInPopUp(string titleKey, string messageKey, Action okAction = null, Action cancelAction = null)
    {
        _popUp.SetElements(titleKey, messageKey, okAction, cancelAction);
    }
}
