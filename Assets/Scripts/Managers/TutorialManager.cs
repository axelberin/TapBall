using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : CanvasElementLocator
{
    public static TutorialManager Instance;

    private bool _inTutorial = true;
    private int _tutorialIndex = 1;

    private Animator _animator;
    private Image _tapTutorialImage;

    private void Awake()
    {
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
            _tapTutorialImage = FindAndValidateImageComponent(transform, "TutorialImage");

        var nextTutorialButton = FindAndValidateButtonComponent(transform, "NextTutorialBTN");
        nextTutorialButton.onClick.AddListener(() =>
        {
            if (HasParameter("Tutorial" + _tutorialIndex, AnimatorControllerParameterType.Trigger))
            {
                if (_tapTutorialImage.rectTransform.localScale.x < 0)
                    FlipTutorial();

                _animator.SetTrigger("Tutorial" + _tutorialIndex);
                _tutorialIndex++;
            }
            else
                FinishTutorial();
        });
    }

    public void FlipTutorial()
    {
        _tapTutorialImage.rectTransform.localScale = new Vector3(_tapTutorialImage.rectTransform.localScale.x * -1,
            _tapTutorialImage.rectTransform.localScale.y, _tapTutorialImage.rectTransform.localScale.z);
    }

    private void FinishTutorial()
    {
        gameObject.SetActive(false);
        _inTutorial = false;
    }

    private bool HasParameter(string paramName, AnimatorControllerParameterType type)
    {
        foreach (AnimatorControllerParameter param in _animator.parameters)
        {
            if (param.name == paramName && param.type == type)
                return true;
        }

        return false;
    }

    public bool GetInTutorial => _inTutorial;
}
