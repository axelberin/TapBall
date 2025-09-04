using TMPro;
using UnityEngine;

public class FPSControlller : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    public float time = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (!fpsText) fpsText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (time >= 0.5f)
        {
            int fps = (int)(1 / Time.unscaledDeltaTime);
            UIManager.Instance.SetText(fpsText, fps);
            time = 0;
        }
        time += Time.unscaledDeltaTime;
    }
}
