using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUp : CanvasElementLocator
{
    private Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Initialize(string okBtnName, string tittleText, string description = null, string cancelBtnName = null,
        Action okAction = null, Action cancelAction = null)
    {
        var tittle = FindAndValidateComponent<TextMeshProUGUI>(transform, "TittleText");
        var (text, font) = LanguageManager.Instance.GetlocalizatedTextAndFont(tittleText);
        tittle.text = text;
        tittle.font = font;

        var descriptionText = FindAndValidateComponent<TextMeshProUGUI>(transform, "DescriptionText");
        if (description != null)
        {
            var text2 = LanguageManager.Instance.GetLocalizedText(description);
            descriptionText.text = text2;
            descriptionText.font = font;
        }
        else
            descriptionText.gameObject.SetActive(false);

        var okButton = FindAndValidateComponent<Button>(transform, okBtnName);
        okButton.onClick.AddListener(() =>
        {
            okAction?.Invoke();
            Hide();
        });

        var cancelButton = FindAndValidateComponent<Button>(transform, cancelBtnName);
        if (cancelBtnName != null)
            cancelButton.onClick.AddListener(() =>
            {
                cancelAction?.Invoke();
                Hide();
            });
        else
            cancelButton.gameObject.SetActive(false);

        var elements = FindAndValidateGameObjectComponent(transform, "PopUpElements");
        elements.gameObject.SetActive(false);
    }

    public void Show()
    {
        _animator.SetTrigger("Show");
    }

    public void Hide()
    {
        _animator.SetTrigger("Hide");
    }
}
