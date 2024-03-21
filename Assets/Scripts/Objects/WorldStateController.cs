using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldStateController : MonoBehaviour
{
    public Action OnUpdate = delegate { };

    [SerializeField] int _level;
    [SerializeField] PlayerController _playerController;
    [SerializeField] Vector3 _playerInitialPos;

    float _timeToWin = 3;
    float _timeToStart = 0;

    BaseController _baseController;

    private void Start()
    {
        GameManager.Instance.SetGetWorldState = this;

        if (!_baseController) _baseController = GetComponentInParent<BaseController>();
        if (_baseController) _baseController.StopMovement();

        if (!_playerController)
        {
            if (GameManager.Instance.SetGetPlayer) _playerController = GameManager.Instance.SetGetPlayer;
            else _playerController = FindObjectOfType<PlayerController>();
        }

        if (_playerInitialPos == Vector3.zero) _playerInitialPos = _playerController.transform.position;
        _playerController.GetRigidbody.bodyType = RigidbodyType2D.Static;

        OnUpdate = StartCount;
    }

    private void Update()
    {
        OnUpdate?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>()) OnUpdate = WinCount;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>())
        {
            OnUpdate -= WinCount;
            _timeToWin = 3;
            if (UIManager.Instance) UIManager.Instance.ActivateUI(UIManager.Instance.winTime.gameObject, false);
        }
    }

    void WinCount()
    {
        if (_timeToWin > 0)
        {
            _timeToWin -= Time.deltaTime;
            if (UIManager.Instance) UIManager.Instance.SetText(UIManager.Instance.winTime, (int)(_timeToWin + 1));
        }
        else
        {
            if (_baseController) _baseController.StopMovement();
            GameManager.Instance.OnWin();
            OnUpdate -= WinCount;
        }
    }

    public void StartCount()
    {
        if (_timeToStart < 3)
        {
            _timeToStart += Time.deltaTime;
            if (_timeToStart > 3) _timeToStart = 3;
            if (UIManager.Instance) UIManager.Instance.SetText(UIManager.Instance.winTime, (int)(_timeToStart + 1));
        }
        else
        {
            _playerController.GetRigidbody.bodyType = RigidbodyType2D.Dynamic;
            if (_baseController) _baseController.PlayMovement();
            if (UIManager.Instance) UIManager.Instance.ActivateUI(UIManager.Instance.winTime.gameObject, false);
            _timeToStart = 0;
            OnUpdate = null;
        }
    }

    public int GetLevel => _level;
    public Vector3 GetInitalPos => _playerInitialPos;
    public BaseController GetBaseController => _baseController;
}
