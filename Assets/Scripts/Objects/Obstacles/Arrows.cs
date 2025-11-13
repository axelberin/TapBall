using UnityEngine;

public class Arrows : ObstaclesManager
{
    private Animator _animator;
    private AudioSource _audioSource;

    private void Start()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();

        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerController player) && _animator != null)
        {
            player.GetRigidbody.linearVelocity = Vector3.zero;
            _animator.SetTrigger("Interact");
            _audioSource.Play();
        }
    }
}
