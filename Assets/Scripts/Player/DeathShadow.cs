using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DeathShadow : MonoBehaviour, ISkinLoader
{
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        transform.localScale = GameManager.Instance.SetGetPlayer.transform.localScale;

        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer != null)
            Addressables.LoadAssetAsync<GameObject>("Death" + SaveAndLoadManager.GetStringValue(
                SaveAndLoadManager.CurrentBallSkinName)).Completed += OnPrefabLoaded;
        var key = "Death" + SaveAndLoadManager.GetStringValue(SaveAndLoadManager.CurrentBallSkinName) + "Particles";
        Addressables.InstantiateAsync(key, transform.position, transform.rotation);
    }

    public void OnPrefabLoaded(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var handleRenderer = handle.Result.GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = handleRenderer.sprite;
            _spriteRenderer.color = handleRenderer.color;
        }
        else
            Debug.LogError("Failed to load prefab.");
    }
}
