using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

public class AudioManager : MonoSingleton<AudioManager>
{
    [SerializeField] AssetReferenceT<AudioMixer> mixerReference;

    AudioMixer mixer;

    public AudioMixer Mixer => mixer;

    private async void Start() {
        mixer = await AssetsManager<AudioMixer>.Load(mixerReference);
    }
}
