using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

public class AudioManager : MonoSingleton<AudioManager>
{
    [SerializeField] AssetReferenceT<AudioMixer> mixerReference;
    [SerializeField] AudioDataContainer audioDataContainer;
    [SerializeField] AudioSource commonAudioSource;

    public Action OnAudioManagerInitialized;

    AudioMixer mixer;

    public AudioMixer Mixer => mixer;
    public AudioDataContainer AudioDataContainer => audioDataContainer;

    private async void Start() {
        mixer = await AssetsManager<AudioMixer>.Load(mixerReference);
        OnAudioManagerInitialized?.Invoke();
    }

    public void PlayCommonAudio(AudioClip clip, bool loop) {
        PlayCommonAudio(clip, loop, 1, 1);
    }

    public void PlayCommonAudio(AudioClip clip, bool loop = false, float minPitch = 1, float maxPitch = 1) {
        GameObject go = new GameObject();
        AudioSource audioSource = go.AddComponent<AudioSource>();
        audioSource.playOnAwake = true;
        audioSource.loop = loop;
        audioSource.clip = clip;
        audioSource.spatialBlend = 0;
        audioSource.volume = 1;
        audioSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Master/SFX")[0];
        audioSource.Play();
        Destroy(go, clip.length);
    }
}
