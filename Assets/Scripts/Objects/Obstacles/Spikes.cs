using System.Collections;
using UnityEngine;

public class Spikes : ObstaclesManager
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float _animatorSpeed = 1;
    [SerializeField] private float _animationDelay = 3;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        if (_animator != null)
        {
            _animator.speed = _animatorSpeed;
            StartCoroutine(StartAnim(_animationDelay +
                (_animator.runtimeAnimatorController.animationClips[0].length / _animatorSpeed)));
        }
    }

    private IEnumerator StartAnim(float delay)
    {
        _animator.SetTrigger("Out");
        yield return new WaitForSeconds(delay);
        _animator.SetTrigger("In");
        yield return new WaitForSeconds(delay);

        StartCoroutine(StartAnim(delay));
    }
}
