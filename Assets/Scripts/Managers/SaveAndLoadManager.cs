using UnityEngine;
using System.Collections.Generic;

public static class SaveAndLoadManager
{
    public static string CoinsName = "Coins";
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

    private static SaveAndLoadOnCloudManager cloudManager;
    private static bool isLoadingFromCloud = false;

    public static void SetCloudManager(SaveAndLoadOnCloudManager manager)
    {
        cloudManager = manager;
    }

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
        // Guardar localmente
        PlayerPrefs.Save();

        // Guardar en la nube si está disponible
        if (cloudManager != null && !isLoadingFromCloud)
            cloudManager.SaveGameData(SerializeGameData());
    }

    // Método para aplicar datos cargados desde la nube
    public static void ApplyCloudData(string cloudData)
    {
        isLoadingFromCloud = true;

        try
        {
            GameData data = JsonUtility.FromJson<GameData>(cloudData);

            // Aplicar los datos deserializados
            if (data.coins >= 0)
                SetIntValue(data.coins, CoinsName);
            if (data.currentBallSkin >= 0)
                SetIntValue(data.currentBallSkin, CurrentBallSkinName);

            SetFloatValue(data.soundsVolume, SoundsVolumeName);
            SetFloatValue(data.musicVolume, MusicVolumeName);

            if (!string.IsNullOrEmpty(data.language))
                SetStringValue(data.language, LanguageName);

            SetIntValue(data.reviewSowed, ReviewSowed);

            // Aplicar datos de niveles
            foreach (var levelData in data.levelData)
            {
                SetIntValue(levelData.Value.level, DunkLevelName + levelData.Key);
                SetIntValue(levelData.Value.withoutDeath ? 1 : 0, DunkWithoutDeathName + levelData.Key);
                SetIntValue(levelData.Value.touchesComplete ? 1 : 0, DunkTouchesCompleteName + levelData.Key);
                SetIntValue(levelData.Value.coins, CoinNameByLevel + levelData.Key);
            }

            // Aplicar skins obtenidas
            foreach (var skinData in data.obtainedSkins)
                SetIntValue(skinData.Value ? 1 : 0, ObtainedBallSkins + skinData.Key);

            PlayerPrefs.Save();
            Debug.Log("Cloud data successfully applied to local save.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error applying cloud data: " + e.Message);
        }
        finally
        {
            isLoadingFromCloud = false;
        }
    }

    private static string[] _skinNames = new string[] {
        "BallBasicSkin","BallBasicRedSkin","BallBasicBlueSkin","BallBasicGreenSkin","BallBasicVioletSkin","BallBasicWhiteSkin","BallBasicBlackSkin",
        "CatBallSkin","DogBallSkin","CarpinchoBallSkin","PandaBallSkin","GrizzlyBallSkin",
        "FutbolBallSkin","BasketBallSkin","TennisBallSkin","BaseBallSkin",
        "MagmaBallSkin","WaterBallSkin",
    };

    // Serializar todos los datos del juego
    private static string SerializeGameData()
    {
        GameData data = new();

        // Datos básicos
        data.coins = GetIntValue(CoinsName);
        data.currentBallSkin = GetIntValue(CurrentBallSkinName);
        data.soundsVolume = GetFloatValue(SoundsVolumeName);
        data.musicVolume = GetFloatValue(MusicVolumeName);
        data.language = GetStringValue(LanguageName);
        data.reviewSowed = GetIntValue(ReviewSowed);

        for (int i = 1; i <= 50; i++) // Ajusta el rango según tus niveles
        {
            string levelKey = i.ToString();

            // Solo agregar si existe al menos un dato para este nivel
            if (PlayerPrefs.HasKey(DunkLevelName + levelKey) ||
                PlayerPrefs.HasKey(DunkWithoutDeathName + levelKey) ||
                PlayerPrefs.HasKey(DunkTouchesCompleteName + levelKey) ||
                PlayerPrefs.HasKey(CoinNameByLevel + levelKey))
            {
                LevelData levelData = new LevelData
                {
                    level = GetIntValue(DunkLevelName + levelKey),
                    withoutDeath = GetIntValue(DunkWithoutDeathName + levelKey) == 1,
                    touchesComplete = GetIntValue(DunkTouchesCompleteName + levelKey) == 1,
                    coins = GetIntValue(CoinNameByLevel + levelKey)
                };

                data.levelData[levelKey] = levelData;
            }
        }

        var skinKeys = new HashSet<string>();

        foreach (string skinName in _skinNames)
        {
            string skinPrefKey = ObtainedBallSkins + skinName;

            if (PlayerPrefs.HasKey(skinPrefKey))
            {
                skinKeys.Add(skinName);
            }
        }

        // Serializar skins encontradas por nombre
        foreach (string skinKey in skinKeys)
        {
            data.obtainedSkins[skinKey] = GetIntValue(ObtainedBallSkins + skinKey) == 1;
        }

        return JsonUtility.ToJson(data);
    }

    public static void DeleteData()
    {
        PlayerPrefs.DeleteAll();

        // También podrías implementar borrado en la nube si es necesario
        if (cloudManager != null)
            cloudManager.SaveGameData("{}"); // Guardar datos vacíos
    }
}

// Clases para la serialización
[System.Serializable]
public class GameData
{
    public int coins;
    public int currentBallSkin;
    public float soundsVolume;
    public float musicVolume;
    public string language;
    public int reviewSowed;
    public Dictionary<string, LevelData> levelData;
    public Dictionary<string, bool> obtainedSkins;

    public GameData()
    {
        levelData = new Dictionary<string, LevelData>();
        obtainedSkins = new Dictionary<string, bool>();
    }
}

[System.Serializable]
public class LevelData
{
    public int level;
    public bool withoutDeath;
    public bool touchesComplete;
    public int coins;
}