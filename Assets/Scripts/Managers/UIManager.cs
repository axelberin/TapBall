using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UtilityAddressables;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private List<GameObject> _canvasesNmaes = new();
    private GameObject _comingSoonNotifyPrefab;
    private TextMeshProUGUI _comingSoonNotifyText;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        AddressablesUtility.LoadAsset<GameObject>("ComingSoonNotifyText",
            text => _comingSoonNotifyPrefab = text);
    }

    public void ActivateUI(GameObject gameObject, bool active)
    {
        gameObject.SetActive(active);
    }

    public void SetText(TextMeshProUGUI text, int count)
    {
        if (!text)
            return;
        if (!text.isActiveAndEnabled)
            ActivateUI(text.gameObject, true);

        text.SetText("{0}", count);
        SetFontByText(text, true);
    }

    public void SetText(TextMeshProUGUI text, float count)
    {
        if (!text)
            return;
        if (!text.isActiveAndEnabled)
            ActivateUI(text.gameObject, true);

        text.SetText("{0}", count);
        SetFontByText(text, true);
    }

    public void SetText(TextMeshProUGUI text, string content, bool hasNum = false)
    {
        if (!text)
            return;
        if (!text.isActiveAndEnabled)
            ActivateUI(text.gameObject, true);

        text.SetText(content);
        SetFontByText(text, hasNum);
    }

    private void SetFontByText(TextMeshProUGUI text, bool isNum)
    {
        if (LanguageManager.Instance)
        {
            var gameFont = LanguageManager.Instance.GetFontByLanguage(isNum);
            if (gameFont != text.font)
                text.font = gameFont;
        }
    }

    public void AddCanvas(GameObject canvas, bool active)
    {
        if (!_canvasesNmaes.Contains(canvas))
            _canvasesNmaes.Add(canvas);

        canvas.SetActive(active);
    }

    public void ClearCnavasesList()
    {
        _canvasesNmaes.Clear();
    }

    public void ChangeCanvas(string canvasFrom, string canvasTo)
    {
        foreach (var canvas in _canvasesNmaes)
        {
            if (canvas.name == canvasFrom)
                canvas.SetActive(false);
            if (canvas.name == canvasTo)
                canvas.SetActive(true);
        }
    }

    public IEnumerator ShowComingSoonNotify(Transform parent)
    {
        if (_comingSoonNotifyPrefab == null)
        {
            Debug.LogError("Missing coming soon text");
            yield break;
        }

        if (_comingSoonNotifyText == null)
        {
            var textGO = Instantiate(_comingSoonNotifyPrefab, parent);
            _comingSoonNotifyText = textGO.GetComponent<TextMeshProUGUI>();
        }

        _comingSoonNotifyText.gameObject.SetActive(true);
        _comingSoonNotifyText.font = LanguageManager.Instance.GetFontByLanguage();
        yield return new WaitForSeconds(1f);
        _comingSoonNotifyText.gameObject.SetActive(false);
    }
}
