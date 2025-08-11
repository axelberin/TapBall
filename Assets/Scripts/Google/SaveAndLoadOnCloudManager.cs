using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames.BasicApi;
using GooglePlayGames;
using UnityEngine;
using System.Collections;

public class SaveAndLoadOnCloudManager : ManagersManager
{
    private void Awake()
    {
        SaveAndLoadManager.SetCloudManager(this);
    }

    public void SaveGameData(string dataToSave)
    {
        if (PlayGamesPlatform.Instance.IsAuthenticated())
        {
            StartCoroutine(SaveGameDataCoroutine(dataToSave));
        }
        else
        {
            MenuManagerCanvas.Instance.DebugIsAuthenticated("Not authenticated");
            Debug.LogWarning("Not authenticated with Google Play Games. Cannot save data.");
        }
    }

    private IEnumerator SaveGameDataCoroutine(string dataToSave)
    {
        bool operationComplete = false;

        PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
            "MultiverseTapBallProgress",
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseMostRecentlySaved,
            (status, game) => OnSavedGameOpened(status, game, dataToSave, () => operationComplete = true));

        // Esperar a que se complete la operación
        while (!operationComplete)
        {
            yield return null;
        }
    }

    private void OnSavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata game, string dataToSave, System.Action onComplete)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataToSave);

            SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder()
                .WithUpdatedDescription("Saved Game at " + System.DateTime.Now.ToShortTimeString())
                .WithUpdatedPlayedTime(System.TimeSpan.FromMinutes(10));

            SavedGameMetadataUpdate updatedMetadata = builder.Build();

            PlayGamesPlatform.Instance.SavedGame.CommitUpdate(
                game,
                updatedMetadata,
                dataBytes,
                (commitStatus, commitGame) => OnSavedGameWritten(commitStatus, onComplete));
        }
        else
        {
            Debug.LogError("Error opening saved game: " + status);
            onComplete?.Invoke();
        }
    }

    private void OnSavedGameWritten(SavedGameRequestStatus status, System.Action onComplete)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            Debug.Log("Game data successfully saved to Google Play Games cloud.");
        }
        else
        {
            Debug.LogError("Error saving game data: " + status);
        }

        onComplete?.Invoke();
    }

    public void LoadGameData()
    {
        if (PlayGamesPlatform.Instance.IsAuthenticated())
        {
            PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
                "MultiverseTapBallProgress",
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseMostRecentlySaved,
                OnSavedGameDataOpenedToLoad);
        }
        else
        {
            OnLoadDataFailed();
            Debug.LogWarning("Not authenticated with Google Play Games. Cannot load data.");
        }
    }

    private void OnSavedGameDataOpenedToLoad(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(game, OnSavedGameDataRead);
        }
        else
        {
            OnLoadDataFailed();
            Debug.LogError("Error opening saved game for loading: " + status);
        }
    }

    private void OnSavedGameDataRead(SavedGameRequestStatus status, byte[] data)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            string loadedData = System.Text.Encoding.UTF8.GetString(data);
            Debug.Log("Game data successfully loaded from Google Play Games cloud: " + loadedData);

            // Aplicar los datos cargados al sistema local
            if (!string.IsNullOrEmpty(loadedData) && loadedData != "{}")
            {
                SaveAndLoadManager.ApplyCloudData(loadedData, () => _isInitialized = true, OnLoadDataFailed);
            }
            else
            {
                Debug.LogWarning("Cloud data is empty. Keeping local save data.");
                _isInitialized = true;
            }
        }
        else
        {
            OnLoadDataFailed();
            Debug.LogError("Error reading saved game data: " + status);
        }
    }

    private void OnLoadDataFailed()
    {
        LoadingGameManager.Instance.ShowCantSignInPopUp("conectionfail", "cantloadcloud", () => _isInitialized = true, Application.Quit);
    }

    public override IEnumerator InizializeManagers()
    {
        LoadGameData();

        // Esperar hasta que se complete la carga
        while (!_isInitialized)
            yield return null;
    }
}