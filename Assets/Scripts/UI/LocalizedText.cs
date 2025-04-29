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

    private void UpdateText()
    {
        if (!LanguageManager.Instance)
            return;

        var (text, font) = LanguageManager.Instance.GetlocalizatedTextAndFont(key);

        _text.font = font;
        _text.text = text;
    }
}
