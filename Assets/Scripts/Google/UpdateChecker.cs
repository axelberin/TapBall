using Google.Play.AppUpdate;
using Google.Play.Common;
using System.Collections;

public class UpdateChecker : ManagersManager
{
    private AppUpdateManager _appUpdateManager;
    private AppUpdateInfo _appUpdateInfoResult;

    protected override void Start()
    {
#if UNITY_ANDROID || UNITY_ANDROID_API
        _appUpdateManager = new AppUpdateManager();
        base.Start();
#endif
    }

    private IEnumerator StartFlexibleUpdate()
    {
        var appUpdateOptions =
          AppUpdateOptions.FlexibleAppUpdateOptions(allowAssetPackDeletion: true);
        // Creates an AppUpdateRequest that can be used to monitor the
        // requested in-app update flow.
        var startUpdateRequest = _appUpdateManager.StartUpdate(
          // The result returned by PlayAsyncOperation.GetResult().
          _appUpdateInfoResult,
          // The AppUpdateOptions created defining the requested in-app update
          // and its parameters.
          appUpdateOptions);

        while (!startUpdateRequest.IsDone)
        {
            // For flexible flow,the user can continue to use the app while
            // the update downloads in the background. You can implement a
            // progress bar showing the download status during this time.
            yield return null;
        }

    }

    private IEnumerator CompleteFlexibleUpdate()
    {
        var result = _appUpdateManager.CompleteUpdate();
        yield return result;

        // If the update completes successfully, then the app restarts and this line
        // is never reached. If this line is reached, then handle the failure (e.g. by
        // logging result.Error or by displaying a message to the user).
    }

    IEnumerator StartImmediateUpdate()
    {
        var appUpdateOptions =
          AppUpdateOptions.ImmediateAppUpdateOptions(allowAssetPackDeletion: true);
        // Creates an AppUpdateRequest that can be used to monitor the
        // requested in-app update flow.
        var startUpdateRequest = _appUpdateManager.StartUpdate(
          // The result returned by PlayAsyncOperation.GetResult().
          _appUpdateInfoResult,
          // The AppUpdateOptions created defining the requested in-app update
          // and its parameters.
          appUpdateOptions);
        yield return startUpdateRequest;

        // If the update completes successfully, then the app restarts and this line
        // is never reached. If this line is reached, then handle the failure (for
        // example, by logging result.Error or by displaying a message to the user).
    }

    public override IEnumerator InizializeManagers()
    {
        PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation =
          _appUpdateManager.GetAppUpdateInfo();

        // Wait until the asynchronous operation completes.
        yield return appUpdateInfoOperation;

        if (appUpdateInfoOperation != null && appUpdateInfoOperation.IsSuccessful)
        {
            var appUpdateInfoResult = appUpdateInfoOperation.GetResult();
            var stalenessDays = appUpdateInfoOperation.GetResult().ClientVersionStalenessDays;
            // Check AppUpdateInfo's UpdateAvailability, UpdatePriority,
            // IsUpdateTypeAllowed(), ... and decide whether to ask the user
            // to start an in-app update.
            if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateAvailable)
            {
                // Start an in-app update.
                // StartFlexibleUpdate() or StartImmediateUpdate().
                if (stalenessDays > 1)
                    yield return StartCoroutine(StartFlexibleUpdate());
                else if (stalenessDays > 10)
                    yield return StartCoroutine(StartImmediateUpdate());
            }
        }

        _isInitialized = true;
    }
}
