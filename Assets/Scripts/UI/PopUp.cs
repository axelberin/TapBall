using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUp : CanvasElementLocator
{
    private Animator _animator;
    private TextMeshProUGUI _tittleText;
    private TextMeshProUGUI _descriptionText;
    private Button _okButton;
    private Button _cancelButton;
    private GameObject _elements;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Initialize(string tittleText, string description = null, Action okAction = null, Action cancelAction = null)
    {
        _tittleText = FindAndValidateComponent<TextMeshProUGUI>(transform, "TittleText");
        SetTittle(tittleText);

        _descriptionText = FindAndValidateComponent<TextMeshProUGUI>(transform, "DescriptionText");
        SetDescription(description);

        _okButton = FindAndValidateComponent<Button>(transform, "SquareOkBTN");
        _okButton.onClick.AddListener(() =>
        {
            okAction?.Invoke();
            Hide();
        });

        _cancelButton = FindAndValidateComponent<Button>(transform, "SquareCancelBTN");
        _cancelButton.onClick.AddListener(() =>
        {
            cancelAction?.Invoke();
            Hide();
        });

        _elements = FindAndValidateGameObjectComponent(transform, "PopUpElements");
        _elements.SetActive(false);
    }

    public void SetElements(string tittleText = null, string description = null, Action okAction = null, Action cancelAction = null)
    {
        if (tittleText != null)
            SetTittle(tittleText);

        SetDescription(description);

        if (okAction != null)
        {
            _okButton.onClick.RemoveAllListeners();
            _okButton.onClick.AddListener(() =>
            {
                okAction?.Invoke();
                Hide();
            });
        }

        if (cancelAction != null)
        {
            _cancelButton.onClick.RemoveAllListeners();
            _cancelButton.onClick.AddListener(() =>
            {
                cancelAction?.Invoke();
                Hide();
            });
        }

        Show();
    }

    public void InitializeWithIcon(string tittleText, GameObject icon, string description = null, Action okAction = null, Action cancelAction = null)
    {
        Initialize(tittleText, description, okAction, cancelAction);

        //var image = FindAndValidateComponent<Image>(transform, "Image");
        //image.sprite = icon.GetComponent<Image>().sprite;

        //if (icon.TryGetComponent(out Animator animator))
        //{
        //    if (image.GetComponent<Animator>() == null)
        //        image.gameObject.AddComponent<Animator>().runtimeAnimatorController = animator.runtimeAnimatorController;
        //    else
        //        image.GetComponent<Animator>().runtimeAnimatorController = animator.runtimeAnimatorController;

        //    animator.SetTrigger("Idle");
        //}
    }

    private void SetTittle(string tittleText)
    {
        UIManager.Instance.SetText(_tittleText, LanguageManager.Instance.GetLocalizedText(tittleText));
    }

    private void SetDescription(string descriptionText)
    {
        if (descriptionText != null)
        {
            _descriptionText.gameObject.SetActive(true);
            UIManager.Instance.SetText(_descriptionText, LanguageManager.Instance.GetLocalizedText(descriptionText));
        }
        else
            _descriptionText.gameObject.SetActive(false);
    }

    public void Show()
    {
        _animator.SetTrigger("Show");
    }

    public void Hide()
    {
        if (_okButton != null)
            _okButton.onClick.RemoveAllListeners();

        if (_cancelButton != null)
            _cancelButton.onClick.RemoveAllListeners();

        _animator.SetTrigger("Hide");
    }
}
