using UnityEngine;
using UnityEngine.UI;

public class ButtonsController : MonoBehaviour
{
    [SerializeField] private AudioManager.AudioClipType _audioClipType = AudioManager.AudioClipType.ButtonsSound;
    [SerializeField] private bool _isActive = true;
    [SerializeField] Transform _textParentTransform;

    void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySoundByType(_audioClipType);
            else
                Debug.LogError("AudioManager is null");

            if (!_isActive && _textParentTransform != null)
                StartCoroutine(UIManager.Instance.ShowComingSoonNotify(_textParentTransform));
        });
    }
}
