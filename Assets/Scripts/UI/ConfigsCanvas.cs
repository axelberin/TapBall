using System;
using UnityEngine;
using UnityEngine.UI;

public class ConfigsCanvas : CanvasElementLocator
{
    public static ConfigsCanvas Instance { get; private set; }

    private Button _configsBackButton;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        _configsBackButton = FindAndValidateButtonComponent(transform, "ConfigsBackButton");
        _configsBackButton.onClick.AddListener(() =>
            UIManager.Instance.ChangeCanvas("ConfigsCanvas", GetCanvasFromGameMode(GameManager.Instance.GetCurrentGameMode)));

        UIManager.Instance.AddCanvas(gameObject, false);
    }

    private string GetCanvasFromGameMode(GameManager.GameModes gameMode)
    {
        switch (gameMode)
        {
            case GameManager.GameModes.Null:
                return "MenuManagerCanvas";
            case GameManager.GameModes.Dunk:
                return "DunkCanvas";
            default:
                return "MenuManagerCanvas";
        }
    }
}
