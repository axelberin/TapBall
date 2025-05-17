using UnityEngine;

public class CanvasElementLocator : MonoBehaviour
{
    #region FindAndValidate

    protected T FindAndValidateComponent<T>(Transform parent, string childName, bool alert = false)
    {
        var childTransform = parent.FindDeepChild(childName);
        if (childTransform == null)
        {
            Debug.LogError("No hemos encontrado " + childName);
            return default;
        }

        var textComponent = childTransform.GetComponent<T>();
        if (textComponent == null) 
            Debug.LogError("No se encontró un componente TMP_Text en " + childName);

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

    #endregion
}
