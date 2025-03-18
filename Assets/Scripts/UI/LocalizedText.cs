using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    public string key;

    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        LanguageManager.Instance.OnUpdateLanguage += UpdateText;
    }

    private void OnEnable()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        if (!LanguageManager.Instance)
            return;

        _text.text = LanguageManager.Instance.GetLocalizedText(key);
    }
}
