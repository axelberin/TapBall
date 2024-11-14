using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    public static ScenesManager Instance;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);
    }

    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
        Application.targetFrameRate = 60;
    }

    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    public bool IsSceneExisting(string sceneName)
    {
        // Busca la escena por su nombre
        int sceneIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);

        // Si el índice es -1, la escena no está en las Build Settings
        return sceneIndex != -1;
    }

    public int GetLevelByCurrentScene()
    {
        string numbers = new string(GetCurrentSceneName().Where(
            char.IsDigit).ToArray());
        int.TryParse(numbers, out int level);
        return level;
    }

    public void LoadNextLevel(int level)
    {
        switch (GameManager.Instance.GetCurrentGameMode)
        {
            case GameManager.GameModes.Dunk:
                PauseAndResumeManager.Instance.RestartResumeAction();
                PauseAndResumeManager.Instance.RestartPauseAction();
                LoadScene("DunkLevel" + (level + 1));
                break;
            case GameManager.GameModes.Endless:
                break;
            case GameManager.GameModes.Time:
                break;
            default: break;
        }
    }
}
