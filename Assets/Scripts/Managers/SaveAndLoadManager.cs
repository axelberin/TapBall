using UnityEngine;
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
    public static string IsPlayingFirstTimeName = "IsPlayingFirstTimeName";
    public static string LastDayUpdateName = "LastDayUpdate";

    // Nueva estructura: Modo_Mundo_Nivel_TipoDato
    private static string LevelDataPrefix = "LevelData_";
    private static string CompletedSuffix = "_Completed";
    private static string CoinSuffix = "_Coin";
    private static string WithoutDeathSuffix = "_WithoutDeath";
    private static string ObjectiveCompleteSuffix = "_ObjectiveComplete";

    // Misiones diarias
    private static string MissionPrefix = "Mission_";
    private static string ProgressSuffix = "_Progress";
    private static string DateSuffix = "_Date";

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
            //Debug.LogWarning($"Doesn´t exist '{parameterName}'");
            return default;
        }
        return PlayerPrefs.GetInt(parameterName);
    }

    public static float GetFloatValue(string parameterName)
    {
        if (!ContainsKey(parameterName))
        {
            //Debug.LogWarning($"Doesn´t exist '{parameterName}'");
            return default;
        }
        return PlayerPrefs.GetFloat(parameterName);
    }

    public static string GetStringValue(string parameterName)
    {
        if (!ContainsKey(parameterName))
        {
            //Debug.LogWarning($"Doesn´t exist '{parameterName}'");
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
    /// Esto se debe marcar siempre que el jugador termine un nivel, sin importar objetivos secundarios
    /// </summary>
    public static void SetLevelCompleted(GameModes gameMode, string world, int level, bool completed, bool withSave = false, bool saveCloud = false)
    {
        SetIntValue(completed ? 1 : 0, GenerateLevelDataKey(gameMode, world, level, CompletedSuffix), withSave, saveCloud);
    }

    /// <summary>
    /// Obtiene si el jugador completó el nivel (independiente de objetivos)
    /// </summary>
    public static bool GetLevelCompleted(GameModes gameMode, string world, int level)
    {
        return GetIntValue(GenerateLevelDataKey(gameMode, world, level, CompletedSuffix)) == 1;
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
    /// Guarda todos los datos de un nivel de una vez, incluyendo completion status
    /// </summary>
    public static void SetLevelData(GameModes gameMode, string world, int level, bool levelCompleted, bool coinObtained, bool withoutDeath, bool objectiveComplete, bool withSave = false, bool saveCloud = false)
    {
        SetLevelCompleted(gameMode, world, level, levelCompleted, false);
        SetLevelCoinObtained(gameMode, world, level, coinObtained, false);
        SetLevelWithoutDeath(gameMode, world, level, withoutDeath, false);
        SetLevelObjectiveComplete(gameMode, world, level, objectiveComplete, withSave, saveCloud);
    }

    /// <summary>
    /// Método de conveniencia para cuando el jugador simplemente completa un nivel
    /// </summary>
    public static void CompleteLevel(GameModes gameMode, string world, int level, bool coinObtained, bool withoutDeath, bool objectiveComplete, bool withSave = false, bool saveCloud = false)
    {
        SetLevelData(gameMode, world, level, true, coinObtained, withoutDeath, objectiveComplete, withSave, saveCloud);
    }

    /// <summary>
    /// Obtiene todos los datos de un nivel específico
    /// </summary>
    public static LevelData GetLevelData(GameModes gameMode, string world, int level)
    {
        return new LevelData
        {
            levelCompleted = GetLevelCompleted(gameMode, world, level),
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
        return ContainsKey(GenerateLevelDataKey(gameMode, world, level, CompletedSuffix)) ||
            ContainsKey(GenerateLevelDataKey(gameMode, world, level, CoinSuffix)) ||
            ContainsKey(GenerateLevelDataKey(gameMode, world, level, WithoutDeathSuffix)) ||
            ContainsKey(GenerateLevelDataKey(gameMode, world, level, ObjectiveCompleteSuffix));
    }

    /// <summary>
    /// Obtiene el nivel más alto completado (no solo con datos)
    /// </summary>
    public static int GetHighestLevelCompleted(GameModes gameMode, string world, int maxLevels = 50)
    {
        int highestLevel = 0;

        for (int level = 1; level <= maxLevels; level++)
        {
            if (GetLevelCompleted(gameMode, world, level))
            {
                highestLevel = level;
            }
        }
        return highestLevel;
    }

    /// <summary>
    /// Obtiene el próximo nivel disponible para jugar
    /// </summary>
    public static int GetNextAvailableLevel(GameModes gameMode, string world, int maxLevels = 50)
    {
        int highestCompleted = GetHighestLevelCompleted(gameMode, world, maxLevels);
        int nextLevel = highestCompleted + 1;

        // Si el siguiente nivel está dentro del rango, devolverlo
        if (nextLevel <= maxLevels)
            return nextLevel;

        // Si ya completó todos los niveles, devolver el último
        return maxLevels;
    }

    public static int GetHighestLevelReachedByGameModeAndWorld(GameModes gameMode, string world, int maxLevels = 50)
    {
        int highestLevel = 0;

        for (int level = 1; level <= maxLevels; level++)
        {
            if (HasLevelData(gameMode, world, level))
            {
                highestLevel = level;
            }
        }
        return highestLevel;
    }
    #endregion

    #region Missions Data region
    public static void SetDailyMissionProgressByMissionID(string missionID, float progress,string day, bool withSave = false, bool cloudSave = false)
    {
        SetFloatValue(progress, GetMissionProgressKey(missionID), withSave, cloudSave);
        SetStringValue(day, GetMissionDateKey(missionID), withSave, cloudSave);
    }

    public static MissionProgressData GetDailyMissionProgressDataByID(string missionID)
    {
        float progress = GetFloatValue(GetMissionProgressKey(missionID));
        string date = GetStringValue(GetMissionDateKey(missionID));

        return new MissionProgressData
        {
            missionID = missionID,
            progress = progress,
            lastUpdateDate = date
        };
    }

    public static MissionData[] GetDailyMissions()
    {
        return DailyMissionsManager.Instance.GetTodayMissions.ToArray();
    }

    public static void DeleteMissionDataByID(string missionID, bool withSave = false, bool cloudSave = false)
    {
        DeleteKey(GetMissionProgressKey(missionID), withSave, cloudSave);
        DeleteKey(GetMissionDateKey(missionID), withSave, cloudSave);
    }

    private static string GetMissionProgressKey(string missionID) => MissionPrefix + missionID + ProgressSuffix;
    private static string GetMissionDateKey(string missionID) => MissionPrefix + missionID + DateSuffix;
    #endregion

    #region Save/Load/Cloud Methods
    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static void SaveCloud()
    {
        if (isLoadingFromCloud)
        {
            Debug.Log("Evitando guardado en nube durante carga desde nube");
            return;
        }

#if !UNITY_EDITOR
        if (SaveAndLoadOnCloudManager.Instance != null)
            SaveAndLoadOnCloudManager.Instance.SaveGameData();
        else
            Debug.LogWarning("SaveAndLoadOnCloudManager.Instance es null, no se puede guardar en la nube");
#else
        Debug.Log("Guardado en nube omitido en editor");
#endif
    }

    public static void DeleteKey(string parameterName, bool withSave = false, bool saveCloud = false)
    {
        if (!ContainsKey(parameterName))
            return;

        PlayerPrefs.DeleteKey(parameterName);
        if (withSave) 
            Save();
        if (saveCloud)
            SaveCloud();
    }

    public static void DeleteData()
    {
        PlayerPrefs.DeleteAll();
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
public class LevelData
{
    public bool levelCompleted; // Si el nivel fue completado básicamente
    public bool coinObtained;   // Objetivo secundario: moneda
    public bool withoutDeath;   // Objetivo secundario: sin morir
    public bool objectiveComplete; // Objetivo secundario: objetivo específico del modo (Dunk: toques máximos, Time: tiempo límite, etc.)
}

[Serializable]
public class MissionProgressData
{
    public string missionID;
    public float progress;
    public string lastUpdateDate;
}