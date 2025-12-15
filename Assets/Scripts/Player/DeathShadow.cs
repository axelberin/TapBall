using UnityEngine;
using UtilityAddressables;

public class DeathShadow : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private void Start()
    {
        transform.localScale = GameManager.Instance.SetGetPlayer.transform.localScale;

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        _spriteRenderer.sprite = GameManager.Instance.SetGetDeathController.GetSprite;
        _spriteRenderer.color = GameManager.Instance.SetGetDeathController.GetSpriteColor;
        if (GameManager.Instance.SetGetDeathController.Animator != null)
        {
            _animator.runtimeAnimatorController = GameManager.Instance.SetGetDeathController.Animator.runtimeAnimatorController;
            _animator.SetTrigger("Death");
        }

        AddressablesUtility.CheckThenLoadAsset<GameObject>(
               "Death" + SaveAndLoadManager.GetStringValue(SaveAndLoadManager.CurrentBallSkinName) + "Particles",
               prefabCargado =>
               {
                   if (prefabCargado != null)
                       Instantiate(prefabCargado, transform.position, transform.rotation);
               });

        GameManager.Instance.SetGetDeathController.ControlDeathShadows(this);
    }
}
