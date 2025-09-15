using UnityEngine;
using System.Collections.Generic;
using System;
using static GameManager;

public static class SaveAndLoadManager
{
    // Nombres de parámetros base
    public static string CoinsName = "Coins";
    public static string OrbsName = "Orbs";
    public static string CurrentBallSkinName = "CurrentBallSkin";
    public static string ObtainedBallSkins = "BallSkin_";
    public static string SoundsVolumeName = "SoundsVolume";
    public static string MusicVolumeName = "MusicVolume";
    public static string LanguageName = "Language";
    public static string ReviewSowedName = "ReviewSowed";
    public static string NoAdsBougthName = "NoAdsBougth";
    public static string CurrentWorldName = "CurrentWorld";
    public static string CurrentModeName = "CurrentMode";

    // DEPRECATED - Mantener para compatibilidad hacia atrás
    public static string CoinNameByLevel = "Coin_";
    public static string DunkLevelName = "DunkLevel_";
    public static string DunkWithoutDeathName = "DunkWithoutDeath_";
    public static string DunkTouchesCompleteName = "DunkTouchesComplete_";

    // Nueva estructura: Modo_Mundo_Nivel_TipoDato
    private static string LevelDataPrefix = "LevelData_";
    private static string CoinSuffix = "_Coin";
    private static string WithoutDeathSuffix = "_WithoutDeath";
    private static string ObjectiveCompleteSuffix = "_ObjectiveComplete";

    private static bool isLoadingFromCloud = false;

    // Mundos disponibles - actualiza esta lista cuando agregues mundos
    private static string[] _availableWorlds = new string[] { "Neon" }; // Temporal hasta que definas los mundos reales

    #region Basic Data Methods
    public static void SetIntValue(int value, string parameterName, bool withSave = false, bool saveCloud = false)
    {
        PlayerPrefs.SetInt(parameterName, value);
        if (withSave)
            Save();
        if (saveCloud)
            SaveCloud();
    }

    public static void SetFloatValue(float value, string parameterName, bool withSave = false, bool saveCloud = false)
    {
        PlayerPrefs.SetFloat(parameterName, value);
        if (withSave)
            Save();
        if (saveCloud)
            SaveCloud();
    }

