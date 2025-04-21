using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DeathShadow : MonoBehaviour, ISkinLoader
{
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private void Start()
    {
        transform.localScale = GameManager.Instance.SetGetPlayer.transform.localScale;

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        if (_spriteRenderer != null)
            Addressables.LoadAssetAsync<GameObject>("Death" + SaveAndLoadManager.GetStringValue(
                SaveAndLoadManager.CurrentBallSkinName)).Completed += OnPrefabLoaded;

        TrySpawnDeathParticles();
    }

    public void OnPrefabLoaded(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var handleRenderer = handle.Result.GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = handleRenderer.sprite;
            _spriteRenderer.color = handleRenderer.color;

            if (handle.Result.TryGetComponent(out Animator anim))
            {
                _animator.runtimeAnimatorController = anim.runtimeAnimatorController;
                _animator.SetTrigger("Death");
            }
        }
        else
            Debug.LogError("Failed to load prefab.");
    }

    private async void TrySpawnDeathParticles()
    {
        var key = "Death" + SaveAndLoadManager.GetStringValue(SaveAndLoadManager.CurrentBallSkinName) + "Particles";
        var locationsHandle = Addressables.LoadResourceLocationsAsync(key);
        await locationsHandle.Task;

        if (locationsHandle.Status == AsyncOperationStatus.Succeeded &&
            locationsHandle.Result != null &&
            locationsHandle.Result.Count > 0)
            Addressables.InstantiateAsync(key, transform.position, transform.rotation);
        else
            Debug.LogWarning($"Key '{key}' no existe en Addressables.");

        Addressables.Release(locationsHandle);
    }

}
