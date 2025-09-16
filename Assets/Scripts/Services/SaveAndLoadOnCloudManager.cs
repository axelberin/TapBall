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
        _dataBase = FirebaseFirestore.DefaultInstance;
        _userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        base.Start();
    }

    public void SaveGameData()
    {
        DocumentReference docRef = _dataBase.Collection("players").Document(_userId);

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
        DocumentReference docRef = _dataBase.Collection("players").Document(userId);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result.Exists)
            {
                var snapshot = task.Result;
                int level = snapshot.GetValue<int>("level");
                int coins = snapshot.GetValue<int>("coins");

                Debug.Log($"Jugador: Nivel {level}, Monedas {coins}");
            }
            else
            {
                Debug.Log("No existe el usuario.");
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