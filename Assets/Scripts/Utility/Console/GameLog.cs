using Firebase.Analytics;
using Firebase.Crashlytics;
using UnityEngine;

public static class GameLog
{
    public static void NonFatal(string tag, string msg, params (string key, object val)[] keys)
    {
        Crashlytics.Log($"{tag}: {msg}");
        foreach (var (key, val) in keys)
            Crashlytics.SetCustomKey(key, val?.ToString() ?? "null");
        Crashlytics.LogException(new System.Exception($"NonFatal::{tag}::{msg}"));
    }

    public static void LogEvent(string eventName, params (string key, object val)[] parameters)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogWarning("GameLog.LogEvent llamado con eventName vacío");
            return;
        }

        // Convertimos los parámetros al formato de Firebase
        var paramList = new System.Collections.Generic.List<Parameter>();
        foreach (var (key, val) in parameters)
        {
            if (val is int i) paramList.Add(new Parameter(key, i));
            else if (val is long l) paramList.Add(new Parameter(key, l));
            else if (val is float f) paramList.Add(new Parameter(key, f));
            else if (val is double d) paramList.Add(new Parameter(key, d));
            else if (val is string s) paramList.Add(new Parameter(key, s));
            else if (val != null) paramList.Add(new Parameter(key, val.ToString()));
        }

        FirebaseAnalytics.LogEvent(eventName, paramList.ToArray());

        Debug.Log($"[Analytics] Event: {eventName} ({paramList.Count} params)");
    }

}