    public static void SetStringValue(string value, string parameterName, bool withSave = false, bool saveCloud = false)
    {
        PlayerPrefs.SetString(parameterName, value);
        if (withSave)
            Save();
        if (saveCloud)
            SaveCloud();
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
    #endregion

    #region Level Data Methods - Nueva Estructura
    /// <summary>
    /// Genera la clave para un dato específico de un nivel
    /// Formato: LevelData_[GameMode]_[World]_[Level]_[DataType]
    /// </summary>
    private static string GenerateLevelDataKey(GameModes gameMode, string world, int level, string dataSuffix)
    {
        return $"{LevelDataPrefix}{gameMode}_{world}_{level}{dataSuffix}";
    }

    /// <summary>
    /// Guarda si el jugador obtuvo la moneda en un nivel específico
    /// </summary>
    public static void SetLevelCoinObtained(GameModes gameMode, string world, int level, bool obtained, bool withSave = false, bool saveCloud = false)
    {
        SetIntValue(obtained ? 1 : 0, GenerateLevelDataKey(gameMode, world, level, CoinSuffix), withSave, saveCloud);
    }

    /// <summary>
    /// Obtiene si el jugador obtuvo la moneda en un nivel específico
    /// </summary>
    public static bool GetLevelCoinObtained(GameModes gameMode, string world, int level)
    {
        return GetIntValue(GenerateLevelDataKey(gameMode, world, level, CoinSuffix)) == 1;
    }

    /// <summary>
    /// Guarda si el jugador completó el nivel sin morir
    /// </summary>
    public static void SetLevelWithoutDeath(GameModes gameMode, string world, int level, bool withoutDeath, bool withSave = false, bool saveCloud = false)
    {
        SetIntValue(withoutDeath ? 1 : 0, GenerateLevelDataKey(gameMode, world, level, WithoutDeathSuffix), withSave, saveCloud);
    }

    /// <summary>
    /// Obtiene si el jugador completó el nivel sin morir
    /// </summary>
    public static bool GetLevelWithoutDeath(GameModes gameMode, string world, int level)
    {
        return GetIntValue(GenerateLevelDataKey(gameMode, world, level, WithoutDeathSuffix)) == 1;
    }

    /// <summary>
    /// Guarda si el jugador cumplió el objetivo específico del modo de juego
    /// Dunk: toques máximos, Time: tiempo límite, OneTouch: límite de toques, etc.
    /// </summary>
    public static void SetLevelObjectiveComplete(GameModes gameMode, string world, int level, bool objectiveComplete, bool withSave = false, bool saveCloud = false)
    {
        SetIntValue(objectiveComplete ? 1 : 0, GenerateLevelDataKey(gameMode, world, level, ObjectiveCompleteSuffix), withSave, saveCloud);
    }

    /// <summary>
    /// Obtiene si el jugador cumplió el objetivo específico del modo de juego
    /// </summary>
    public static bool GetLevelObjectiveComplete(GameModes gameMode, string world, int level)
    {
        return GetIntValue(GenerateLevelDataKey(gameMode, world, level, ObjectiveCompleteSuffix)) == 1;
    }

    /// <summary>
    /// Guarda todos los datos de un nivel de una vez
    /// </summary>
    public static void SetLevelData(GameModes gameMode, string world, int level, bool coinObtained, bool withoutDeath, bool objectiveComplete, bool withSave = false, bool saveCloud = false)
    {
        SetLevelCoinObtained(gameMode, world, level, coinObtained, false);
        SetLevelWithoutDeath(gameMode, world, level, withoutDeath, false);
        SetLevelObjectiveComplete(gameMode, world, level, objectiveComplete, withSave, saveCloud);
    }

    /// <summary>
    /// Obtiene todos los datos de un nivel específico
    /// </summary>
    public static LevelData GetLevelData(GameModes gameMode, string world, int level)
    {
        return new LevelData
        {
            coinObtained = GetLevelCoinObtained(gameMode, world, level),
            withoutDeath = GetLevelWithoutDeath(gameMode, world, level),
            objectiveComplete = GetLevelObjectiveComplete(gameMode, world, level)
        };
    }

    /// <summary>
    /// Verifica si existe algún dato guardado para un nivel específico
    /// </summary>
    public static bool HasLevelData(GameModes gameMode, string world, int level)
    {
        return ContainsKey(GenerateLevelDataKey(gameMode, world, level, CoinSuffix)) ||
            ContainsKey(GenerateLevelDataKey(gameMode, world, level, WithoutDeathSuffix)) ||
            ContainsKey(GenerateLevelDataKey(gameMode, world, level, ObjectiveCompleteSuffix));
    }

    public static int GetHighestLevelReachedByGameModeAndWorld(GameModes gameMode, string world, int maxLevels = 50)
    {
        int highestLevel = 0;

        for(int level = 1; level <= maxLevels; level++)
        {
            if(HasLevelData(gameMode, world,level))
            {
                highestLevel = level;
            }
        }
        return highestLevel;
    }
    #endregion

    #region Compatibility Methods - Para migrar del sistema viejo
    /// <summary>
    /// Genera el nombre de moneda por nivel como lo hacías antes
    /// DEPRECATED - Usar SetLevelCoinObtained en su lugar
    /// </summary>
    public static string GenerateCoinNameByLevel(string gameMode, int level)
    {
        return CoinNameByLevel + gameMode + level;
    }

    /// <summary>
    /// Migra datos del formato viejo al nuevo
    /// Llamar una sola vez para migrar datos existentes
    /// </summary>
    public static void MigrateLegacyData()
    {
        Debug.Log("Starting legacy data migration...");
        int migratedLevels = 0;

        // Migrar datos de Dunk (formato viejo)
        for (int level = 1; level <= 50; level++)
        {
            bool hasAnyData = false;

            // Migrar nivel completado
            string oldLevelKey = DunkLevelName + level;
            if (ContainsKey(oldLevelKey))
            {
                // En el formato viejo, si existe la key significa que se completó
                hasAnyData = true;
            }

            // Migrar sin muerte
            string oldWithoutDeathKey = DunkWithoutDeathName + level;
            bool withoutDeath = false;
            if (ContainsKey(oldWithoutDeathKey))
            {
                withoutDeath = GetIntValue(oldWithoutDeathKey) == 1;
                hasAnyData = true;
            }

            // Migrar toques completos (objetivo de Dunk)
            string oldTouchesKey = DunkTouchesCompleteName + level;
            bool touchesComplete = false;
            if (ContainsKey(oldTouchesKey))
            {
                touchesComplete = GetIntValue(oldTouchesKey) == 1;
                hasAnyData = true;
            }

            // Migrar moneda
            string oldCoinKey = GenerateCoinNameByLevel(GameModes.Dunk.ToString(), level);
            bool coinObtained = false;
            if (ContainsKey(oldCoinKey))
            {
                coinObtained = GetIntValue(oldCoinKey) == 1;
                hasAnyData = true;
            }

            // Si hay datos para este nivel, migrarlos al nuevo formato
            if (hasAnyData)
            {
                SetLevelData(GameModes.Dunk, "Neon", level, coinObtained, withoutDeath, touchesComplete);
                migratedLevels++;
                Debug.Log($"Migrated level {level} data to new format");
            }
        }

        if (migratedLevels > 0)
        {
            Save();
            Debug.Log($"Migration completed. {migratedLevels} levels migrated to new format.");
        }
        else
        {
            Debug.Log("No legacy data found to migrate.");
        }
    }
    #endregion

    #region Save/Load/Cloud Methods
    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static void SaveCloud()
    {
        //if (SaveAndLoadOnCloudManager.Instance != null && !isLoadingFromCloud)
        //    SaveAndLoadOnCloudManager.Instance.SaveGameData(SerializeGameData());
    }

    public static void ApplyCloudData(string cloudData, Action onComplete, Action onFail)
    {
        isLoadingFromCloud = true;

        try
        {
            GameData data = Newtonsoft.Json.JsonConvert.DeserializeObject<GameData>(cloudData);

            // Aplicar datos básicos
            SetIntValue(data.coins, CoinsName);
            if (!string.IsNullOrEmpty(data.currentBallSkin))
                SetStringValue(data.currentBallSkin, CurrentBallSkinName);

            SetFloatValue(data.soundsVolume, SoundsVolumeName);
            SetFloatValue(data.musicVolume, MusicVolumeName);

            if (!string.IsNullOrEmpty(data.language))
            {
                SetStringValue(data.language, LanguageName);
            }

            if (!string.IsNullOrEmpty(data.currentModeName))
                SetStringValue(data.currentModeName, CurrentModeName);

            if (!string.IsNullOrEmpty(data.currentWorldName))
                SetStringValue(data.currentWorldName, CurrentWorldName);

            SetIntValue(data.reviewSowed, ReviewSowedName);

            // Aplicar datos de niveles con nueva estructura
            foreach (var modeData in data.gameModeData)
            {
                if (Enum.TryParse(modeData.Key, out GameModes gameMode))
                {
                    foreach (var worldData in modeData.Value)
                    {
                        string world = worldData.Key;

                        foreach (var levelData in worldData.Value)
                        {
                            if (int.TryParse(levelData.Key, out int level))
                            {
                                LevelData levelInfo = levelData.Value;
                                SetLevelData(gameMode, world, level,
                                           levelInfo.coinObtained,
                                           levelInfo.withoutDeath,
                                           levelInfo.objectiveComplete);
                            }
                        }
                    }
                }
            }

            // Aplicar skins obtenidas
            foreach (var skinData in data.obtainedSkins)
                SetIntValue(skinData.Value ? 1 : 0, ObtainedBallSkins + skinData.Key);

            PlayerPrefs.Save();
            Debug.Log("Cloud data successfully applied to local save.");
        }
        catch (Exception e)
        {
            Debug.LogError("Error applying cloud data: " + e.Message);
            onFail?.Invoke();
        }
        finally
        {
            isLoadingFromCloud = false;
            onComplete?.Invoke();
            LanguageManager.Instance?.LoadSavedOrDetectLanguage();
        }
    }

    private static string[] _skinNames = new string[] {
        "BallBasicSkin","BallBasicRedSkin","BallBasicBlueSkin","BallBasicGreenSkin","BallBasicVioletSkin","BallBasicWhiteSkin","BallBasicBlackSkin",
        "CatBallSkin","DogBallSkin","CarpinchoBallSkin","PandaBallSkin","GrizzlyBallSkin",
        "FutbolBallSkin","BasketBallSkin","TennisBallSkin","BaseBallSkin",
        "MagmaBallSkin","WaterBallSkin",
        "NeonBallSkin"
    };

    private static string SerializeGameData()
    {
        GameData data = new();

        // Datos básicos
        data.coins = GetIntValue(CoinsName);
        data.currentBallSkin = GetStringValue(CurrentBallSkinName);
        data.soundsVolume = GetFloatValue(SoundsVolumeName);
        data.musicVolume = GetFloatValue(MusicVolumeName);
        data.language = GetStringValue(LanguageName);
        data.reviewSowed = GetIntValue(ReviewSowedName);
        data.currentModeName = GetStringValue(CurrentModeName);
        data.currentWorldName = GetStringValue(CurrentWorldName);

        // Serializar datos de niveles con nueva estructura
        foreach (GameModes mode in Enum.GetValues(typeof(GameModes)))
        {
            if (mode == GameModes.Null)
                continue;

            foreach (string world in _availableWorlds)
            {
                for (int level = 1; level <= 50; level++)
                {
                    if (HasLevelData(mode, world, level))
                    {
                        // Asegurar que existe la estructura anidada
                        if (!data.gameModeData.ContainsKey(mode.ToString()))
                            data.gameModeData[mode.ToString()] = new Dictionary<string, Dictionary<string, LevelData>>();

                        if (!data.gameModeData[mode.ToString()].ContainsKey(world))
                            data.gameModeData[mode.ToString()][world] = new Dictionary<string, LevelData>();

                        // Agregar los datos del nivel
                        data.gameModeData[mode.ToString()][world][level.ToString()] = GetLevelData(mode, world, level);
                    }
                }
            }
        }

        // Serializar skins
        foreach (string skinName in _skinNames)
        {
            string skinPrefKey = ObtainedBallSkins + skinName;
            if (PlayerPrefs.HasKey(skinPrefKey))
            {
                data.obtainedSkins[skinName] = GetIntValue(skinPrefKey) == 1;
            }
        }

        return Newtonsoft.Json.JsonConvert.SerializeObject(data);
    }

    public static void DeleteData()
    {
        PlayerPrefs.DeleteAll();

        //if (SaveAndLoadOnCloudManager.Instance != null)
        //    SaveAndLoadOnCloudManager.Instance.SaveGameData("{}");
    }
    #endregion

    #region Utility Methods
    /// <summary>
    /// Actualiza la lista de mundos disponibles
    /// Llamar cuando agregues nuevos mundos al juego
    /// </summary>
    public static void SetAvailableWorlds(string[] worlds)
    {
        _availableWorlds = worlds;
    }

    /// <summary>
    /// Obtiene los mundos disponibles
    /// </summary>
    public static string[] GetAvailableWorlds()
    {
        return _availableWorlds;
    }
    #endregion
}

// Clases para la serialización
[Serializable]
public class GameData
{
    public int coins;
    public string currentBallSkin;
    public float soundsVolume;
    public float musicVolume;
    public string language;
    public int reviewSowed;
    public string currentWorldName;
    public string currentModeName;

    // Estructura: GameMode -> World -> Level -> LevelData
    public Dictionary<string, Dictionary<string, Dictionary<string, LevelData>>> gameModeData;
    public Dictionary<string, bool> obtainedSkins;

    public GameData()
    {
        gameModeData = new Dictionary<string, Dictionary<string, Dictionary<string, LevelData>>>();
        obtainedSkins = new Dictionary<string, bool>();
    }
}

[Serializable]
public class LevelData
{
    public bool coinObtained;
    public bool withoutDeath;
    public bool objectiveComplete; // Dunk: toques máximos, Time: tiempo límite, etc.
}