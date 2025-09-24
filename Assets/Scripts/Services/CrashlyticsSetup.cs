using Firebase;
using Firebase.Crashlytics;
using Firebase.Auth;
using UnityEngine;

public class CrashlyticsSetup : MonoBehaviour
{
    async void Awake()
    {
        await FirebaseApp.CheckAndFixDependenciesAsync();
        var userId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId ?? "anon";
        Crashlytics.SetUserId(userId);

        Crashlytics.SetCustomKey("app_version", Application.version);
        Crashlytics.SetCustomKey("build", Application.buildGUID);
        Crashlytics.SetCustomKey("unity", Application.unityVersion);
        Crashlytics.SetCustomKey("device", SystemInfo.deviceModel);
        Crashlytics.SetCustomKey("os", SystemInfo.operatingSystem);

        // Capturar todos los logs de Unity
        Application.logMessageReceived += OnLogMessageReceived;
        DontDestroyOnLoad(this);
    }

    void OnDestroy() => Application.logMessageReceived -= OnLogMessageReceived;

    void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        // Mandá errores y excepciones como non-fatal (no detiene la app)
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            Crashlytics.Log(condition);
            Crashlytics.LogException(new System.Exception($"{type}: {condition}\n{stackTrace}"));
        }
        else if (type == LogType.Warning)
        {
            // Útil para ver patrones de warnings sin inundar
            Crashlytics.Log($"WARN: {condition}");
        }
    }
}
