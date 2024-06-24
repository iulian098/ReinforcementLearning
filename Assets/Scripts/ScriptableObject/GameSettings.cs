using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings")]
public class GameSettings : ScriptableObject
{
    const string MASTER_VOLUME = "MasterVolume";
    const string MUSIC_VOLUME = "MusicVolume";
    const string SFX_VOLUME = "SFXVolume";

    [SerializeField] int qualityLevel;
    [SerializeField] int antiAliasing = 0;
    [SerializeField] int shadowResolution;
    [SerializeField] float masterVolume;
    [SerializeField] float musicVolume;
    [SerializeField] float sfxVolume;

    int resolution;
    int fullscreenMode;

    public Action OnQualityLevelChanged;
    public Action OnAntialiasingChanged;
    public Action OnShadowResolutionChanged;
    public Action OnMasterVolumeChanged;
    public Action OnMusicVolumeChanged;
    public Action OnSFXVolumeChanged;

    public int QualityLevel => qualityLevel;
    public int AntiAliasing => antiAliasing;    
    public int ShadowResolution => shadowResolution;
    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;
    public int Resolution => resolution;
    public int FullscreenMode => fullscreenMode;

    public void Setup() {
        QualitySettings.SetQualityLevel(qualityLevel, true);
        QualitySettings.antiAliasing = antiAliasing;
    }

    public async void Load(bool useSavefile) {
        UniversalRenderPipelineAsset asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        int currentResolutionIndex = Array.IndexOf(Screen.resolutions, Screen.currentResolution);

        qualityLevel = PlayerPrefs.GetInt("Quality_Level", -1);
        antiAliasing = PlayerPrefs.GetInt("Antialiasing", -1);
        shadowResolution = PlayerPrefs.GetInt("ShadowResolution", -1);
        masterVolume = PlayerPrefs.GetFloat("Master_Volume", -1);
        musicVolume = PlayerPrefs.GetFloat("Music_Volume", -1);
        sfxVolume = PlayerPrefs.GetFloat("SFX_Volume", -1);

#if !UNITY_ANDROID
        resolution = PlayerPrefs.GetInt("Resolution", currentResolutionIndex);
        fullscreenMode = PlayerPrefs.GetInt("FullscreenMode", (int)Screen.fullScreenMode);
#endif
        while(AudioManager.Instance == null || AudioManager.Instance.Mixer == null) {
            await Task.Delay(100);
        }

        if(qualityLevel == -1 && useSavefile) {
            qualityLevel = QualitySettings.GetQualityLevel();
            switch ((MsaaQuality)asset.msaaSampleCount) {
                case MsaaQuality.Disabled:
                    antiAliasing = 0;
                    break;
                case MsaaQuality._2x:
                    antiAliasing = 1;
                    break;
                case MsaaQuality._4x:
                    antiAliasing = 2;
                    break;
                case MsaaQuality._8x:
                    antiAliasing = 3;
                    break;
                default:
                    break;
            }

            AudioManager.Instance.Mixer.GetFloat(MASTER_VOLUME, out float masterVolume);
            AudioManager.Instance.Mixer.GetFloat(MUSIC_VOLUME, out float musicVolume);
            AudioManager.Instance.Mixer.GetFloat(SFX_VOLUME, out float sfxVolume);

            this.masterVolume = Mathf.InverseLerp(-80f, 20f, masterVolume);
            this.musicVolume = Mathf.InverseLerp(-80f, 20f, musicVolume);
            this.sfxVolume = Mathf.InverseLerp(-80f, 20f, sfxVolume);

            resolution = currentResolutionIndex;
        }
        else {
            MasterVolumeChanged(masterVolume);
            MusicVolumeChanged(musicVolume);
            SfxVolumeChanged(sfxVolume);
            ChangeQualityLevel(qualityLevel);
            //ChangeAntiAliasing(antiAliasing);
#if !UNITY_ANDROID
            ChangeResolution(resolution);
            ChangeFullScreenMode(fullscreenMode);
#endif
        }
    }

    public void Save() {
        PlayerPrefs.SetInt("Resolution", resolution);
        PlayerPrefs.SetInt("FullscreenMode", fullscreenMode);
        PlayerPrefs.SetInt("Quality_Level", qualityLevel);
        PlayerPrefs.SetInt("Antialiasing", antiAliasing);
        PlayerPrefs.SetInt("ShadowResolution", shadowResolution);
        PlayerPrefs.SetFloat("Master_Volume", masterVolume);
        PlayerPrefs.SetFloat("Music_Volume", musicVolume);
        PlayerPrefs.SetFloat("SFX_Volume", sfxVolume);

    }

    public void MasterVolumeChanged(float val) {
        AudioManager.Instance.Mixer.SetFloat(MASTER_VOLUME, Mathf.Lerp(-80, 0, val));
        masterVolume = val;
    }

    public void MusicVolumeChanged(float val) {
        AudioManager.Instance.Mixer.SetFloat(MUSIC_VOLUME, Mathf.Lerp(-80, 0, val));
        OnMusicVolumeChanged?.Invoke();
        musicVolume = val;
    }

    public void SfxVolumeChanged(float val) {
        AudioManager.Instance.Mixer.SetFloat(SFX_VOLUME, Mathf.Lerp(-80, 0, val));
        sfxVolume = val;
    }

    public void ChangeAntiAliasing(int val) {
        ChangeQualityLevel(3);
        UniversalRenderPipelineAsset asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        switch (val) {
            case 0:
                asset.msaaSampleCount = (int)MsaaQuality.Disabled;
                break;
            case 1:
                asset.msaaSampleCount = (int)MsaaQuality._2x;
                break;
            case 2:
                asset.msaaSampleCount = (int)MsaaQuality._4x;
                break;
            case 3:
                asset.msaaSampleCount = (int)MsaaQuality._8x;
                break;
        }
        antiAliasing = val;
    }

    public void ChangeShadowQuality(int val) {
        ChangeQualityLevel(3);
        UniversalRenderPipelineAsset asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        var assetType = asset.GetType();
        FieldInfo shadowInfo = assetType.GetField("m_MainLightRenderingMode", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        switch (val) {
            case 0:
                asset.shadowDistance = 50;
                asset.shadowCascadeCount = 1;
                shadowInfo.SetValue(asset, LightRenderingMode.Disabled);
                break;
            case 1:
                asset.shadowDistance = 50;
                asset.shadowCascadeCount = 1;
                shadowInfo.SetValue(asset, LightRenderingMode.PerPixel);
                break;
            case 2:
                asset.shadowDistance = 150;
                asset.shadowCascadeCount = 2;
                shadowInfo.SetValue(asset, LightRenderingMode.PerPixel);
                break;
            case 3:
                asset.shadowDistance = 200;
                asset.shadowCascadeCount = 4;
                shadowInfo.SetValue(asset, LightRenderingMode.PerPixel);
                break;
            default:
                break;
        }
    }

    public void ChangeQualityLevel(int val) {
        QualitySettings.SetQualityLevel(val);
        qualityLevel = val;
    }

    public void ChangeResolution(int val) {
        Resolution[] resolutions = Screen.resolutions;
        if (val > resolutions.Length - 1)
            val = resolutions.Length - 1;
        Screen.SetResolution(resolutions[val].width, resolutions[val].height, (FullScreenMode)fullscreenMode);
        resolution = val;
    }

    public void ChangeFullScreenMode(int val) {
        Screen.fullScreenMode = (FullScreenMode)val;
        fullscreenMode = val;
    }
}
