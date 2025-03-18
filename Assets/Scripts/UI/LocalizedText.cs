using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    public string key;

    private TextMeshProUGUI _text;

    private void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        LanguageManager.Instance.OnUpdateLanguage += UpdateText;
    }

    public void UpdateText()
    {
        _text.text = LanguageManager.Instance.GetLocalizedText(key);
    }
}
