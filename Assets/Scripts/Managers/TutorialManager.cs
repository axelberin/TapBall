using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : CanvasElementLocator
{
    public static TutorialManager Instance;

    private bool _inTutorial = true;

    private Animator _animator;
    private Image _tapTutorialImage;

    private void Awake()
    {
        if (GameManager.Instance.GetCurrentGameMode != GameManager.GameModes.Dunk)
        {
            FinishTutorial();
            return;
        }

        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    void Start()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();

        if (_tapTutorialImage == null)
            _tapTutorialImage = FindAndValidateComponent<Image>(transform, "TutorialImage");

        var nextTutorialButton = FindAndValidateComponent<Button>(transform, "NextTutorialBTN");
        nextTutorialButton.onClick.AddListener(() => FinishTutorial());

        StartCoroutine(DelayToAnim());
    }

    private IEnumerator DelayToAnim()
    {
        if (_animator == null)
            yield break;

        yield return new WaitForSeconds(0.3f);

        if (ScenesManager.Instance.GetLevelByCurrentScene() == 1)
            _animator.SetTrigger("TapTutorial");
        else if (ScenesManager.Instance.GetLevelByCurrentScene() == 2)
            _animator.SetTrigger("SlideTutorial");
    }

    private void FinishTutorial()
    {
        gameObject.SetActive(false);
        _inTutorial = false;
    }

    public bool GetInTutorial => _inTutorial;
}
