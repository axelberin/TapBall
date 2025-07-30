using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections;

public class GoogleSignInManager : ManagersManager
{
    // Método para iniciar sesión de forma silenciosa (si ya ha iniciado sesión antes)
    public void SignInSilently(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            Debug.Log("Inicio de sesión silencioso exitoso. ¡Bienvenido, " + PlayGamesPlatform.Instance.GetUserDisplayName() + "!");
            OnSignInSuccess();
        }
        else
        {
            OnFailSignIn();
            Debug.LogError("Inicio de sesión silencioso fallido. Estado: " + status);
        }
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
            {
                OnFailSignIn();
                Debug.LogError("Inicio de sesión manual fallido. Estado: " + success);
            }
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
            Debug.LogError("No hay sesión activa para cerrar.");
    }

    // Método que se llama cuando el inicio de sesión es exitoso
    private void OnSignInSuccess()
    {
        _isInitialized = true;
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

    private void OnFailSignIn()
    {
        LoadingGameManager.Instance.ShowCantSignInPopUp("conectionfail", "cantconnect", () => _isInitialized = true, Application.Quit);
    }

    public override IEnumerator InizializeManagers()
    {
        PlayGamesPlatform.Instance.Authenticate(SignInSilently);

        while (!_isInitialized)
            yield return null;
    }
}