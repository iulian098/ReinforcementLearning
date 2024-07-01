using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum State {
        Init,
        Login,
        RemoteConfig,
        LoadSave,
        ChangeScene
    }

    [Header("Status Panel")]
    [SerializeField] GameObject statusPanel;
    [SerializeField] TMP_Text statusText;

    [Header("Login")]
    [SerializeField] TMP_InputField login_Email;
    [SerializeField] TMP_InputField login_Password;
    [Header("Create")]
    [SerializeField] TMP_InputField create_Email;
    [SerializeField] TMP_InputField create_Password;
    [SerializeField] TMP_InputField create_RepeatPassword;

    [Space]
    [SerializeField] RemoteConfigManager remoteConfigManager;
    [SerializeField] SaveSystem saveSystem;

    State currentState;
    bool isOffline;
    void Start()
    {
        if (remoteConfigManager == null) remoteConfigManager = RemoteConfigManager.Instance;
        saveSystem = SaveSystem.Instance;

        isOffline = Application.internetReachability == NetworkReachability.NotReachable;

        ChangeState(State.Init);
    }

    public void ChangeState(State newState) {
        currentState = newState;
        switch (newState) {
            case State.Init:
                if(isOffline)
                    ChangeState(State.LoadSave);
                else
                    Init();
                break;
            case State.Login:
                break;
            case State.RemoteConfig:
                RemoteConfigFetch();
                break;
            case State.LoadSave:
                LoadSaveFile();
                break;
            case State.ChangeScene:
                ChangeScene();
                break;
            default:
                break;
        }
    }

    void Init() {
        AuthenticationManager.Instance.Init();
        AuthenticationManager.Instance.OnUserLoggedIn += OnUserLoggedIn;
        ChangeState(State.Login);
    }

    async void RemoteConfigFetch() {
        statusPanel.SetActive(true);
        statusText.text = "Loading Remote Config";
        await remoteConfigManager.FetchData();
        ChangeState(State.LoadSave);
    }

    void LoadSaveFile() {
        statusText.text = "Loading Save File";
        saveSystem.OnSaveFileLoaded += () => {
            ChangeState(State.ChangeScene);
        };
        _ = saveSystem.Init();
    }

    private void OnUserLoggedIn(bool success) {
        if (success)
            ChangeState(State.RemoteConfig);
    }

    void ChangeScene() {
        SceneManager.LoadScene(1);
    }
}
