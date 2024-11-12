using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float _jumpForce = 3;

    private bool _death;

    private Rigidbody2D _rb;
    private Collider2D _collider;

    void Awake()
    {
        if (!_rb)
            _rb = GetComponent<Rigidbody2D>();

        if (_collider == null)
            _collider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (GameManager.Instance)
            GameManager.Instance.SetGetPlayer = this;
    }

    public void AddForce(Vector3 touchPos)
    {
        _rb.velocity = Vector3.zero;

        Vector3 dir = (transform.position - touchPos).normalized;

        float clampY;

        if (dir.y >= 0)
            clampY = Mathf.Clamp(dir.y, 0.2f, 0.3f);
        else
            clampY = Mathf.Clamp(dir.y, -0.3f, -0.2f);

        dir = new Vector3(dir.x, clampY, dir.z);
        _rb.AddForce(dir * _jumpForce, ForceMode2D.Impulse);
    }

    public void Death()
    {
        _death = true;

        _collider.enabled = false;
        DeathShadow deathShadow = Factory.Instance.deathShadowPool.GetSingleObject();
        deathShadow.transform.position = transform.position;
        deathShadow.transform.localScale = transform.localScale;
        _collider.enabled = true;

        LevelManager.Instance.OnLose();
    }

    public bool HasDeath => _death;
    public Rigidbody2D GetRigidbody => _rb;
}
