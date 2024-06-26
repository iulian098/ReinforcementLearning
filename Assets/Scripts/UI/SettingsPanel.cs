using Racing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    const string MASTER_VOLUME = "MasterVolume";
    const string MUSIC_VOLUME = "MusicVolume";
    const string SFX_VOLUME = "SFXVolume";

    [SerializeField] GameSettings gameSettings;

    [SerializeField] TMP_InputField playerNameInput;

    [Header("Audio")]
    [SerializeField] Slider masterVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider sfxVolumeSlider;

    [Space, Header("Graphics")]
    [SerializeField] TMP_Dropdown fullscreen;
    [SerializeField] TMP_Dropdown resolution;
    [SerializeField] TMP_Dropdown qualityLevel;
    [SerializeField] Slider renderScale;
    [SerializeField] TMP_Dropdown vsync;
    [SerializeField] TMP_Dropdown antialiasing;
    [SerializeField] TMP_Dropdown shadowQuality;

    [SerializeField] GameObject[] desktopOnlySettings;

    AudioMixer audioMixer;

    public void Start() {

#if UNITY_ANDROID
        foreach (var item in desktopOnlySettings) {
            item.SetActive(false);
        }
#endif

        audioMixer = AudioManager.Instance.Mixer;

        List<string> fullScreenNames = System.Enum.GetNames(typeof(FullScreenMode)).ToList();
        fullscreen.AddOptions(fullScreenNames);

        List<string> resolutions = new List<string>();
        for (int i = 0; i < Screen.resolutions.Length; i++)
            resolutions.Add($"{Screen.resolutions[i].width}x{Screen.resolutions[i].height} {Screen.resolutions[i].refreshRateRatio}Hz");
        resolution.AddOptions(resolutions);

        audioMixer.GetFloat(MASTER_VOLUME, out float masterVolume);
        audioMixer.GetFloat(MUSIC_VOLUME, out float musicVolume);
        audioMixer.GetFloat(SFX_VOLUME, out float sfxVolume);


        playerNameInput.text = UserManager.playerData.PlayerName;

        masterVolumeSlider.value = Mathf.InverseLerp(-80f, 0, masterVolume);
        musicVolumeSlider.value = Mathf.InverseLerp(-80f, 0, musicVolume);
        sfxVolumeSlider.value = Mathf.InverseLerp(-80f, 0, sfxVolume);

        fullscreen.value = (int)Screen.fullScreenMode;
        resolution.value = gameSettings.Resolution;

        qualityLevel.value = gameSettings.QualityLevel;
        vsync.value = QualitySettings.vSyncCount;
        antialiasing.value = gameSettings.AntiAliasing;
        shadowQuality.value = (int)QualitySettings.shadowResolution;

        fullscreen.onValueChanged.AddListener(gameSettings.ChangeFullScreenMode);
        resolution.onValueChanged.AddListener(gameSettings.ChangeResolution);
        antialiasing.onValueChanged.AddListener(gameSettings.ChangeAntiAliasing);
        qualityLevel.onValueChanged.AddListener(gameSettings.ChangeQualityLevel);
        shadowQuality.onValueChanged.AddListener(gameSettings.ChangeShadowQuality);
        
        masterVolumeSlider.onValueChanged.AddListener(gameSettings.MasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(gameSettings.MusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(gameSettings.SfxVolumeChanged);

        playerNameInput.onSubmit.AddListener(OnPlayerNameChanged);
        
    }

    private void OnPlayerNameChanged(string playerName) {
        UserManager.playerData.PlayerName = playerName;
    }

    public void Open() {
        gameObject.SetActive(true);
    }

    public void Close() {
        gameObject.SetActive(false);
        gameSettings.Save();
    }

    public void OnClose() {
        PanelManager.Instance.HidePanel();
    }

    public void Logout() {
        AuthenticationManager.Instance.Logout();
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
