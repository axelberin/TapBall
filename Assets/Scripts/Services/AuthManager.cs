using UnityEngine;
using Firebase.Auth;
using System.Collections;
using System.Threading.Tasks;
using Firebase.Extensions;
#if UNITY_ANDROID
using Google;
#elif UNITY_IOS
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;
#endif
using System.Security.Cryptography;
using System.Text;
using System;

public class AuthManager : ManagersManager
{
    public const string GoogleAPI = "70505230779-dcec4ure6uki7ertg47imreu6o07lhrf.apps.googleusercontent.com";
    private const string kSignedOnceKey = "GOOGLE_SIGNED_ONCE";
    private const string kAppleSignedOnceKey = "APPLE_SIGNED_ONCE";

    FirebaseAuth _auth;
    FirebaseUser _user;
#if UNITY_IOS
    private IAppleAuthManager _appleAuthManager;
    private bool _isAppleInit = false;
#elif UNITY_ANDROID
    private bool _isGoogleSignInInitialized = false;
#endif

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
        SignInWithApple(silentOnly: true);
#elif UNITY_ANDROID
        SignInWithGoogle(silentOnly:true);
#else
        _isInitialized = true;
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

#if UNITY_ANDROID
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

            // 2) Si fall� el silent:
            //    - Si es la PRIMERA VEZ (no hay bandera), abrimos chooser.
            //    - Si NO es la primera vez y nos llamaron en arranque (silentOnly=true),
            //      NO abrimos chooser autom�tico -> dejamos que el juego inicie y
            //      que el usuario pulse un bot�n "Reintentar".
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
            PlayerPrefs.SetInt(kSignedOnceKey, 1); // marcamos que ya eligi� cuenta
            PlayerPrefs.Save();

            Debug.Log("Login exitoso en Firebase: " + _user.DisplayName);
            _isInitialized = true;
        });
    }

    public void SignOutGoogle()
    {
        GoogleSignIn.DefaultInstance.SignOut(); // esto borra cach� de Google
        _auth.SignOut(); // tambi�n Firebase
        PlayerPrefs.DeleteKey(kSignedOnceKey);
    }

#elif UNITY_IOS
    public void SignInWithApple(bool silentOnly = false)
    {
        //#if UNITY_IOS
        EnsureAppleInitialized();
        if (_appleAuthManager == null) return;

        string rawNonce = GenerateRandomNonce();
        string hashedNonce = Sha256(rawNonce);

        bool finished = false;
        void fail(string tag, string msg) { OnFailSignIn(tag, msg); finished = true; }

        // 1) Quick login silencioso
        _appleAuthManager.QuickLogin(
            cred => { StartCoroutine(AppleToFirebase(cred, rawNonce, () => finished = true, fail)); },
            err =>
            {
                bool signedOnce = PlayerPrefs.GetInt(kAppleSignedOnceKey, 0) == 1;
                if (silentOnly && signedOnce)
                {
                    fail("AppleSilentNoChooser", "Silent Apple Sign-In failed and chooser disabled");
                    return;
                }

                var args = new AppleAuthLoginArgs(
                    LoginOptions.IncludeEmail | LoginOptions.IncludeFullName,
                    null,
                    hashedNonce
                );
                _appleAuthManager.LoginWithAppleId(
                    args,
                    cred => { StartCoroutine(AppleToFirebase(cred, rawNonce, () => finished = true, fail)); },
                    loginErr => { fail("AppleLoginError", loginErr.ToString()); }
                );
            });

        // Bombeo temporal SOLO durante el login:
        StartCoroutine(PumpAppleUntil(() => finished));
        //#else
        OnFailSignIn("AppleWrongPlatform", "Called SignInWithApple outside iOS");
        _isInitialized = true;
        return;
        //#endif
    }

    private IEnumerator PumpAppleUntil(Func<bool> done)
    {
        const float MAX_SECONDS = 12f;
        float t = 0f;
        while (!done() && t < MAX_SECONDS)
        {
            _appleAuthManager?.Update();
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private IEnumerator AppleToFirebase(ICredential credential, string rawNonce,
                                        Action onSuccess,
                                        Action<string, string> onFail)
    {
        if (!(credential is IAppleIDCredential appleCred))
        {
            onFail("AppleBadCredential", "Credential is not AppleID");
            yield break;
        }

        var tokenBytes = appleCred.IdentityToken;
        if (tokenBytes == null || tokenBytes.Length == 0)
        {
            onFail("AppleNoToken", "No identity token from Apple");
            yield break;
        }

        string idToken = Encoding.UTF8.GetString(tokenBytes);

        var firebaseCred = OAuthProvider.GetCredential("apple.com", idToken, rawNonce, null);
        var t = _auth.SignInWithCredentialAsync(firebaseCred);
        yield return new WaitUntil(() => t.IsCompleted);

        if (t.IsFaulted || t.IsCanceled)
        {
            onFail("AppleFirebaseFail", t.Exception?.Message ?? "SignInWithCredentialAsync failed");
            yield break;
        }

        _user = t.Result;
        PlayerPrefs.SetInt(kAppleSignedOnceKey, 1);
        PlayerPrefs.Save();

        Debug.Log("Login Apple exitoso (Firebase): " + _user.DisplayName);
        _isInitialized = true;
        onSuccess?.Invoke();
    }

    private void EnsureAppleInitialized()
    {
        if (_isAppleInit) return;
        if (!AppleAuthManager.IsCurrentPlatformSupported)
            throw new System.Exception("Apple Sign-In not supported on this platform");

        var deserializer = new PayloadDeserializer();
        _appleAuthManager = new AppleAuthManager(deserializer);
        _isAppleInit = true;
    }

#endif
    private string GenerateRandomNonce(int length = 32)
    {
        const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._";
        var data = new byte[length];
        using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(data);
        var chars = new char[length];
        for (int i = 0; i < length; i++) chars[i] = charset[data[i] % charset.Length];
        return new string(chars);
    }

    private string Sha256(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    public override IEnumerator InizializeManagers()
    {
        // Timeout del watchdog (pod�s ajustarlo)
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

    private void OnFailSignIn(string tag, string msg, params (string key, object val)[] keys)
{
    GameLog.NonFatal(tag, msg, keys);
    GameLog.LogEvent("auth_failed", ("tag", tag), ("message", msg));

    Debug.LogWarning($"Auth failed, continuing in local mode. Tag: {tag}, Msg: {msg}");

    // No bloquear el inicio del juego
    _isInitialized = true;
}
}
