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
    private Image _iconImage;
    private Animator _iconAnimator;

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

        StrongHide();
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

    public void InitializeWithIcon(string tittleText, GameObject icon, string description = null, Action okAction = null,
        Action cancelAction = null, string triggerToPlay = null)
    {
        Initialize(tittleText, description, okAction, cancelAction);

        _iconImage = FindAndValidateComponent<Image>(transform, "SkinToWin");
        _iconAnimator = _iconImage.GetComponent<Animator>();

        ApplyIconFromObject(icon, triggerToPlay);
    }

    private void ApplyIconFromObject(GameObject sourceObject, string triggerToPlay = null)
    {
        if (_iconImage == null || sourceObject == null)
        {
            Debug.LogWarning("[PopUp] ApplyIconFromObject llamado sin _iconImage o sourceObject.");
            return;
        }

        // 1) Buscamos sprite genérico: SpriteRenderer o Image
        Sprite sprite = null;
        Color color = Color.white;

        var sourceSpriteRenderer = sourceObject.GetComponentInChildren<SpriteRenderer>();
        var sourceImage = sourceObject.GetComponentInChildren<Image>();

        if (sourceSpriteRenderer != null)
        {
            sprite = sourceSpriteRenderer.sprite;
            color = sourceSpriteRenderer.color;
        }
        else if (sourceImage != null)
        {
            sprite = sourceImage.sprite;
            color = sourceImage.color;
        }

        if (sprite == null)
        {
            _iconImage.gameObject.SetActive(false);
            return;
        }

        // 2) Seteamos sprite en la imagen del popup
        _iconImage.gameObject.SetActive(true);
        _iconImage.sprite = sprite;
        _iconImage.color = color;


        // 3) Animator (si el objeto tiene)
        var sourceAnimator = sourceObject.GetComponentInChildren<Animator>();

        if (sourceAnimator != null)
        {
            if (_iconAnimator == null)
                _iconAnimator = _iconImage.GetComponent<Animator>() ?? _iconImage.gameObject.AddComponent<Animator>();

            _iconAnimator.runtimeAnimatorController = sourceAnimator.runtimeAnimatorController;
            _iconAnimator.enabled = true;

            if (string.IsNullOrEmpty(triggerToPlay))
            {
                _iconAnimator.ResetTrigger(triggerToPlay);
                _iconAnimator.SetTrigger(triggerToPlay);
            }
        }
        else if (_iconAnimator != null)
        {
            _iconAnimator.enabled = false;
        }
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
        _animator.SetTrigger("Hide");
    }

    public void StrongHide()
    {
        if (_elements == null)
            _elements = FindAndValidateGameObjectComponent(transform, "PopUpElements");
        _elements.SetActive(false);
    }
}
