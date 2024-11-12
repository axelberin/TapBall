using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platforms : MonoBehaviour
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

    private void Awake()
    {
        _initialMoveSpeed = _movementSpeed;
        _initialRotationSpeed = _rotateSpeed;
        _initialPosition = transform.position;
    }

    private void Update()
    {
        if (_move) WaypointsPatrol();
        if (_rotate) Rotate();
    }

    void WaypointsPatrol()
    {
        Vector3 dir = _waypoints[_waypointsIndex].position - transform.position;
        if (dir.magnitude < 0.1f)
        {
            _waypointsIndex++;
            if (_waypointsIndex > _waypoints.Length - 1)
            {
                _waypointsIndex = 0;
            }
        }
        transform.position += dir.normalized * (_movementSpeed * Time.deltaTime);
    }

    protected virtual void Rotate()
    {
        transform.eulerAngles += new Vector3(0, 0, _rotateSpeed * Time.deltaTime);
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
}
