using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    void Start()
    {
        ChangeState(State.Init);
    }

    public void ChangeState(State newState) {
        switch (newState) {
            case State.Init:
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
        await remoteConfigManager.FetchData();
        ChangeState(State.LoadSave);
    }

    void LoadSaveFile() {
        saveSystem.OnSaveFileLoaded += () => {
            ChangeState(State.ChangeScene);
        };
        saveSystem.Init();
    }

    private void OnUserLoggedIn(bool success) {
        if (success)
            ChangeState(State.RemoteConfig);
    }

    void ChangeScene() {
        SceneManager.LoadScene(1);
    }
}
