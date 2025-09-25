using UnityEngine;
using Firebase.Auth;
using System.Collections;
using Google;
using System.Threading.Tasks;
using Firebase.Extensions;

public class AuthManager : ManagersManager
{
    public const string GoogleAPI = "70505230779-dcec4ure6uki7ertg47imreu6o07lhrf.apps.googleusercontent.com";
    private const string kSignedOnceKey = "GOOGLE_SIGNED_ONCE";

    FirebaseAuth _auth;
    FirebaseUser _user;
    private bool _isGoogleSignInInitialized = false;

    protected override void Start()
    {
        _auth = FirebaseAuth.DefaultInstance;
        _auth.StateChanged += OnAuthStateChanged; // escuchar restauraciones
        _user = _auth.CurrentUser;

        base.Start();

        if (_user != null)
        {
            Debug.Log("Sesion restaurada: " + _user.DisplayName);
            _isInitialized = true;
            return;
        }

#if UNITY_EDITOR
        Debug.Log("Simulando login en Editor...");
        _isInitialized = true;
#elif UNITY_IOS
        SignInWithApple();
#elif UNITY_ANDROID
        SignInWithGoogle(silentOnly:true);
#endif
    }

    private void OnDestroy()
    {
        if (_auth != null)
            _auth.StateChanged -= OnAuthStateChanged;
    }

    private void OnAuthStateChanged(object sender, System.EventArgs e)
    {
        if (_auth.CurrentUser != null && !_isInitialized)
        {
            _user = _auth.CurrentUser;
            Debug.Log("Auth state changed -> user: " + _user.DisplayName);
            _isInitialized = true;
        }
    }

    // si silentOnly = true, NO abrir el chooser; ideal para el arranque
    public void SignInWithGoogle(bool silentOnly = false)
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

        // 1) Intento silencioso
        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled && task.Result != null)
            {
                OnGoogleAuthFinished(task); // ok: tenemos token, seguimos con Firebase
                return;
            }

            // 2) Si falló el silent:
            //    - Si es la PRIMERA VEZ (no hay bandera), abrimos chooser.
            //    - Si NO es la primera vez y nos llamaron en arranque (silentOnly=true),
            //      NO abrimos chooser automático -> dejamos que el juego inicie y
            //      que el usuario pulse un botón "Reintentar".
            bool signedOnce = PlayerPrefs.GetInt(kSignedOnceKey, 0) == 1;
            if (silentOnly && signedOnce)
                OnFailSignIn("SilentNoChooser", "Silent GoogleSignIn failed and chooser disabled");
            else
                GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(OnGoogleAuthFinished);
        });
    }

    private void OnGoogleAuthFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            Debug.LogError("Google Sign-In error: " + task.Exception);
            OnFailSignIn("FailSignIn", "OnGoogleAuthFinished task = IsFaulted",
                ("exception", task.Exception?.Message),
                ("inner", task.Exception?.InnerException?.Message));
            return;
        }
        if (task.IsCanceled)
        {
            Debug.LogWarning("Google Sign-In cancelado");
            OnFailSignIn("CancelSignIn", "OnGoogleAuthFinished task = IsCanceled",
                ("exception", task.Exception?.Message),
                ("inner", task.Exception?.InnerException?.Message));
            return;
        }

        Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);
        _auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
        {
            if (authTask.IsFaulted || authTask.IsCanceled)
            {
                Debug.LogError("Error en Firebase: " + authTask.Exception);
                OnFailSignIn("AuthCredentialFail", "SignInWithCredentialAsync task fail",
                ("exception", authTask.Exception?.Message),
                ("inner", authTask.Exception?.InnerException?.Message));
                return;
            }

            _user = authTask.Result;
            PlayerPrefs.SetInt(kSignedOnceKey, 1); // marcamos que ya eligió cuenta
            PlayerPrefs.Save();

            Debug.Log("Login exitoso en Firebase: " + _user.DisplayName);
            _isInitialized = true;
        });
    }

    private void SignInWithApple()
    {
        Debug.Log("Intentando login automático con Apple...");
        string idToken = "";
        Credential credential = OAuthProvider.GetCredential("apple.com", idToken, null, null);
        _auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Error login Apple: " + task.Exception);
                //OnFailSignIn();
                return;
            }
            FirebaseUser user = task.Result;
            Debug.Log("Login Apple exitoso: " + user.DisplayName);
        });
    }

    public override IEnumerator InizializeManagers()
    {
        // Timeout del watchdog (podés ajustarlo)
        const float AUTH_TIMEOUT_SECONDS = 12f;
        float elapsed = 0f;
        bool watchdogShown = false;

        while (!_isInitialized)
        {
            // usar unscaled por si la carga pone timeScale=0
            elapsed += Time.unscaledDeltaTime;

            if (!watchdogShown && elapsed >= AUTH_TIMEOUT_SECONDS)
            {
                watchdogShown = true;
                OnFailSignIn("AuthTimeout", "Auth took too long",
                    ("timeout_seconds", AUTH_TIMEOUT_SECONDS));
            }

            yield return null;
        }
    }

    public void SignOutGoogle()
    {
        GoogleSignIn.DefaultInstance.SignOut(); // esto borra caché de Google
        _auth.SignOut(); // también Firebase
        PlayerPrefs.DeleteKey(kSignedOnceKey);
    }

    private void OnFailSignIn(string tag, string msg, params (string key, object val)[] keys)
    {
        GameLog.NonFatal(tag, msg, keys);
        GameLog.LogEvent("auth_failed", ("tag", tag), ("message", msg));
        if (LoadingGameManager.Instance)
            LoadingGameManager.Instance.ShowCantSignInPopUp("conectionfail", "cantconnect",
                () => _isInitialized = true, () => SignInWithGoogle(silentOnly: true));
        else
            _isInitialized = true;
    }
}
