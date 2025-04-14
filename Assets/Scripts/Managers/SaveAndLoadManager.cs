using UnityEngine;

public static class SaveAndLoadManager
{
    public static string CoinsName = "Coins";
    /// <summary>
    /// The name + the game mode + level.
    /// </summary>
    public static string CoinNameByLevel = "Coin_";
    public static string DunkLevelName = "DunkLevel_";
    public static string DunkWithoutDeathName = "DunkWithoutDeath_";
    public static string DunkTouchesCompleteName = "DunkTouchesComplete_";

    public static string CurrentBallSkinName = "CurrentBallSkin";
    public static string ObtainedBallSkins = "BallSkin_";

    public static string SoundsVolumeName = "SoundsVolume";
    public static string MusicVolumeName = "MusicVolume";
    public static string LanguageName = "Language";

    public static string ReviewSowed = "ReviewSowed";

    public static void SetIntValue(int value, string parameterName, bool withSave = false)
    {
        PlayerPrefs.SetInt(parameterName, value);
        if (withSave)
            Save();
    }

    public static void SetFloatValue(float value, string parameterName, bool withSave = false)
    {
        PlayerPrefs.SetFloat(parameterName, value);
        if (withSave)
            Save();
    }

    public static void SetStringValue(string value, string parameterName, bool withSave = false)
    {
        PlayerPrefs.SetString(parameterName, value);
        if (withSave)
            Save();
    }

    public static int GetIntValue(string parameterName)
    {
        if (!ContainsKey(parameterName))
        {
            Debug.LogWarning($"Doesn´t exist '{parameterName}'");
            return default;
        }

        return PlayerPrefs.GetInt(parameterName);
    }

    public static float GetFloatValue(string parameterName)
    {
        if (!ContainsKey(parameterName))
        {
            Debug.LogWarning($"Doesn´t exist '{parameterName}'");
            return default;
        }

        return PlayerPrefs.GetFloat(parameterName);
    }

    public static string GetStringValue(string parameterName)
    {
        if (!ContainsKey(parameterName))
        {
            Debug.LogWarning($"Doesn´t exist '{parameterName}'");
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
