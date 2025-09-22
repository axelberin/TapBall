using UnityEngine;
using System.Collections;
using Firebase.Firestore;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Auth;

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
#else
        _userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
#endif
    }

    public void SaveGameData()
    {
        DocumentReference docRef = _dataBase.Document($"PlayersData/{_userId}");

        Dictionary<string, object> playerData = new Dictionary<string, object>
        {
            { "name", FirebaseAuth.DefaultInstance.CurrentUser.DisplayName},
            { "lastUpdate", Timestamp.GetCurrentTimestamp() }
        };

        docRef.SetAsync(playerData).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
                Debug.Log("Datos guardados en Firestore");
            else
                Debug.LogError("Error al guardar: " + task.Exception);
        });
    }

    public void LoadGameData(string userId)
    {
        DocumentReference docRef = _dataBase.Document($"PlayersData/{userId}");
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error al obtener datos: " + task.Exception);
                OnLoadDataFailed(); // El OK del popup pone _isInitialized = true
                return;
            }

            var snapshot = task.Result;
            if (!snapshot.Exists)
            {
                // Primera vez: crear documento con defaults
                var defaults = new Dictionary<string, object>
            {
                { "name", FirebaseAuth.DefaultInstance.CurrentUser.DisplayName },
                { "level", 1 },
                { "coins", 0 },
                { "lastUpdate", Timestamp.GetCurrentTimestamp() }
            };

                docRef.SetAsync(defaults, SetOptions.MergeAll).ContinueWithOnMainThread(_ =>
                {
                    Debug.Log("Documento creado con valores por defecto.");
                    _isInitialized = true; // ahora sí seguimos
                });
                return;
            }

            int level = snapshot.ContainsField("level") ? snapshot.GetValue<int>("level") : 1;
            int coins = snapshot.ContainsField("coins") ? snapshot.GetValue<int>("coins") : 0;

            Debug.Log($"Jugador: Nivel {level}, Monedas {coins}");
            _isInitialized = true; 
        });
    }

    private void OnLoadDataFailed()
    {
        LoadingGameManager.Instance.ShowCantSignInPopUp("conectionfail", "cantloadcloud", () => _isInitialized = true, Application.Quit);
    }

    public override IEnumerator InizializeManagers()
    {
        LoadGameData(_userId);

        // Esperar hasta que se complete la carga
        while (!_isInitialized)
            yield return null;
    }
}