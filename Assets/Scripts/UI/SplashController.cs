using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour
{
    private Animator _animator;
    private AudioSource _audioSource;

    private void Awake()
    {
        SetVariables();
    }

    private void Start()
    {
        StartCoroutine(SplashSquence());
    }

    private IEnumerator SplashSquence()
    {
        if (_audioSource == null || _animator == null)
        {
            SetVariables();
            yield return null;
        }

        _animator.Play("SplashAnim");
        yield return new WaitForSeconds(0.25f);
        _audioSource.Play();
        yield return new WaitForSeconds(0.25f);
        _audioSource.Play();
        yield return new WaitForSeconds(0.25f);
        _audioSource.Play();
        yield return new WaitForSeconds(0.3f);
        LoadNextScene();
    }

    private void SetVariables()
    {
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
    }

    private void LoadNextScene()
    {
        SceneManager.LoadSceneAsync("LoadingGameScene");
    }
}
