using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveAndLoadOnCloudManager : ManagersManager
{
    public static SaveAndLoadOnCloudManager Instance;

    private FirebaseFirestore _dataBase;
    private string _userId;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    protected override void Start()
    {
        base.Start();
#if UNITY_EDITOR
        _userId = "EditorTestUser";
        _isInitialized = true;
#endif
    }

    private bool EnsureFirestoreReady()
    {
        _dataBase ??= FirebaseFirestore.DefaultInstance;
        return _dataBase != null;
    }

    public void SaveGameData()
    {
        if (string.IsNullOrEmpty(_userId))
        {
            Debug.LogWarning("No hay userId disponible para guardar en la nube");
            return;
        }

        DocumentReference docRef = _dataBase.Document($"PlayersData/{_userId}");

        // Obtener todos los datos del sistema local
        var gameData = SerializeLocalGameData();

        Dictionary<string, object> playerData = new()
        {
            { "name", FirebaseAuth.DefaultInstance.CurrentUser?.DisplayName ?? "Unknown Player"},
            { "lastUpdate", Timestamp.GetCurrentTimestamp() },
            { "gameData", gameData }
        };

        docRef.SetAsync(playerData).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
                Debug.Log("Datos de juego guardados en Firestore exitosamente");
            else
                Debug.LogError("Error al guardar datos de juego en Firestore: " + task.Exception?.Message);
        });
    }

    public void LoadGameData(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("userId vac�o no se puede leer Firestore");
            OnLoadDataFailed("UserIDNull", "User ID is Null on load");
            return;
        }

        if (!EnsureFirestoreReady())
        {
            Debug.LogError("Firestore DefaultInstance a�n no est� listo");
            OnLoadDataFailed("FirestoreNull", "DefaultInstance is null");
            return;
        }

        DocumentReference docRef;
        try
        {
            docRef = _dataBase.Document($"PlayersData/{userId}");
        }
        catch (Exception e)
        {
            Debug.LogError("Ruta inv�lida a Firestore: " + e.Message);
            OnLoadDataFailed("InvalidPath", "Firebase Path not found",
                ("exep", e?.Message));
            return;
        }

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error al obtener datos: " + task.Exception.Message);
                OnLoadDataFailed("GetSnapshotAsyncFail", "GetSnapshotAsyncFail task Fail",
                ("exception", task.Exception?.Message),
                ("inner", task.Exception?.InnerException?.Message));
                return;
            }

            var snapshot = task.Result;
            if (!snapshot.Exists)
            {
                // Crear documento con datos por defecto
                CreateDefaultPlayerDocument(docRef);
                return;
            }

            try
            {
                // Cargar datos del juego desde la nube
                if (snapshot.ContainsField("gameData"))
                {
                    var gameDataDict = snapshot.GetValue<Dictionary<string, object>>("gameData");
                    ApplyCloudDataToLocal(gameDataDict);
                    Debug.Log("Datos de juego cargados desde la nube exitosamente");
                }
                else
                {
                    Debug.Log("No se encontraron datos de juego en la nube, usando datos locales");
                }

                _isInitialized = true;
            }
            catch (Exception e)
            {
                Debug.LogError("Error al procesar datos de la nube: " + e.Message);
                OnLoadDataFailed("LoadSnapshotFail", "Fail on Load Snapshot",
                ("exception", e?.Message));
            }
        });
    }

    private void CreateDefaultPlayerDocument(DocumentReference docRef)
    {
        // Crear documento con datos por defecto basados en el sistema local
        var defaultGameData = SerializeLocalGameData();

        var defaults = new Dictionary<string, object>
        {
            { "gameData", defaultGameData },
            { "lastUpdate", Timestamp.GetCurrentTimestamp() },
            { "name", FirebaseAuth.DefaultInstance.CurrentUser?.DisplayName ?? "New Player" }
        };

        docRef.SetAsync(defaults, SetOptions.MergeAll).ContinueWithOnMainThread(setTask =>
        {
            if (setTask.IsFaulted || setTask.IsCanceled)
            {
                Debug.LogError("No se pudo crear documento inicial: " + setTask.Exception);
                OnLoadDataFailed("ContinueWithOnMainThreadFail", "Fail creating document",
                ("exception", setTask.Exception?.Message),
                ("inner", setTask.Exception?.InnerException?.Message));
                return;
            }
            Debug.Log("Documento creado con valores por defecto.");
            _isInitialized = true;
        });
    }

    private Dictionary<string, object> SerializeLocalGameData()
    {
        var gameData = new Dictionary<string, object>();

        try
        {
            // Datos b�sicos del juego
            gameData["coins"] = SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CoinsName) ?
                               SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CoinsName) : 0;

            gameData["orbs"] = SaveAndLoadManager.ContainsKey(SaveAndLoadManager.OrbsName) ?
                              SaveAndLoadManager.GetIntValue(SaveAndLoadManager.OrbsName) : 0;

            gameData["currentBallSkin"] = SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CurrentBallSkinName) ?
                                         SaveAndLoadManager.GetStringValue(SaveAndLoadManager.CurrentBallSkinName) : "";

            gameData["soundsVolume"] = SaveAndLoadManager.ContainsKey(SaveAndLoadManager.SoundsVolumeName) ?
                                      SaveAndLoadManager.GetFloatValue(SaveAndLoadManager.SoundsVolumeName) : 1f;

            gameData["musicVolume"] = SaveAndLoadManager.ContainsKey(SaveAndLoadManager.MusicVolumeName) ?
                                     SaveAndLoadManager.GetFloatValue(SaveAndLoadManager.MusicVolumeName) : 1f;

            gameData["language"] = SaveAndLoadManager.ContainsKey(SaveAndLoadManager.LanguageName) ?
                                  SaveAndLoadManager.GetStringValue(SaveAndLoadManager.LanguageName) : "";

            gameData["reviewSowed"] = SaveAndLoadManager.ContainsKey(SaveAndLoadManager.ReviewSowedName) ?
                                     SaveAndLoadManager.GetIntValue(SaveAndLoadManager.ReviewSowedName) : 0;

            gameData["noAdsBought"] = SaveAndLoadManager.ContainsKey(SaveAndLoadManager.NoAdsBougthName) ?
                                     SaveAndLoadManager.GetIntValue(SaveAndLoadManager.NoAdsBougthName) : 0;

            gameData["currentWorld"] = SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CurrentWorldName) ?
                                      SaveAndLoadManager.GetStringValue(SaveAndLoadManager.CurrentWorldName) : "";

            gameData["currentMode"] = SaveAndLoadManager.ContainsKey(SaveAndLoadManager.CurrentModeName) ?
                                     SaveAndLoadManager.GetIntValue(SaveAndLoadManager.CurrentModeName) : 1;

            gameData["isFirstTimePlaying"] = SaveAndLoadManager.ContainsKey(SaveAndLoadManager.IsPlayingFirstTimeName) ?
                                            SaveAndLoadManager.GetIntValue(SaveAndLoadManager.IsPlayingFirstTimeName) : 0;

            gameData["LastDayUpdate"] = SaveAndLoadManager.ContainsKey(SaveAndLoadManager.LastDayUpdateName) ?
                                        SaveAndLoadManager.GetStringValue(SaveAndLoadManager.LastDayUpdateName) : "";

            // Serializar datos de niveles
            var levelDataDict = new Dictionary<string, object>();
            var availableWorlds = SaveAndLoadManager.GetAvailableWorlds();

            foreach (GameManager.GameModes mode in Enum.GetValues(typeof(GameManager.GameModes)))
            {
                if (mode == GameManager.GameModes.Null) continue;

                foreach (string world in availableWorlds)
                {
                    for (int level = 1; level <= 50; level++)
                    {
                        if (SaveAndLoadManager.HasLevelData(mode, world, level))
                        {
                            var levelData = SaveAndLoadManager.GetLevelData(mode, world, level);
                            string levelKey = $"{mode}_{world}_{level}";

                            levelDataDict[levelKey] = new Dictionary<string, object>
                            {
                                ["levelCompleted"] = levelData.levelCompleted,
                                ["coinObtained"] = levelData.coinObtained,
                                ["withoutDeath"] = levelData.withoutDeath,
                                ["objectiveComplete"] = levelData.objectiveComplete
                            };
                        }
                    }
                }
            }
            gameData["levelData"] = levelDataDict;

            // Serializar skins obtenidas
            var skinsDict = new Dictionary<string, object>();
            var skinNames = new string[] {
                "BallBasicSkin", "BallBasicRedSkin", "BallBasicBlueSkin", "BallBasicGreenSkin",
                "BallBasicVioletSkin", "BallBasicWhiteSkin", "BallBasicBlackSkin",
                "CatBallSkin", "DogBallSkin", "CarpinchoBallSkin", "PandaBallSkin", "GrizzlyBallSkin",
                "FutbolBallSkin", "BasketBallSkin", "TennisBallSkin", "BaseBallSkin",
                "MagmaBallSkin", "WaterBallSkin", "NeonBallSkin"
            };

            foreach (string skinName in skinNames)
            {
                string skinKey = SaveAndLoadManager.ObtainedBallSkins + skinName;
                if (SaveAndLoadManager.ContainsKey(skinKey))
                {
                    skinsDict[skinName] = SaveAndLoadManager.GetIntValue(skinKey) == 1;
                }
            }
            gameData["obtainedSkins"] = skinsDict;

            //Serializar datos de misiones diarias
            var missionsList = new List<Dictionary<string, object>>();
            var availableMissions = SaveAndLoadManager.GetDailyMissions();

            foreach (var mission in availableMissions)
            {
                var missionData = new Dictionary<string, object>
                {
                    { "missionID", mission.missionID },
                    { "currentProgress", mission.currentProgress },
                    { "date", DateTime.Today.ToString("yyyyMMdd") }
                };
                missionsList.Add(missionData);
            }
            gameData["DailyMissionsData"] = missionsList;

            //Serializar los powerups
            var powerupDict = new Dictionary<string, object>();
            var powerupNames = new string[]
            {
           $"{SaveAndLoadManager.PowerUpPrefix + PowerUpManager.PowerUpType.TimeStopPowerUp}",
           $"{SaveAndLoadManager.PowerUpPrefix + PowerUpManager.PowerUpType.StopTouchCounterPowerUp}",
           $"{SaveAndLoadManager.PowerUpPrefix + PowerUpManager.PowerUpType.ImmunityPowerUp}",
           $"{SaveAndLoadManager.PowerUpPrefix + PowerUpManager.PowerUpType.RevivePowerUp}"
            };

            foreach (string powerUpName in powerupNames)
            {
                string powerUpKey = powerUpName;
                if (SaveAndLoadManager.ContainsKey(powerUpKey))
                {
                    powerupDict[powerUpName] = SaveAndLoadManager.GetIntValue(powerUpKey);
                }
            }

            gameData["obtainedPowerUps"] = powerupDict;

            Debug.Log("Datos locales serializados exitosamente para la nube");
        }
        catch (Exception e)
        {
            Debug.LogError("Error serializando datos locales: " + e.Message);
        }

        return gameData;
    }

    private void ApplyCloudDataToLocal(Dictionary<string, object> cloudGameData)
    {
        try
        {
            // Aplicar datos b�sicos
            if (cloudGameData.ContainsKey("coins"))
                SaveAndLoadManager.SetIntValue(Convert.ToInt32(cloudGameData["coins"]), SaveAndLoadManager.CoinsName);

            if (cloudGameData.ContainsKey("orbs"))
                SaveAndLoadManager.SetIntValue(Convert.ToInt32(cloudGameData["orbs"]), SaveAndLoadManager.OrbsName);

            if (cloudGameData.ContainsKey("currentBallSkin"))
                SaveAndLoadManager.SetStringValue(cloudGameData["currentBallSkin"].ToString(), SaveAndLoadManager.CurrentBallSkinName);

            if (cloudGameData.ContainsKey("soundsVolume"))
                SaveAndLoadManager.SetFloatValue(Convert.ToSingle(cloudGameData["soundsVolume"]), SaveAndLoadManager.SoundsVolumeName);

            if (cloudGameData.ContainsKey("musicVolume"))
                SaveAndLoadManager.SetFloatValue(Convert.ToSingle(cloudGameData["musicVolume"]), SaveAndLoadManager.MusicVolumeName);

            if (cloudGameData.ContainsKey("language"))
                SaveAndLoadManager.SetStringValue(cloudGameData["language"].ToString(), SaveAndLoadManager.LanguageName);

            if (cloudGameData.ContainsKey("reviewSowed"))
                SaveAndLoadManager.SetIntValue(Convert.ToInt32(cloudGameData["reviewSowed"]), SaveAndLoadManager.ReviewSowedName);

            if (cloudGameData.ContainsKey("noAdsBought"))
                SaveAndLoadManager.SetIntValue(Convert.ToInt32(cloudGameData["noAdsBought"]), SaveAndLoadManager.NoAdsBougthName);

            if (cloudGameData.ContainsKey("currentWorld"))
                SaveAndLoadManager.SetStringValue(cloudGameData["currentWorld"].ToString(), SaveAndLoadManager.CurrentWorldName);

            if (cloudGameData.ContainsKey("currentMode"))
                SaveAndLoadManager.SetIntValue(Convert.ToInt32(cloudGameData["currentMode"]), SaveAndLoadManager.CurrentModeName);

            if (cloudGameData.ContainsKey("isFirstTimePlaying"))
                SaveAndLoadManager.SetIntValue(Convert.ToInt32(cloudGameData["isFirstTimePlaying"]), SaveAndLoadManager.IsPlayingFirstTimeName);

            if (cloudGameData.ContainsKey("LastDayUpdate"))
                SaveAndLoadManager.SetStringValue(cloudGameData["LastDayUpdate"].ToString(), SaveAndLoadManager.LastDayUpdateName);

            // Aplicar datos de niveles
            if (cloudGameData.ContainsKey("levelData"))
            {
                var levelDataDict = cloudGameData["levelData"] as Dictionary<string, object>;
                if (levelDataDict != null)
                {
                    foreach (var levelEntry in levelDataDict)
                    {
                        string[] keyParts = levelEntry.Key.Split('_');
                        if (keyParts.Length >= 3)
                        {
                            if (Enum.TryParse(keyParts[0], out GameManager.GameModes gameMode) &&
                                int.TryParse(keyParts[2], out int level))
                            {
                                string world = keyParts[1];
                                var levelDataObj = levelEntry.Value as Dictionary<string, object>;

                                if (levelDataObj != null)
                                {
                                    bool levelCompleted = levelDataObj.ContainsKey("levelCompleted") ?
                                                        Convert.ToBoolean(levelDataObj["levelCompleted"]) :
                                                        (levelDataObj.ContainsKey("coinObtained") ||
                                                         levelDataObj.ContainsKey("withoutDeath") ||
                                                         levelDataObj.ContainsKey("objectiveComplete"));

                                    bool coinObtained = levelDataObj.ContainsKey("coinObtained") &&
                                                       Convert.ToBoolean(levelDataObj["coinObtained"]);
                                    bool withoutDeath = levelDataObj.ContainsKey("withoutDeath") &&
                                                       Convert.ToBoolean(levelDataObj["withoutDeath"]);
                                    bool objectiveComplete = levelDataObj.ContainsKey("objectiveComplete") &&
                                                            Convert.ToBoolean(levelDataObj["objectiveComplete"]);

                                    SaveAndLoadManager.SetLevelData(gameMode, world, level,
                                                                   levelCompleted,
                                                                   coinObtained, withoutDeath, objectiveComplete);
                                }
                            }
                        }
                    }
                }
            }

            // Aplicar skins obtenidas
            if (cloudGameData.ContainsKey("obtainedSkins"))
            {
                var skinsDict = cloudGameData["obtainedSkins"] as Dictionary<string, object>;
                if (skinsDict != null)
                {
                    foreach (var skinEntry in skinsDict)
                    {
                        string skinKey = SaveAndLoadManager.ObtainedBallSkins + skinEntry.Key;
                        bool isObtained = Convert.ToBoolean(skinEntry.Value);
                        SaveAndLoadManager.SetIntValue(isObtained ? 1 : 0, skinKey);
                    }
                }
            }

            //Aplicar datos de misi�n obtenidos
            if (cloudGameData.ContainsKey("DailyMissionsData"))
            {
                var missionsList = cloudGameData["DailyMissionsData"] as List<Dictionary<string, object>>;
                if (missionsList != null)
                {
                    foreach (var missionObj in missionsList)
                    {
                        var missionData = missionObj as Dictionary<string, object>;
                        if (missionData != null &&
                            missionData.ContainsKey("missionID") &&
                            missionData.ContainsKey("currentProgress") &&
                            missionData.ContainsKey("date"))
                        {
                            string missionID = missionData["missionID"].ToString();
                            int progress = Convert.ToInt32(missionData["currentProgress"]);
                            string date = missionData["date"].ToString();

                            SaveAndLoadManager.SetDailyMissionProgressByMissionID(missionID, progress, date);
                        }
                    }
                }
            }

            //Aplicar datos de powerup obtenidos
            if (cloudGameData.ContainsKey("obtainedPowerUps"))
            {
                var powerUpDict = cloudGameData["obtainedPowerUps"] as Dictionary<string, object>;
                if (powerUpDict != null)
                {
                    foreach (var powerUp in powerUpDict)
                        SaveAndLoadManager.SetIntValue(Convert.ToInt32(powerUp.Value), powerUp.Key);
                }
            }

            // Guardar todos los cambios localmente
            SaveAndLoadManager.Save();
            Debug.Log("Datos de la nube aplicados exitosamente a los datos locales");
        }
        catch (Exception e)
        {
            Debug.LogError("Error aplicando datos de la nube: " + e.Message);
            throw;
        }
    }

    private void OnLoadDataFailed(string tag, string msg, params (string key, object val)[] keys)
{
    GameLog.NonFatal(tag, msg, keys);
    GameLog.LogEvent("cloud_load_failed", ("tag", tag), ("message", msg));

    Debug.LogWarning($"Cloud load failed, using local data. Tag: {tag}, Msg: {msg}");

    // Seguir con datos locales sin mostrar error
    _isInitialized = true;
}


    public override IEnumerator InizializeManagers()
    {
        var auth = FirebaseAuth.DefaultInstance;

        // Esperar a que haya usuario (o vencer por timeout)
        float deadline = Time.realtimeSinceStartup + 15f;
        while (auth.CurrentUser == null && Time.realtimeSinceStartup < deadline)
            yield return null;

        if (auth.CurrentUser == null)
        {
            Debug.LogWarning("Auth no listo en tiempo: continuando sin nube");
            _isInitialized = true;
            //OnLoadDataFailed("AuthNull", "auth.CurrentUser = Null");      // el OK del pop-up pone _isInitialized = true
            yield break;
        }

        _userId = auth.CurrentUser.UserId; // ahora s� existe
        LoadGameData(_userId);

        const float LOAD_TIMEOUT = 12f;
        float elapsed = 0f;
        while (!_isInitialized && elapsed < LOAD_TIMEOUT)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!_isInitialized)
        {
            Debug.LogWarning("Timeout cargando datos en la nube, mostrando pop-up/fallback.");
            OnLoadDataFailed("CloudLoadTimeout", "Firestore load timed out");
            while (!_isInitialized)
                yield return null;
        }
    }
}