using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    const string MASTER_VOLUME = "MasterVolume";
    const string MUSIC_VOLUME = "MusicVolume";
    const string SFX_VOLUME = "SFXVolume";

    [Header("Audio")]
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider masterVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider sfxVolumeSlider;

    [Space, Header("Graphics")]
    [SerializeField] TMP_Dropdown qualityLevel;
    [SerializeField] Slider renderScale;
    [SerializeField] TMP_Dropdown vsync;
    [SerializeField] TMP_Dropdown antialiasing;
    [SerializeField] TMP_Dropdown shadowQuality;

    public void Start() {
        audioMixer.GetFloat(MASTER_VOLUME, out float masterVolume);
        audioMixer.GetFloat(MUSIC_VOLUME, out float musicVolume);
        audioMixer.GetFloat(SFX_VOLUME, out float sfxVolume);

        masterVolumeSlider.value = Mathf.InverseLerp(-80f, 20f, masterVolume);
        musicVolumeSlider.value = Mathf.InverseLerp(-80f, 20f, musicVolume);
        sfxVolumeSlider.value = Mathf.InverseLerp(-80f, 20f, sfxVolume);


        qualityLevel.value = QualitySettings.GetQualityLevel();
        vsync.value = QualitySettings.vSyncCount;
        antialiasing.value = QualitySettings.antiAliasing;
        shadowQuality.value = (int)QualitySettings.shadowResolution;

        masterVolumeSlider.onValueChanged.AddListener(MasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(MusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(SfxVolumeChanged);

        qualityLevel.onValueChanged.AddListener(OnQualityLevelChange);
        shadowQuality.onValueChanged.AddListener(ChangeShadowQuality);
    }

    public void MasterVolumeChanged(float val) {
        audioMixer.SetFloat(MASTER_VOLUME, Mathf.Lerp(-80, 20, val));
    }

    public void MusicVolumeChanged(float val) {
        audioMixer.SetFloat(MUSIC_VOLUME, Mathf.Lerp(-80, 20, val));
    }

    public void SfxVolumeChanged(float val) {
        audioMixer.SetFloat(SFX_VOLUME, Mathf.Lerp(-80, 20, val));
    }

    public void ChangeShadowQuality(int val) {
        
    }

    public void OnQualityLevelChange(int val) {
        QualitySettings.SetQualityLevel(val);
    }
}
