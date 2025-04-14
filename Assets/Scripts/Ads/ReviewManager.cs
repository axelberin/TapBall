using UnityEngine;
using Google.Play.Review;
using System.Collections;

public class ReviewManagerController : MonoBehaviour
{
    public static ReviewManagerController Instance { get; private set; }

    private ReviewManager _reviewManager;
    private PlayReviewInfo _playReviewInfo;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void RequestReview()
    {
        StartCoroutine(LaunchReviewFlow());
    }

    private IEnumerator LaunchReviewFlow()
    {
        _reviewManager = new ReviewManager();

        var requestFlowOperation = _reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;

        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        {
            Debug.Log("Error solicitando el flujo de reseñas: " + requestFlowOperation.Error);
            yield break;
        }

        _playReviewInfo = requestFlowOperation.GetResult();

        var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
        yield return launchFlowOperation;

        _playReviewInfo = null;

        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {
            Debug.Log("Error al mostrar el flujo de reseñas: " + launchFlowOperation.Error);
        }
        else
        {
            Debug.Log("Reseña mostrada con éxito (si el usuario cumple los criterios de Google)");
        }
    }
}
