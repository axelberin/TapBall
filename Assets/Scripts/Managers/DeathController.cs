using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityAddressables;

public class DeathController : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private Queue<GameObject> _deathsIntances = new();
    [SerializeField] private int _maxDeaths = 10;
    private void Start()
    {
        if (GameManager.Instance)
            GameManager.Instance.SetGetDeathController = this;

        AddressablesUtility.LoadAsset<GameObject>("Death" + SaveAndLoadManager.GetStringValue(
            SaveAndLoadManager.CurrentBallSkinName), go =>
            {
                _spriteRenderer = go.GetComponent<SpriteRenderer>();
                _animator = go.GetComponent<Animator>();
            });
    }
    public void ControlDeathShadows(DeathShadow deathShadow)
    {
        DeleteLastDeathShadowAfterMax(deathShadow.gameObject);
    }

    private void DeleteLastDeathShadowAfterMax(GameObject deathInstance)
    {
        _deathsIntances.Enqueue(deathInstance);

        if (_deathsIntances.Count > _maxDeaths)
        {
            GameObject oldestDeath = _deathsIntances.Dequeue();
            Destroy(oldestDeath);
        }
    }

    public Sprite GetSprite => _spriteRenderer.sprite;
    public Color GetSpriteColor => _spriteRenderer.color;

    public Animator Animator => _animator;
}
