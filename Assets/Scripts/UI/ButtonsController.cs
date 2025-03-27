using UnityEngine;
using UnityEngine.UI;

public class ButtonsController : MonoBehaviour
{
    void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySoundByType(AudioManager.AudioClipType.ButtonsSound);
            else
                Debug.LogError("AudioManager is null");
        });
    }
}
