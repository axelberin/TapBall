using UnityEngine;

public class Arrows : ObstaclesManager
{
    private Animator _animator;

    private void Start()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() && _animator != null)
            _animator.SetTrigger("Interact");
    }
}
