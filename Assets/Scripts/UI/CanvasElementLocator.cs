using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasElementLocator : MonoBehaviour
{
    #region FindAndValidate

    //TODO: Probar
    //protected T FindAndValidateTextComponent<T>(Transform parent, string childName, bool alert = false)
    //{
    //    var childTransform = parent.FindDeepChild(childName);
    //    if (childTransform == null)
    //    {
    //        Debug.LogError("No hemos encontrado " + childName);
    //        return default;
    //    }

    //    var textComponent = childTransform.GetComponent<T>();
    //    if (textComponent == null) Debug.LogError("No se encontró un componente TMP_Text en " + childName);

    //    return textComponent;
    //}

    protected TextMeshProUGUI FindAndValidateTextComponent(Transform parent, string childName, bool alert = false)
    {
        var childTransform = parent.FindDeepChild(childName);
        if (childTransform == null)
        {
            Debug.LogError("No hemos encontrado " + childName);
            return null;
        }

        var textComponent = childTransform.GetComponent<TextMeshProUGUI>();
        if (textComponent == null) Debug.LogError("No se encontró un componente TMP_Text en " + childName);

        return textComponent;
    }

    protected GameObject FindAndValidateGameObjectComponent(Transform parent, string childName, bool alert = true)
    {
        var childTransform = parent.FindDeepChild(childName);
        if (childTransform == null && alert)
        {
            Debug.LogError("No hemos encontrado " + childName);
            return null;
        }

        return childTransform?.gameObject ?? null;
    }

    protected Image FindAndValidateImageComponent(Transform parent, string childName)
    {
        var childTransform = parent.FindDeepChild(childName);
        if (childTransform == null)
        {
            Debug.LogError("No hemos encontrado " + childName);
            return null;
        }

        var imageComponent = childTransform.GetComponent<Image>();
        if (imageComponent == null) Debug.LogError("No se encontró un componente Image en " + childName);

        return imageComponent;
    }

    protected RawImage FindAndValidateRawImageComponent(Transform parent, string childName)
    {
        var childTransform = parent.FindDeepChild(childName);
        if (childTransform == null)
        {
            Debug.LogError("No hemos encontrado " + childName);
            return null;
        }

        var imageComponent = childTransform.GetComponent<RawImage>();
        if (imageComponent == null) Debug.LogError("No se encontró un componente RawImage en " + childName);

        return imageComponent;
    }

    protected SpriteRenderer FindAndValidateSpriteRendererComponent(Transform parent, string childName)
    {
        var childTransform = parent.FindDeepChild(childName);
        if (childTransform == null)
        {
            Debug.LogError("No hemos encontrado " + childName);
            return null;
        }

        var spriteRendererComponent = childTransform.GetComponent<SpriteRenderer>();
        if (spriteRendererComponent == null)
            Debug.LogError("No se encontró un componente spriteRenderer en " + childName);

        return spriteRendererComponent;
    }

    protected TMP_InputField FindAndValidateInputFieldComponent(Transform parent, string childName)
    {
        var childTransform = parent.FindDeepChild(childName);
        if (childTransform == null)
        {
            Debug.LogError("No hemos encontrado " + childName);
            return null;
        }

        var inputFieldComponent = childTransform.GetComponent<TMP_InputField>();
        if (inputFieldComponent == null)
            Debug.LogError("No se encontró un componente TMP_InputField en " + childName);

        return inputFieldComponent;
    }

    protected Toggle FindAndValidateToggleComponent(Transform parent, string childName)
    {
        var childTransform = parent.FindDeepChild(childName);
        if (childTransform == null)
        {
            Debug.LogError("No hemos encontrado " + childName);
            return null;
        }

        var toggleComponent = childTransform.GetComponent<Toggle>();
        if (toggleComponent == null) Debug.LogError("No se encontró un componente Toggle en " + childName);

        return toggleComponent;
    }

    protected Button FindAndValidateButtonComponent(Transform parent, string childName, bool alert = true)
    {
        var childTransform = parent.FindDeepChild(childName);
        if (childTransform == null)
        {
            if (alert)
                Debug.LogError("No hemos encontrado " + childName);
            return null;
        }

        var buttonComponent = childTransform.GetComponent<Button>();
        if (buttonComponent == null) Debug.LogError("No se encontró un componente Button en " + childName);

        return buttonComponent;
    }

    protected Transform FindAndValidateTransformComponent(Transform parent, string childName)
    {
        var childTransform = parent.FindDeepChild(childName);
        if (childTransform == null)
        {
            Debug.LogError("No hemos encontrado " + childName);
            return null;
        }

        var transformComponent = childTransform.GetComponent<Transform>();
        if (transformComponent == null) Debug.LogError("No se encontró un componente Transform en " + childName);

        return transformComponent;
    }

    protected TMP_Dropdown FindAndValidateDropdownComponent(Transform parent, string childName)
    {
        var childTransform = parent.FindDeepChild(childName);
        if (childTransform == null)
        {
            Debug.LogError("No hemos encontrado " + childName);
            return null;
        }

        var dropdownComponent = childTransform.GetComponent<TMP_Dropdown>();
        if (dropdownComponent == null)
            Debug.LogError("No se encontró un componente TMP_Dropdown en " + childName);

        return dropdownComponent;
    }

    protected RectTransform FindAndValidateRectTransformComponent(Transform parent, string childName)
    {
        var childTransform = parent.FindDeepChild(childName);
        if (childTransform == null)
        {
            Debug.LogError("No hemos encontrado " + childName);
            return null;
        }

        var rectTransform = childTransform.GetComponent<RectTransform>();
        if (rectTransform == null) Debug.LogError("No se encontró un componente RectTransform en " + childName);

        return rectTransform;
    }

    protected Scrollbar FindAndValidateScrollbarComponent(Transform parent, string childName)
    {
        var childTransform = parent.FindDeepChild(childName);
        if (childTransform == null)
        {
            Debug.LogError("No hemos encontrado " + childName);
            return null;
        }

        var slider = childTransform.GetComponent<Scrollbar>();
        if (slider == null) Debug.LogError("No se encontró un componente Scrollbar en " + childName);

        return slider;
    }

    #endregion
}
