using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase;
using Google;
using System;

public class AuthenticationManager : MonoSingleton<AuthenticationManager>
{
    public enum LoginType {
        Guest,
        Google,
        EmailLogin,
        EmailCreate
    }

    FirebaseAuth auth;
    FirebaseUser user;
    bool isInit = false;
    string userId = string.Empty;
    string userName = string.Empty;

    public string UserId => userId;
    public string UserName => userName;

    public Action<bool> OnUserLoggedIn;
    public Action OnUserLoggedOut;

    private void Start() {
        DontDestroyOnLoad(this);
    }

    public void Init() {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith((task) => {
            auth = FirebaseAuth.DefaultInstance;
            isInit = true;
        });
        
    }

    public void Login(int type) {
        Login(type, "", "");
    }

    public async void Login(int type, string email = "", string pass = "") {
        Task<bool> loginTask = null;
        switch (type) {
            case (int)LoginType.Guest:
                loginTask = GuestLogin();
                break;
            case (int)LoginType.Google:
                //loginTask = GoogleLogin();
                break;
            case (int)LoginType.EmailLogin:
                loginTask = EmailLogin(email, pass);
                break;
            case (int)LoginType.EmailCreate:
                loginTask = CreateAccount(email, pass);
                break;
        }

        if (loginTask != null)
            await loginTask;

        OnUserLoggedIn?.Invoke(loginTask.Result);

    }

    async Task<bool> GuestLogin() {
        if (!isInit) 
            Init();

        string accountEmail = SystemInfo.deviceUniqueIdentifier + "@rlgame.com";
        string pass = SystemInfo.deviceUniqueIdentifier.Substring(0, 8);

        try {
            AuthResult result = await auth.CreateUserWithEmailAndPasswordAsync(accountEmail, pass);
            user = result.User;
            userId = result.User.UserId;
        }catch (FirebaseException ex) {
            if (ex.ErrorCode == 8) {
                return await EmailLogin(accountEmail, pass);
            }
            else {
                Debug.LogError("Failed to login as guest");
                return false;
            }
        }

        userName = user.DisplayName;
        if (string.IsNullOrEmpty(userName))
            userName = user.UserId;

        return true;
    }

    public async Task<bool> EmailLogin(string email, string password) {
        if (!isInit)
            Init();
        try {
            Task<AuthResult> loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
            await loginTask;

            if (loginTask.IsCanceled) {
                Debug.Log("[Auth] EmailLogin canceled");
                return false;
            }
            if (loginTask.IsFaulted) {
                PopupPanel.Instance.Show("", loginTask.Exception.Message, null);
                Debug.Log("[Auth] EmailLogin error: " + loginTask.Exception);
                return false;
            }
            user = loginTask.Result.User;
        }catch(FirebaseException ex) {
            if(ex.ErrorCode == 12) {
                PopupPanel.Instance.Show("", "The email or password is invalid", null);
            }
            else {
                Debug.LogError(ex.Message);
            }
            return false;
        }
        
        userId = user.UserId;
        userName = user.DisplayName;

        if(string.IsNullOrEmpty(userName))
            userName = "Guest";

        return true;
    }

    public async Task<bool> CreateAccount(string email, string password) {
        if (!isInit)
            Init();

        try {
            Task<AuthResult> createUserTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
            await createUserTask;

            if (createUserTask.IsCanceled) {
                Debug.Log("[Auth] EmailLogin canceled");
                return false;
            }
            if (createUserTask.IsFaulted) {
                PopupPanel.Instance.Show("", createUserTask.Exception.Message, null);
                Debug.Log($"[Auth] EmailLogin error: {createUserTask.Exception.Message} | {createUserTask.Exception}");
                return false;
            }

            user = createUserTask.Result.User;
        }catch (FirebaseException ex) {
            if(ex.ErrorCode == 8) {
                PopupPanel.Instance.Show("", ex.Message, null);
            }
            return false;
        }

        userId = user.UserId;
        userName = user.DisplayName;
        if (string.IsNullOrEmpty(userName))
            userName = "Guest";

        return true;
    }

    /*async Task<bool> GoogleLogin() {
        GoogleSignIn.Configuration = new GoogleSignInConfiguration {
            RequestIdToken = false,
            WebClientId = "299180800988-539r2205koupqdqedlsk3po47b5hm7kv.apps.googleusercontent.com"
        };

        GoogleSignIn.DefaultInstance.EnableDebugLogging(true);
        Task<GoogleSignInUser> googleLogin = GoogleSignIn.DefaultInstance.SignIn();

        TaskCompletionSource<FirebaseUser> loginComplete = new TaskCompletionSource<FirebaseUser>();
        await googleLogin.ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("[Auth]Google Login canceled");
            }

            if (task.IsFaulted) {
                Debug.LogError("[Autg] Google Login error: " + task.Exception);
            }

            Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);
            auth.SignInWithCredentialAsync(credential);

        });

        return true;

    }*/

    public void Logout() {
        try {
            auth.SignOut();
            OnUserLoggedOut?.Invoke();
        }catch (FirebaseException ex) {
            Debug.LogError(ex.ToString());
            PopupPanel.Instance.Show("", "Something went wrong. Please try again later.", null);
        }
    }
}
