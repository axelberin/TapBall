using UnityEngine;

public static class SaveAndLoadManager
{
    public static string CoinsName = "Coins";
    public static string DunkBestName = "DunkBest_";
    public static string DunkLevelName = "DunkLevel_";
    public static string DunkWithoutDeathName = "DunkWithoutDeath_";

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

    public static int GetIntValue(string parameterName)
    {
        if (!ContainsKey(parameterName))
        {
            Debug.LogWarning($"Does´t exist '{parameterName}'");
            return default;
        }

        return PlayerPrefs.GetInt(parameterName);
    }

    public static float GetFloatValue(string parameterName)
    {
        if (!ContainsKey(parameterName))
        {
            Debug.LogWarning($"Does´t exist '{parameterName}'");
            return default;
        }

        return PlayerPrefs.GetFloat(parameterName);
    }

    public static string GetStringValue(string parameterName)
    {
        if (!ContainsKey(parameterName))
        {
            Debug.LogWarning($"Does´t exist '{parameterName}'");
            return default;
        }

        return PlayerPrefs.GetString(parameterName);
    }

    public static bool ContainsKey(string parameterName)
    {
        return PlayerPrefs.HasKey(parameterName);
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static void DeleteData()
    {
        PlayerPrefs.DeleteAll();
    }
}
