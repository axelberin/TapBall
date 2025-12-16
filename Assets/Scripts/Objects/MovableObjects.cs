using UnityEngine;

public class MovableObjects : MonoBehaviour, IPauseble
{
    [SerializeField] bool _move;
    [SerializeField] float _movementSpeed = 3;
    [SerializeField] bool _rotate;
    [SerializeField] protected float _rotateSpeed = 3;

    [SerializeField] Transform[] _waypoints;

    int _waypointsIndex;
    float _initialMoveSpeed;
    float _initialRotationSpeed;
    Vector3 _initialPosition;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _initialMoveSpeed = _movementSpeed;
        _initialRotationSpeed = _rotateSpeed;
        _initialPosition = transform.position;

        if (_rb == null)
            _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (PauseAndResumeManager.Instance)
        {
            PauseAndResumeManager.Instance.AddResumeAction(OnResume);
            PauseAndResumeManager.Instance.AddPauseAction(OnPause);
        }
    }

    private void FixedUpdate()
    {
        if (_move) WaypointsPatrol();
        if (_rotate) Rotate();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController playerController))
            playerController.transform.parent = transform;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController playerController))
            playerController.transform.parent = null;
    }

    void WaypointsPatrol()
    {
        Vector2 current = _rb.position;
        Vector2 target = _waypoints[_waypointsIndex].position;
        Vector2 dir = target - current;

        if (dir.magnitude < 0.1f)
        {
            _waypointsIndex = (_waypointsIndex + 1) % _waypoints.Length;
            return;
        }

        Vector2 next = current + dir.normalized * (_movementSpeed * Time.fixedDeltaTime);
        _rb.MovePosition(next);
    }

    protected virtual void Rotate()
    {
        transform.eulerAngles += new Vector3(0, 0, _rotateSpeed * Time.fixedDeltaTime);
    }

    public void PlayMovement()
    {
        _movementSpeed = _initialMoveSpeed;
        _rotateSpeed = _initialRotationSpeed;
    }

    public void StopMovement()
    {
        _movementSpeed = 0;
        _rotateSpeed = 0;
    }

    public void ResetMovement()
    {
        transform.position = _initialPosition;
        transform.eulerAngles = Vector3.zero;
        StopMovement();
    }

    public virtual void OnResume()
    {
        PlayMovement();
    }

    public virtual void OnPause()
    {
        StopMovement();
    }
}
