using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections;
using GooglePlayGames.BasicApi.SavedGame;

public class GoogleSignInManager : ManagersManager
{
    private void Awake()
    {
        PlayGamesPlatform.Instance.Authenticate(SignInSilently);
    }

    // Método para iniciar sesión de forma silenciosa (si ya ha iniciado sesión antes)
    public void SignInSilently(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            Debug.Log("Inicio de sesión silencioso exitoso. ¡Bienvenido, " + PlayGamesPlatform.Instance.GetUserDisplayName() + "!");
            OnSignInSuccess();
        }
        else
            Debug.Log("Inicio de sesión silencioso fallido. Estado: " + status);
    }

    // Método para iniciar sesión cuando el usuario hace clic en un botón
    public void SignInManually()
    {
        PlayGamesPlatform.Instance.Authenticate((success) =>
        {
            if (success == SignInStatus.Success)
            {
                Debug.Log("Inicio de sesión manual exitoso. ¡Bienvenido, " + PlayGamesPlatform.Instance.GetUserDisplayName() + "!");
                OnSignInSuccess();
            }
            else
                Debug.Log("Inicio de sesión manual fallido. Estado: " + success);
        });
    }

    // Método para cerrar sesión
    public void SignOut()
    {
        if (PlayGamesPlatform.Instance.IsAuthenticated())
        {
            //PlayGamesPlatform.Instance.SignOut();
            Debug.Log("Sesión cerrada.");
        }
        else
            Debug.Log("No hay sesión activa para cerrar.");
    }

    // Método que se llama cuando el inicio de sesión es exitoso
    private void OnSignInSuccess()
    {
        // Aquí puedes realizar acciones posteriores al inicio de sesión exitoso, como:
        // - Cargar datos del usuario desde un servidor
        // - Habilitar funcionalidades del juego
        // - Cargar la siguiente escena
        Debug.Log("Usuario ID: " + PlayGamesPlatform.Instance.GetUserId());
        Debug.Log("Usuario Nombre: " + PlayGamesPlatform.Instance.GetUserDisplayName());
        // Puedes obtener el token de ID o el código de autorización del servidor si los solicitaste en la configuración:
        // string idToken = PlayGamesPlatform.Instance.Get
        // string serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
    }

    public override IEnumerator InizializeManagers()
    {
        yield return new WaitForSeconds(1);

        _isInitialized = true;
    }

    public void SaveGameData(string dataToSave) // 'dataToSave' sería tu JSON o datos serializados
    {
        if (PlayGamesPlatform.Instance.IsAuthenticated())
        {
            (PlayGamesPlatform.Instance).SavedGame.OpenWithAutomaticConflictResolution(
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

            (PlayGamesPlatform.Instance).SavedGame.CommitUpdate(
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
            (PlayGamesPlatform.Instance).SavedGame.OpenWithAutomaticConflictResolution(
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
            (PlayGamesPlatform.Instance).SavedGame.ReadBinaryData(
                game,
                OnSavedGameDataRead);
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
        }
        else
            Debug.LogError("Error reading saved game data: " + status);
    }
}