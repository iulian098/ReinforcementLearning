using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSystem : MonoBehaviour
{
    [SerializeField] AudioSource musicAudioSource;
    [SerializeField] AudioClip musicClip;
    private void Start() {
        AudioManager.Instance.OnAudioManagerInitialized += Init;
        DontDestroyOnLoad(gameObject);
    }

    private void Init() {
        musicAudioSource.outputAudioMixerGroup = AudioManager.Instance.Mixer.FindMatchingGroups("Master/Music")[0];
        musicAudioSource.clip = musicClip;
        musicAudioSource.Play();
        
    }
}
