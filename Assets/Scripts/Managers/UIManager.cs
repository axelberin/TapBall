using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UtilityAddressables;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private List<GameObject> _canvasesNmaes = new();
    private GameObject _comingSoonNotifyText;

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
            text => _comingSoonNotifyText = text);
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

        text.text = count.ToString();
    }

    public void SetText(TextMeshProUGUI text, float count)
    {
        if (!text)
            return;
        if (!text.isActiveAndEnabled)
            ActivateUI(text.gameObject, true);

        text.text = count.ToString();
    }

    public void SetText(TextMeshProUGUI text, string content)
    {
        if (!text)
            return;
        if (!text.isActiveAndEnabled)
            ActivateUI(text.gameObject, true);

        text.text = content;
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
        if (_comingSoonNotifyText == null)
        {
            Debug.LogError("Missing coming soon text");
            yield break;
        }

        var textGO = Instantiate(_comingSoonNotifyText, parent);
        var text = textGO.GetComponent<TextMeshProUGUI>();
        text.font = LanguageManager.Instance.GetFontByLanguage();
        yield return new WaitForSeconds(2f);
        textGO.SetActive(false);
    }
}
