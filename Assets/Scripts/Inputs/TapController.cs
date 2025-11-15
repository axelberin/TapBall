using UnityEngine;

public class TapController : MonoBehaviour
{
    [SerializeField] private float _swipeMinDistance = 100f; // en píxeles, ajustable
    private Vector2 _swipeStartPos;
    int _tapCount;
    private bool _tapEnabled = true;

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
        if (Input.GetMouseButtonDown(0))
        {
            _swipeStartPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            HandleInput(_swipeStartPos, Input.mousePosition);
        }
#elif UNITY_ANDROID
    if (Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            _swipeStartPos = touch.position;
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            HandleInput(_swipeStartPos, touch.position);
        }
    }
#endif
    }


    private void TapsBehaviourByMode(GameManager.GameModes gameModes, bool isSwipe)
    {
        switch (gameModes)
        {
            case GameManager.GameModes.Dunk:
                if (isSwipe)
                    _tapCount += 3;
                else
                    _tapCount++;
                break;
            case GameManager.GameModes.OneTouch:
                if (_tapCount > 0)
                    _tapCount--;
                else
                    GameManager.Instance.SetGetPlayer.Death();
                break;
        }

        LevelCanvas.Instance.OnTap(_tapCount);
    }

    private void HandleInput(Vector2 startPos, Vector2 endPos)
    {
        if (!_tapEnabled)
            return;

        if (!GameManager.Instance.SetGetPlayer || !LevelCanvas.Instance ||
            GameManager.Instance.SetGetPlayer.GetRigidbody.bodyType != RigidbodyType2D.Dynamic)
            return;

        // Diferencia en pantalla (en píxeles)
        Vector2 delta = endPos - startPos;

        bool isSwipe = delta.magnitude >= _swipeMinDistance;

        TapsBehaviourByMode(GameManager.Instance.GetCurrentGameMode, isSwipe);

        // ¿Es swipe?
        if (isSwipe)
        {
            // Convertimos a mundo para sacar dirección real
            Vector3 worldStart = Camera.main.ScreenToWorldPoint(startPos);
            Vector3 worldEnd = Camera.main.ScreenToWorldPoint(endPos);

            Vector2 swipeDir = worldEnd - worldStart;

            GameManager.Instance.SetGetPlayer.OnDash(swipeDir);
        }
        else
        {
            // Tap normal como antes
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(endPos);
            GameManager.Instance.SetGetPlayer.OnTap(worldPos);
        }
    }

    public void AddTouchesFromBubbles(int touchesToAdd)
    {
        _tapCount += touchesToAdd;
        LevelCanvas.Instance.OnTap(_tapCount);
        if (_tapCount < 0)
            GameManager.Instance.SetGetPlayer.Death();
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
