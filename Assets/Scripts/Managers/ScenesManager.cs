using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    public static ScenesManager Instance;

    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(this);
    }

    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
        Application.targetFrameRate = 60;
    }

    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void LoadNextLevel()
    {
        switch (GameManager.Instance.GetCurrentGameMode)
        {
            case GameManager.GameModes.Dunk:
                LoadScene("DunkLevel" + (GameManager.Instance.SetGetWorldState.GetLevel + 1));
                break;
            case GameManager.GameModes.Endless:
                break;
            case GameManager.GameModes.Time:
                break;
            default: break;
        }
    }
}
