using UnityEngine;
using Firebase.Auth;
using System.Collections;
using Google;
using System.Threading.Tasks;

public class AuthManager : ManagersManager
{
    public const string GoogleAPI = "1067990701779-7ruheridq1uesrkoa4f7uqhhjodur2v3.apps.googleusercontent.com";
    FirebaseAuth _auth;
    FirebaseUser _user;

    private bool _isGoogleSignInInitialized = false;

    protected override void Start()
    {
        _auth = FirebaseAuth.DefaultInstance;
        _user = _auth.CurrentUser;

        base.Start();

        if (_user != null)
        {
            Debug.Log("Ya hay sesión iniciada: " + _user.DisplayName);
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
        SignInWithGoogle();
#endif
    }

    public void SignInWithGoogle()
    {
        if (!_isGoogleSignInInitialized)
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = GoogleAPI,
                RequestEmail = true
            };

            _isGoogleSignInInitialized = true;
        }

        Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignInSilently();

        signIn.ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled || task.Result == null)
            {
                // Si falla o no hay sesión previa, mostrar chooser normal
                GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleAuthFinished);
            }
            else
            {
                OnGoogleAuthFinished(task);
            }
        });
    }

    private void OnGoogleAuthFinished(Task<GoogleSignInUser> task)
    {
        TaskCompletionSource<FirebaseUser> signInCompleted = new();
        if (task.IsFaulted)
        {
            signInCompleted.SetCanceled();
            Debug.LogError("Google Sign-In error: " + task.Exception);
            return;
        }
        if (task.IsCanceled)
        {
            signInCompleted.SetException(task.Exception);
            Debug.LogWarning("Google Sign-In cancelado");
            return;
        }

        Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);
        _auth.SignInWithCredentialAsync(credential).ContinueWith(authTask =>
        {
            if (authTask.IsFaulted || authTask.IsCanceled)
            {
                signInCompleted.SetCanceled();
                Debug.LogError("Error en Firebase: " + authTask.Exception);
                return;
            }

            signInCompleted.SetResult(authTask.Result);
            _user = authTask.Result;
            Debug.Log("Login exitoso en Firebase: " + _user.DisplayName);
            _isInitialized = true;
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
