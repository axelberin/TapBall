using UnityEngine;
using Firebase.Auth;

public class AutoAuthManager : MonoBehaviour
{
    private FirebaseAuth auth;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            Debug.Log("Ya hay sesión iniciada: " + auth.CurrentUser.DisplayName);
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
        string idToken = "70505230779-dita4q8jooj19sdb4j8kta4tg44jne65.apps.googleusercontent.com";

        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
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
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
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
}
