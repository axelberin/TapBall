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
        _isInitialized = true;
        DocumentReference docRef = _dataBase.Document($"PlayersData/{userId}");
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error al obtener datos: " + task.Exception);
                OnLoadDataFailed();
                return;
            }

            if (task.IsCanceled)
            {
                Debug.LogWarning("La solicitud fue cancelada.");
                OnLoadDataFailed();
                return;
            }

            if (task.Result.Exists)
            {
                var snapshot = task.Result;
                int level = snapshot.GetValue<int>("level");
                int coins = snapshot.GetValue<int>("coins");

                Debug.Log($"Jugador: Nivel {level}, Monedas {coins}");
                _isInitialized = true;
            }
            else
            {
                Debug.Log("No existe el usuario.");
                OnLoadDataFailed();
            }
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