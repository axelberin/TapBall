using UnityEngine;

public class TapController : MonoBehaviour
{
    int _tapCount;
    private bool _tapEnabled = true;
    private bool _tapCountEnabled = true;
    private void Start()
    {
        if (GameManager.Instance)
            GameManager.Instance.SetGetTapController = this;

        if (GameManager.Instance.GetCurrentGameMode == GameManager.GameModes.OneTouch)
        {
            _tapCount = GameManager.Instance.SetGetWorldState.GetLimitTapsOneTouch;
        }
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_EDITOR_WIN
        if (Input.GetKeyDown(KeyCode.Mouse0))
            OnTap(Input.mousePosition);
#elif UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began) OnTap(touch.position);
        }
#endif
    }

    private void TapsBehaviourByMode(GameManager.GameModes gameModes)
    {
        if (PowerUpManager.Instance.PowerUpTapsEnabled)
            return;
        switch (gameModes)
        {
            case GameManager.GameModes.Dunk:
                _tapCount++;
                LevelCanvas.Instance.OnTap(_tapCount);
                break;
            case GameManager.GameModes.OneTouch:
                if (_tapCount > 0)
                {
                    _tapCount--;
                    LevelCanvas.Instance.OnTap(_tapCount);
                    if (_tapCount == 0)
                        GameManager.Instance.SetGetPlayer.Death();
                }
                break;
        }
    }

    void OnTap(Vector3 pos)
    {
        if (!_tapEnabled)
            return;
        if (!GameManager.Instance.SetGetPlayer || !LevelCanvas.Instance ||
            GameManager.Instance.SetGetPlayer.GetRigidbody.bodyType != RigidbodyType2D.Dynamic)
            return;
        TapsBehaviourByMode(GameManager.Instance.GetCurrentGameMode);
        if (GameManager.Instance)
            GameManager.Instance.SetGetPlayer.OnTap(Camera.main.ScreenToWorldPoint(pos));
    }

    public int SetGetTapCount
    {
        set => _tapCount = value;
        get => _tapCount;
    }

    public bool SetGetTapEnabled
    {
        set => _tapEnabled = value;
        get => _tapEnabled;
    }
}
