using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private List<GameObject> _canvasesNmaes = new();

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
        if (!text)
            return;
        if (!text.isActiveAndEnabled)
            ActivateUI(text.gameObject, true);

        text.text = count.ToString();
    }

    public void SetText(TextMeshProUGUI text, float count)
    {
        if (!text)
            return;
        if (!text.isActiveAndEnabled)
            ActivateUI(text.gameObject, true);

        text.text = count.ToString();
    }

    public void AddCanvas(GameObject canvas, bool active)
    {
        if (!_canvasesNmaes.Contains(canvas))
            _canvasesNmaes.Add(canvas);

        canvas.SetActive(active);
    }

    public void RemoveNullsCnavases()
    {
        _canvasesNmaes.Clear();
    }

    public void ChangeCanvas(string canvasFrom, string canvasTo)
    {
        foreach (var canvas in _canvasesNmaes)
        {
            if (canvas.name == canvasFrom)
                canvas.SetActive(false);
            if (canvas.name == canvasTo)
                canvas.SetActive(true);
        }
    }
}
