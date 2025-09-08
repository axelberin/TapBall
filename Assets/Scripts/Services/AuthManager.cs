using UnityEngine;
using Firebase.Auth;
using System.Collections;

public class AutoAuthManager : ManagersManager
{
    private FirebaseAuth _auth;

    void Start()
    {
        _auth = FirebaseAuth.DefaultInstance;

        if (_auth.CurrentUser != null)
        {
            Debug.Log("Ya hay sesión iniciada: " + _auth.CurrentUser.DisplayName);
            return;
        }


#if UNITY_ANDROID
        SignInWithGoogle();
#elif UNITY_IOS
        SignInWithApple();
#else
        Debug.Log("Plataforma no soportada para login automático.");
#endif
    }

    private void SignInWithGoogle()
    {
        Debug.Log("Intentando login automático con Google...");

        // Acá llamás al plugin de Google Sign-In para obtener el idToken
        string idToken = "1067990701779-7ruheridq1uesrkoa4f7uqhhjodur2v3.apps.googleusercontent.com";

        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
        _auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Error login Google: " + task.Exception);
                return;
            }

            FirebaseUser user = task.Result;
            Debug.Log("Login Google exitoso: " + user.DisplayName);
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
