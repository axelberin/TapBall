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

        var tutorialText = FindAndValidateComponent<TextMeshProUGUI>(transform, "TutorialText");
        var (text, font) = LanguageManager.Instance.GetlocalizatedTextAndFont("tutorial1");
        tutorialText.text = text;
        tutorialText.font = font;

        var nextTutorialButton = FindAndValidateComponent<Button>(transform, "NextTutorialBTN");
        nextTutorialButton.onClick.AddListener(() => FinishTutorial());
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

    public bool GetInTutorial => _inTutorial;
}
