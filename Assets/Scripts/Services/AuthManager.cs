using UnityEngine;
using Firebase.Auth;
using System.Collections;
using Google;

public class AutoAuthManager : ManagersManager
{
    private FirebaseAuth _auth;

    protected override void Start()
    {
        _auth = FirebaseAuth.DefaultInstance;

        if (_auth.CurrentUser != null)
        {
            Debug.Log("Ya hay sesión iniciada: " + _auth.CurrentUser.DisplayName);
            return;
        }

#if UNITY_EDITOR
        Debug.Log("Simulando login en Editor...");
        var fakeUser = new { DisplayName = "EditorUser", UserId = "12345" };
        Debug.Log("Login simulado: " + fakeUser.DisplayName);
        _isInitialized = true;
#elif UNITY_IOS
        SignInWithApple();
#elif UNITY_ANDROID
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            WebClientId = "1067990701779-7ruheridq1uesrkoa4f7uqhhjodur2v3.apps.googleusercontent.com", // este sí va fijo
            RequestIdToken = true
        };

        SignInWithGoogle();
#endif
        base.Start();
    }

    private void SignInWithGoogle()
    {
        Debug.Log("Intentando login automático con Google...");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error en Google Sign-In: " + task.Exception);
                return;
            }

            Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);

            _auth.SignInWithCredentialAsync(credential).ContinueWith(authTask =>
            {
                if (authTask.IsCanceled || authTask.IsFaulted)
                {
                    Debug.LogError("Error login Firebase: " + authTask.Exception);
                    return;
                }

                FirebaseUser user = authTask.Result;
                Debug.Log("Login Firebase exitoso: " + user.DisplayName);
            });
        });
    }

    private void SignInWithApple()
    {
        Debug.Log("Intentando login automático con Apple...");

        // Acá llamás al plugin de Sign in with Apple para obtener el idToken
        string idToken = "";

        Credential credential = OAuthProvider.GetCredential("apple.com", idToken, null, null);
        _auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Error login Apple: " + task.Exception);
                return;
            }

            FirebaseUser user = task.Result;
            Debug.Log("Login Apple exitoso: " + user.DisplayName);
        });
    }

    public override IEnumerator InizializeManagers()
    {
        while (!_isInitialized)
            yield return null;
    }
}
