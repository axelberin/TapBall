using Firebase.Auth;
using UnityEngine;
using TMPro;

public class AuthManager : MonoBehaviour
{
    // Campos de entrada para el correo y la contraseña
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    // Referencia al Auth de Firebase
    FirebaseAuth auth;

    void Start()
    {
        // Inicializa la instancia de Auth
        auth = FirebaseAuth.DefaultInstance;
    }

    // Método para registrar un nuevo usuario
    public void RegisterUser()
    {
        if (string.IsNullOrEmpty(emailInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            Debug.LogWarning("Email y contraseña requeridos.");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(emailInput.text, passwordInput.text).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Error al registrar usuario: " + task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result.User;
            Debug.LogFormat("Usuario registrado: {0} ({1})", newUser.DisplayName, newUser.UserId);
        });
    }

    // Método para iniciar sesión
    public void LoginUser()
    {
        auth.SignInWithEmailAndPasswordAsync(emailInput.text, passwordInput.text).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Error al iniciar sesión: " + task.Exception);
                return;
            }

            FirebaseUser user = task.Result.User;
            Debug.LogFormat("Sesión iniciada: {0} ({1})", user.DisplayName, user.UserId);
        });
    }

    public void SignInWithGoogle(string idToken)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Error en login con Google: " + task.Exception);
                return;
            }

            FirebaseUser user = task.Result;
            Debug.Log("Usuario logueado con Google: " + user.DisplayName + " (" + user.UserId + ")");
        });
    }

}
