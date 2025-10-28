using UnityEngine;
using Google.Play.Review;
using System.Collections;

public class ReviewManagerController : MonoBehaviour
{
#if !UNITY_IOS
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
            OpenReviewFromLink();
            yield break;
        }

        _playReviewInfo = requestFlowOperation.GetResult();

        var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
        yield return launchFlowOperation;

        _playReviewInfo = null;

        OpenReviewFromLink();

        yield return null;
    }

    private void OpenReviewFromLink()
    {
        Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);
    }
#endif
}
