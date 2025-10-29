using UnityEngine;

public class ActivateByModeController : MonoBehaviour
{
    void Start()
    {
        foreach (Transform child in transform)
        {
            if (child == null)
                continue;

            child.gameObject.SetActive(child.name.Contains(
                GameManager.Instance.GetCurrentGameMode.ToString()));
        }
    }
}
