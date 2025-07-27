using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames.BasicApi;
using GooglePlayGames;
using UnityEngine;
using System.Collections;

public class SaveAndLoadOnCloudManager : ManagersManager
{
    public void SaveGameData(string dataToSave) // 'dataToSave' sería tu JSON o datos serializados
    {
        if (PlayGamesPlatform.Instance.IsAuthenticated())
        {
            PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
                "MultiverseTapBallProgress",
                DataSource.ReadCacheOrNetwork, // Intentar leer de la caché o la red
                ConflictResolutionStrategy.UseMostRecentlySaved, // Estrategia de resolución de conflictos
                OnSavedGameOpened);
        }
        else
            Debug.LogWarning("Not authenticated with Google Play Games. Cannot save data.");
    }

    private void OnSavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            // El archivo se ha abierto correctamente. Ahora, lee los datos actuales, modifícalos y escríbelos.
            // Para simplificar, aquí se asume que solo queremos escribir directamente.
            // En un caso real, primero leerías los datos existentes, los actualizarías y luego escribirías.

            string currentData = "{\"level\": 5, \"score\": 1200}"; // Ejemplo de datos (reemplazar con tus datos reales)
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(currentData);

            SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder()
                .WithUpdatedDescription("Saved Game at " + System.DateTime.Now.ToShortTimeString())
                .WithUpdatedPlayedTime(System.TimeSpan.FromMinutes(10)); // Actualiza el tiempo de juego

            SavedGameMetadataUpdate updatedMetadata = builder.Build();

            PlayGamesPlatform.Instance.SavedGame.CommitUpdate(
                game,
                updatedMetadata,
                dataBytes,
                OnSavedGameWritten);
        }
        else
            Debug.LogError("Error opening saved game: " + status);
    }

    private void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
            Debug.Log("Game data successfully saved to Google Play Games cloud.");
        else
            Debug.LogError("Error saving game data: " + status);
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
            Debug.LogWarning("Not authenticated with Google Play Games. Cannot load data.");
    }

    private void OnSavedGameDataOpenedToLoad(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            // Ahora, lee el contenido del archivo guardado
            PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(
                game, OnSavedGameDataRead);
        }
        else
            Debug.LogError("Error opening saved game for loading: " + status);
    }

    private void OnSavedGameDataRead(SavedGameRequestStatus status, byte[] data)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            string loadedData = System.Text.Encoding.UTF8.GetString(data);
            Debug.Log("Game data successfully loaded from Google Play Games cloud: " + loadedData);
            // Aquí puedes deserializar 'loadedData' y aplicar los datos a tu juego

            _isInitialized = true;
        }
        else
            Debug.LogError("Error reading saved game data: " + status);
    }

    public override IEnumerator InizializeManagers()
    {
        LoadGameData();
        yield return new WaitForSeconds(1);
    }
}
