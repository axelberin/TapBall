using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingGameManager : CanvasElementLocator
{
    public static LoadingGameManager Instance;

    private bool _canShowTexts = false;
    private List<ManagersManager> _managers = new();

    private Animator _fadeAnimator;
    private Image _loadingBarImage;
    private TextMeshProUGUI _loadingText;

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
        _loadingBarImage = FindAndValidateComponent<Image>(transform, "LoadingBarImage");
        _loadingText = FindAndValidateComponent<TextMeshProUGUI>(transform, "LoadingText");
        _loadingBarImage.fillAmount = 0;

        StartCoroutine(InitializeManagers());
    }

    private IEnumerator InitializeManagers()
    {
        yield return new WaitForSeconds(1f);
        foreach (var manager in _managers)
        {
            StartCoroutine(SmoothFill((_managers.IndexOf(manager) + 1) / (float)_managers.Count));

            if (!manager.IsInitialized)
            {
                yield return manager.InizializeManagers();
                if (manager.IsInitialized && manager is LanguageManager)
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
            ScenesManager.Instance.LoadSceneAsync("Menu", _fadeAnimator);
        }
    }

    private void ShowRandomLoadingText()
    {
        if (!_canShowTexts)
            return;

        int randomIndex = UnityEngine.Random.Range(1, 5);
        var (text, font) = LanguageManager.Instance.GetlocalizatedTextAndFont("loadingText" + randomIndex);
        _loadingText.font = font;
        UIManager.Instance.SetText(_loadingText, text);
    }

    public void AddManager(ManagersManager manager)
    {
        if (manager == null)
            return;

        if (!_managers.Contains(manager))
            _managers.Add(manager);
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
}
