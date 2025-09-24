using Firebase.Crashlytics;

public static class GameLog
{
    public static void NonFatal(string tag, string msg, params (string key, object val)[] keys)
    {
        Crashlytics.Log($"{tag}: {msg}");
        foreach (var (key, val) in keys)
            Crashlytics.SetCustomKey(key, val?.ToString() ?? "null");
        Crashlytics.LogException(new System.Exception($"NonFatal::{tag}::{msg}"));
    }
}
