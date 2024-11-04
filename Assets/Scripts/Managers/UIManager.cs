using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TextMeshProUGUI pointsCount;
    public TextMeshProUGUI winTime;
    public TextMeshProUGUI winText;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else 
            Destroy(this);
    }

    public void ActivateUI(GameObject gameObject, bool active)
    {
        gameObject.SetActive(active);
    }

    public void SetText(TextMeshProUGUI text, int count)
    {
        if (!text) return;
        if (!text.isActiveAndEnabled) ActivateUI(text.gameObject, true);

        text.text = count.ToString();
    }

    public void SetText(TextMeshProUGUI text, float count)
    {
        if (!text) return;
        if (!text.isActiveAndEnabled) ActivateUI(text.gameObject, true);

        text.text = count.ToString();
    }
}
