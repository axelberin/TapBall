using UnityEngine;

public static class LoadAndSaveManager
{
    public static void SaveIntValue(int value, string parameterName, bool withSave = false)
    {
        PlayerPrefs.SetInt(parameterName, value);
        if (withSave)
            PlayerPrefs.Save();
    }

    public static void SaveFloatValue(float value, string parameterName, bool withSave = false)
    {
        PlayerPrefs.SetFloat(parameterName, value);
        if (withSave)
            PlayerPrefs.Save();
    }

    public static void SaveIntValue(string value, string parameterName, bool withSave = false)
    {
        PlayerPrefs.SetString(parameterName, value);
        if (withSave)
            PlayerPrefs.Save();
    }

    public static int GetIntValue(int value, string parameterName)
    {
        if (!PlayerPrefs.HasKey(parameterName))
        {
            Debug.LogWarning($"Does´t exist '{parameterName}'");
            return 0;
        }

        return PlayerPrefs.GetInt(parameterName);
    }

    public static float GetFloatValue(float value, string parameterName)
    {
        if (!PlayerPrefs.HasKey(parameterName))
        {
            Debug.LogWarning($"Does´t exist '{parameterName}'");
            return 0;
        }

        return PlayerPrefs.GetFloat(parameterName);
    }

    public static string GetStringValue(string value, string parameterName)
    {
        if (!PlayerPrefs.HasKey(parameterName))
        {
            Debug.LogWarning($"Does´t exist '{parameterName}'");
            return default;
        }

        return PlayerPrefs.GetString(parameterName);
    }
}
