using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DeathShadow : MonoBehaviour, ISkinLoader
{
    private SpriteRenderer _spriteRenderer;
    private ParticleSystem _particleSystem;

    private void Start()
    {
        transform.localScale = GameManager.Instance.SetGetPlayer.transform.localScale;

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _particleSystem = GetComponent<ParticleSystem>();

        if (_spriteRenderer != null)
            Addressables.LoadAssetAsync<GameObject>("Death" + SaveAndLoadManager.GetStringValue(
                SaveAndLoadManager.CurrentBallSkinName)).Completed += OnPrefabLoaded;
    }

    public void OnPrefabLoaded(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _spriteRenderer.sprite = handle.Result.GetComponent<SpriteRenderer>().sprite;
            //Debug.Log(handle.Result.GetComponent<ParticleSystem>().name);
            //UtilityFuntions.CopyCompleteParticleSystem(handle.Result.GetComponent<ParticleSystem>(), ref _particleSystem);
            //_particleSystem.Play();
        }
        else
            Debug.LogError("Failed to load prefab.");
    }
}
