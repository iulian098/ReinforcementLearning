using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSystem : MonoSingleton<LevelSystem>
{
    [SerializeField] AnimationCurve levelCurve;
    public Action<int> OnLevelUp;
    bool levelUpPending = false;

    private void Start() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode) {
        if (scene.name == "MainMenu" && levelUpPending)
            PopupPanel.Instance.Show("Level Up!", $"You just leveled up!\nLevel {UserManager.playerData.GetInt(PlayerPrefsStrings.LEVEL) + 1}", null);
    }

    public void AddExp(int value) {
        UserManager.playerData.AddInt(PlayerPrefsStrings.EXP, value);
        int expNeeded = (int)levelCurve.Evaluate(UserManager.playerData.GetInt(PlayerPrefsStrings.LEVEL));
        int currentExp = UserManager.playerData.GetInt(PlayerPrefsStrings.EXP) + value;

        while (currentExp > expNeeded) {
            UserManager.playerData.AddInt(PlayerPrefsStrings.LEVEL, 1);
            UserManager.playerData.SetInt(PlayerPrefsStrings.EXP, currentExp - expNeeded);
            OnLevelUp?.Invoke(UserManager.playerData.GetInt(PlayerPrefsStrings.LEVEL));
            if (SceneManager.GetActiveScene().name != "MainMenu")
                levelUpPending = true;
            else
                PopupPanel.Instance.Show("Level Up!", $"You just leveled up!\nLevel {UserManager.playerData.GetInt(PlayerPrefsStrings.LEVEL) + 1}", null);

            expNeeded = (int)levelCurve.Evaluate(UserManager.playerData.GetInt(PlayerPrefsStrings.LEVEL));
            currentExp = UserManager.playerData.GetInt(PlayerPrefsStrings.EXP) + value;
        }

    }

    public float GetLevelProgress() {
        int expNeeded = (int)levelCurve.Evaluate(UserManager.playerData.GetInt(PlayerPrefsStrings.LEVEL));
        int currentExp = UserManager.playerData.GetInt(PlayerPrefsStrings.EXP);
        Debug.Log($"[LevelSystem]{currentExp}/{expNeeded}");
        return (float)currentExp / expNeeded;
    }
}
