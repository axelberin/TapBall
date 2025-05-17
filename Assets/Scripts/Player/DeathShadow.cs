using UnityEngine;

public class DeathShadow : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private void Start()
    {
        transform.localScale = GameManager.Instance.SetGetPlayer.transform.localScale;

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        _spriteRenderer.sprite = GameManager.Instance.SetGetDeathController.SpriteRenderer.sprite;
        _spriteRenderer.color = GameManager.Instance.SetGetDeathController.SpriteRenderer.color;
        if (GameManager.Instance.SetGetDeathController.Animator != null)
        {
            _animator.runtimeAnimatorController = GameManager.Instance.SetGetDeathController.Animator.runtimeAnimatorController;
            _animator.SetTrigger("Death");
        }

        string key = "Death" + SaveAndLoadManager.GetStringValue(SaveAndLoadManager.CurrentBallSkinName) + "Particles";

        AddressablesManager.CheckThenLoadAsset<GameObject>(
               key, prefabCargado =>
               {
                   if (prefabCargado != null)
                   {
                       Instantiate(prefabCargado, transform.position, transform.rotation);
                   }
               });
    }
}
