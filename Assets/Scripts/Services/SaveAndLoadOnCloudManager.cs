using UnityEngine;
using System.Collections;
using Firebase.Firestore;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Auth;
using System;

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
        _dataBase = FirebaseFirestore.DefaultInstance;
#if UNITY_EDITOR
        _userId = "EditorTestUser";
        _isInitialized = true;
#endif
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

        Dictionary<string, object> playerData = new Dictionary<string, object>
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
                Debug.LogError("Error al guardar datos de juego en Firestore: " + task.Exception);
        });
    }

    public void LoadGameData(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("userId vacío no se puede leer Firestore");
            OnLoadDataFailed();
            return;
        }

        DocumentReference docRef;
        try
        {
            docRef = _dataBase.Document($"PlayersData/{userId}");
        }
        catch (Exception e)
        {
            Debug.LogError("Ruta inválida a Firestore: " + e);
            OnLoadDataFailed();
            return;
        }

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error al obtener datos: " + task.Exception);
                OnLoadDataFailed();
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
                OnLoadDataFailed();
            }
        });
    }

    private void CreateDefaultPlayerDocument(DocumentReference docRef)
    {
        // Crear documento con datos por defecto basados en el sistema local
        var defaultGameData = SerializeLocalGameData();

        var defaults = new Dictionary<string, object>
        {
            { "name", FirebaseAuth.DefaultInstance.CurrentUser?.DisplayName ?? "New Player" },
            { "lastUpdate", Timestamp.GetCurrentTimestamp() },
            { "gameData", defaultGameData }
        };

        docRef.SetAsync(defaults, SetOptions.MergeAll).ContinueWithOnMainThread(setTask =>
        {
            if (setTask.IsFaulted || setTask.IsCanceled)
            {
                Debug.LogError("No se pudo crear documento inicial: " + setTask.Exception);
                OnLoadDataFailed();
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
            // Datos básicos del juego
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
            // Aplicar datos básicos
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

    private void OnLoadDataFailed()
    {
        if (LoadingGameManager.Instance)
            LoadingGameManager.Instance.ShowCantSignInPopUp(
                "conectionfail", "cantloadcloud", () => _isInitialized = true, Application.Quit);
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
            OnLoadDataFailed();      // el OK del pop-up pone _isInitialized = true
            yield break;
        }

        _userId = auth.CurrentUser.UserId; // ahora sí existe
        LoadGameData(_userId);
        while (!_isInitialized)
            yield return null;
    }
}