using UnityEngine;
using Firebase.Auth;
using System.Collections;
using Google;
using System.Threading.Tasks;

public class AuthManager : ManagersManager
{
    public const string GoogleAPI = "1067990701779-7ruheridq1uesrkoa4f7uqhhjodur2v3.apps.googleusercontent.com";
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
        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(task =>
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
            if (!silentOnly || !signedOnce)
            {
                GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleAuthFinished);
            }
            else
            {
                Debug.Log("Silent falló pero ya había login previo. No forzamos chooser.");
            }
        });
    }

    private void OnGoogleAuthFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            Debug.LogError("Google Sign-In error: " + task.Exception);
            return;
        }
        if (task.IsCanceled)
        {
            Debug.LogWarning("Google Sign-In cancelado");
            return;
        }

        Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);
        _auth.SignInWithCredentialAsync(credential).ContinueWith(authTask =>
        {
            if (authTask.IsFaulted || authTask.IsCanceled)
            {
                Debug.LogError("Error en Firebase: " + authTask.Exception);
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

    public void SignOutGoogle()
    {
        GoogleSignIn.DefaultInstance.SignOut(); // esto borra caché de Google
        _auth.SignOut(); // también Firebase
        PlayerPrefs.DeleteKey(kSignedOnceKey);
    }
}
