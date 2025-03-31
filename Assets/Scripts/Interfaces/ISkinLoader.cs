using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public interface ISkinLoader
{
    public void OnPrefabLoaded(AsyncOperationHandle<GameObject> handle);
}
