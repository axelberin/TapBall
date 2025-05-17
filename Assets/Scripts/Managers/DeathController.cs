using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathController : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private void Start()
    {
        if (GameManager.Instance)
            GameManager.Instance.SetGetDeathController = this;

        AddressablesManager.LoadAsset<GameObject>("Death" + SaveAndLoadManager.GetStringValue(
            SaveAndLoadManager.CurrentBallSkinName), go =>
            {
                _spriteRenderer = go.GetComponent<SpriteRenderer>();
                _animator = go.GetComponent<Animator>();
            });
    }

    public SpriteRenderer SpriteRenderer => _spriteRenderer;

    public Animator Animator => _animator;
}
