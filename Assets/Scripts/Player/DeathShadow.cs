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
        {
            if (!SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CurrentBallSkin))
                SaveAndLoadManager.SetStringValue(SaveAndLoadManager.CurrentBallSkin, SaveAndLoadManager.CurrentBallSkin);

            Addressables.LoadAssetAsync<Sprite>(SaveAndLoadManager.GetStringValue(
                SaveAndLoadManager.CurrentBallSkin)).Completed += OnSpriteLoaded;
        }
    }

    public void OnSpriteLoaded(AsyncOperationHandle<Sprite> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _spriteRenderer.sprite = handle.Result;
        }
    }
}
