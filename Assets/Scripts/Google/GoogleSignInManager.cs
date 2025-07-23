using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using TMPro; // Si usas TextMeshPro para mostrar mensajes
using UnityEngine.UI; // Si usas UI Text para mostrar mensajes

public class GoogleSignInManager : MonoBehaviour
{
    public TextMeshProUGUI statusText; // Asigna un componente TextMeshProUGUI en el Inspector
    // O si usas el sistema UI Text tradicional:
    // public Text statusText; // Asigna un componente Text en el Inspector

    private void Start()
    {
        // Configura la autenticación
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        // Intenta iniciar sesión automáticamente si el usuario ya ha autenticado antes
        SignInSilently();
    }

    // Método para iniciar sesión de forma silenciosa (si ya ha iniciado sesión antes)
    public void SignInSilently()
    {
        if (statusText != null) statusText.text = "Intentando inicio de sesión silencioso...";

        PlayGamesPlatform.Instance.Authenticate((success) =>
        {
            if (success == SignInStatus.Success)
            {
                Debug.Log("Inicio de sesión silencioso exitoso. ¡Bienvenido, " + PlayGamesPlatform.Instance.GetUserDisplayName() + "!");
                if (statusText != null) statusText.text = "¡Sesión iniciada como: " + PlayGamesPlatform.Instance.GetUserDisplayName() + "!";
                // Aquí puedes cargar la escena principal del juego o hacer otras acciones
                OnSignInSuccess();
            }
            else
            {
                Debug.Log("Inicio de sesión silencioso fallido. Estado: " + success);
                if (statusText != null) statusText.text = "Error al iniciar sesión silenciosa. Intenta con el botón.";
            }
        });
    }

    // Método para iniciar sesión cuando el usuario hace clic en un botón
    public void SignInManually()
    {
        if (statusText != null) statusText.text = "Intentando inicio de sesión manual...";

        PlayGamesPlatform.Instance.Authenticate((success) =>
        {
            if (success == SignInStatus.Success)
            {
                Debug.Log("Inicio de sesión manual exitoso. ¡Bienvenido, " + PlayGamesPlatform.Instance.GetUserDisplayName() + "!");
                if (statusText != null) statusText.text = "¡Sesión iniciada como: " + PlayGamesPlatform.Instance.GetUserDisplayName() + "!";
                OnSignInSuccess();
            }
            else
            {
                Debug.Log("Inicio de sesión manual fallido. Estado: " + success);
                if (statusText != null) statusText.text = "Error al iniciar sesión manualmente. Estado: " + success;
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
            if (statusText != null) statusText.text = "Sesión cerrada.";
        }
        else
        {
            Debug.Log("No hay sesión activa para cerrar.");
            if (statusText != null) statusText.text = "No hay sesión activa.";
        }
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

    // Puedes agregar más funciones aquí, como mostrar logros, tablas de clasificación, etc.
}