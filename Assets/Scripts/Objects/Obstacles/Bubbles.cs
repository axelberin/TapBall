using UnityEngine;

public class Bubbles : ObstaclesManager
{
    private Animator _animator;
    private CircleCollider2D _collider;

    private void Start()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();

        if (_collider == null)
            _collider = GetComponent<CircleCollider2D>();
    }

    private void OnEnable()
    {
        if (LevelManager.Instance)
            LevelManager.Instance.OnLoseLevel += OnLose;
    }

    private void OnDisable()
    {
        if (LevelManager.Instance)
            LevelManager.Instance.OnLoseLevel -= OnLose;
    }

    private void OnLose()
    {
        if (_animator != null)
            _animator.SetTrigger("OnLose");

        if (_collider != null)
            _collider.enabled = true;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerController player) && _animator != null
            && _collider != null)
        {
            _animator.SetTrigger("Interact");
            _collider.enabled = false;
        }
    }
}
