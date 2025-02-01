using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public interface ISkinLoader
{
    public void OnSpriteLoaded(AsyncOperationHandle<Sprite> handle);
}
