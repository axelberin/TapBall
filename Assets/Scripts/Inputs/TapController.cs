using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapController : MonoBehaviour
{
    int _tapCount;

    private void Start()
    {
        if (GameManager.Instance) GameManager.Instance.SetGetTapController = this;
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_EDITOR_WIN
        if (Input.GetKeyDown(KeyCode.Mouse0)) OnTap(Input.mousePosition);
#elif UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began) OnTap(touch.position);
        }
#endif
    }

    void OnTap(Vector3 pos)
    {
        if (!GameManager.Instance.SetGetPlayer || !DunkLevelCanvas.Instance ||
            GameManager.Instance.SetGetPlayer.GetRigidbody.bodyType != RigidbodyType2D.Dynamic)
            return;

        _tapCount++;
        if (DunkLevelCanvas.Instance)
            DunkLevelCanvas.Instance.OnTap(_tapCount);
        if (GameManager.Instance)
            GameManager.Instance.SetGetPlayer.AddForce(Camera.main.ScreenToWorldPoint(pos));
    }

    public int SetGetTapCount
    {
        set => _tapCount = value;
        get => _tapCount;
    }
}
