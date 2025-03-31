using UnityEngine;
using UnityEngine.UI;

public class ButtonsController : MonoBehaviour
{
    [SerializeField] private AudioManager.AudioClipType _audioClipType = AudioManager.AudioClipType.ButtonsSound;

    void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySoundByType(_audioClipType);
            else
                Debug.LogError("AudioManager is null");
        });
    }
}
