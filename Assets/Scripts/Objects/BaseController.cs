using UnityEngine;

public class BaseController : MovableObjects
{
    float _auxRotateSpeed = 1;

    protected override void Rotate()
    {
        if (transform.eulerAngles.z > 90 && transform.eulerAngles.z < 300) _auxRotateSpeed = 4;
        else _auxRotateSpeed = 1;

        if (transform.eulerAngles.z > 360) transform.eulerAngles = Vector3.zero;

        transform.eulerAngles += new Vector3(0, 0, _rotateSpeed * _auxRotateSpeed * Time.deltaTime);
    }
}
