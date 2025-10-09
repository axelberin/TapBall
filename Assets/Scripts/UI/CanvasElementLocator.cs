using UnityEngine;

public class CanvasElementLocator : MonoBehaviour
{
    #region FindAndValidate

    protected T FindAndValidateComponent<T>(Transform parent, string childName, bool alert = true)
    {
        var childTransform = parent.FindDeepChild(childName);
        if (childTransform == null && alert)
        {
            Debug.LogError("No hemos encontrado " + childName);
            return default;
        }
        else if (childTransform == null && !alert)
            return default;

        var component = childTransform.GetComponent<T>();
        if (component.Equals(default))
            Debug.LogError($"No se encontró un componente {typeof(T)} en {childName}");

        return component;
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
