using Firebase.Auth;
using UnityEngine;
using TMPro;

public class AuthManager : MonoBehaviour
{
    // Referencia al Auth de Firebase
    FirebaseAuth auth;

    void Start()
    {
        // Inicializa la instancia de Auth
        auth = FirebaseAuth.DefaultInstance;
    }

    // Método para registrar un nuevo usuario
    public void RegisterUser(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Email y contraseña requeridos.");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
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
    public void LoginUser(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, email).ContinueWith(task =>
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
