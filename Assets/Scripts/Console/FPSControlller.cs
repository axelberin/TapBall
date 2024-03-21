using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSControlller : MonoBehaviour
{
    public TMP_Text fpsText;
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
            fpsText.text = fps.ToString();
            time = 0;
        }
        time += Time.unscaledDeltaTime;
    }
}
