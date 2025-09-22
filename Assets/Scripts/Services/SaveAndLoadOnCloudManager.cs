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
        _isInitialized = true;
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
        catch (System.Exception e)
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
                var defaults = new Dictionary<string, object> {
                { "name", FirebaseAuth.DefaultInstance.CurrentUser.DisplayName },
                { "level", 1 }, { "coins", 0 },
                { "lastUpdate", Timestamp.GetCurrentTimestamp() }
                };

                docRef.SetAsync(defaults, SetOptions.MergeAll).ContinueWithOnMainThread(setTask =>
                {
                    if (setTask.IsFaulted || setTask.IsCanceled)
                    {
                        Debug.LogError("No se pudo crear doc inicial: " + setTask.Exception);
                        OnLoadDataFailed();
                        return;
                    }
                    Debug.Log("Documento creado con valores por defecto.");
                    _isInitialized = true;
                });
                return;
            }

            int level = snapshot.ContainsField("level") ? snapshot.GetValue<int>("level") : 1;
            int coins = snapshot.ContainsField("coins") ? snapshot.GetValue<int>("coins") : 0;

            _isInitialized = true;
        });
    }


    private void OnLoadDataFailed()
    {
        LoadingGameManager.Instance.ShowCantSignInPopUp("conectionfail", "cantloadcloud", () => _isInitialized = true, Application.Quit);
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