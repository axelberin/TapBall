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
            Addressables.LoadAssetAsync<Texture2D>("Death" + SaveAndLoadManager.GetStringValue(
                SaveAndLoadManager.CurrentBallSkinName)).Completed += OnSpriteLoaded;
    }

    public void OnSpriteLoaded(AsyncOperationHandle<Texture2D> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Texture2D texture = handle.Result;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            _spriteRenderer.sprite = sprite;
        }
    }
}
