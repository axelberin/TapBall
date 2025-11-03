using TMPro;
using UnityEngine;

public class Bubbles : ObstaclesManager
{
    [SerializeField] private int _touchesToAdd = 1;

    private Animator _animator;
    private CircleCollider2D _collider;
    private TMP_Text _touchesText;

    private void Start()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();

        if (_collider == null)
            _collider = GetComponent<CircleCollider2D>();

        if (_touchesText == null)
            _touchesText = GetComponentInChildren<TMP_Text>();

        if (_touchesText != null)
            _touchesText.SetText(_touchesToAdd > 0 ? $"+{_touchesToAdd}" : _touchesToAdd.ToString());
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
