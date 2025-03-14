using System.Collections;
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

    public void LoadSceneAsync(string name, Animator fadeAnimator)
    {
        StartCoroutine(LoadSceneAsyncCoroutine(name, GetCurrentSceneName(), fadeAnimator));
    }

    private IEnumerator LoadSceneAsyncCoroutine(string sceneToLoadName,
        string lastSceneName, Animator fadeAnimator)
    {
        if (fadeAnimator != null)
            fadeAnimator.SetTrigger("Fade");

        yield return new WaitForSeconds(0.5f);

        var loadingScene = SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);

        loadingScene.allowSceneActivation = false;

        while (loadingScene.progress < 0.9f)
            yield return new WaitForEndOfFrame();


        loadingScene.allowSceneActivation = true;
        while (!loadingScene.isDone)
            yield return new WaitForEndOfFrame();

        UnloadScene(lastSceneName);

        yield return new WaitForSeconds(0.1f);

        var sceneToLoad = SceneManager.LoadSceneAsync(sceneToLoadName, LoadSceneMode.Additive);
        sceneToLoad.allowSceneActivation = false;

        while (sceneToLoad.progress < 0.9f)
            yield return new WaitForEndOfFrame();


        sceneToLoad.allowSceneActivation = true;
        while (!sceneToLoad.isDone)
            yield return new WaitForEndOfFrame();

        UnloadScene(SceneManager.GetSceneByName("LoadingScene").name);
        Application.targetFrameRate = 60;
    }

    public void UnloadScene(string sceneName)
    {
        SceneManager.UnloadSceneAsync(sceneName);
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

    public void LoadLevelByType(int level, GameManager.GameModes gameMode,
        Animator fadeAnimator)
    {
        switch (gameMode)
        {
            case GameManager.GameModes.Dunk:
                PauseAndResumeManager.Instance.RestartResumeAction();
                PauseAndResumeManager.Instance.RestartPauseAction();
                LoadSceneAsync("DunkLevel" + level, fadeAnimator);
                break;
            case GameManager.GameModes.Endless:
                break;
            case GameManager.GameModes.Time:
                break;
            default: break;
        }
    }
}
