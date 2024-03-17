using System;
using UnityEngine;

public class VehicleAudio : MonoBehaviour
{
    [Serializable]
    public class AudioSettings {
        public float engineMinDistance;
        public float engineMaxDistance;
        public AudioRolloffMode rolloffMode;
    }

    [Serializable]
    public class EngineSoundData {
        [SerializeField] string name;
        [SerializeField] AudioClip on;
        [SerializeField] AudioClip off;

        public float minPitch;
        public float maxPitch;
        public float maxVolume;

        AudioSource accSource;
        AudioSource decSource;

        public AudioSource AccSource => accSource;
        public AudioSource DecSource => decSource;

        public void SetAudioSource(AudioSource acc, AudioSource dec) {
            accSource = acc;
            decSource = dec;
        }

        public AudioClip GetAudioClip(bool acc) {
            return acc ? on : off;
        }
    }

    [SerializeField] Vehicle vehicle;
    [SerializeField] EngineSoundData[] engineSounds;
    [SerializeField] EngineSoundData lowSounds;
    [SerializeField] EngineSoundData highSounds;
    [SerializeField] AudioSettings engineAudioSettings;

    float pitch;
    float currentAcc;

    private void Start() {
        CreateEngineAudioSource(lowSounds);
        CreateEngineAudioSource(highSounds);
    }

    private void Update() {
        pitch = ULerp(lowSounds.minPitch, lowSounds.maxPitch, vehicle.EngineRPM / vehicle.MaxRPM);
        currentAcc = Mathf.Lerp(currentAcc, vehicle.Braking ? 0 : Mathf.Abs(vehicle.CurrentInput.acceleration), Time.deltaTime * 30);

        lowSounds.AccSource.pitch = pitch;
        lowSounds.DecSource.pitch = pitch;
        highSounds.AccSource.pitch = pitch;
        highSounds.DecSource.pitch = pitch;

        float accFade = currentAcc;
        float decFade = 1 - accFade;

        float highFade = Mathf.InverseLerp(0.2f, 0.8f, vehicle.EngineRPM / vehicle.MaxRPM);
        float lowFade = 1 - highFade;

        highFade = 1 - ((1 - highFade) * (1 - highFade));
        lowFade = 1 - ((1 - lowFade) * (1 - lowFade));
        accFade = 1 - ((1 - accFade) * (1 - accFade));
        decFade = 1 - ((1 - decFade) * (1 - decFade));

        lowSounds.AccSource.volume = Mathf.Min(lowSounds.maxVolume, lowFade * accFade);
        lowSounds.DecSource.volume = Mathf.Min(lowSounds.maxVolume, lowFade * decFade);
        highSounds.AccSource.volume = Mathf.Min(highSounds.maxVolume, highFade * accFade);
        highSounds.DecSource.volume = Mathf.Min(highSounds.maxVolume, highFade * decFade);
    }

    void CreateEngineAudioSource(EngineSoundData engineSound) {
        AudioSource[] audioSources = new AudioSource[2];
        for (int i = 0; i < 2; i++) {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = engineSound.GetAudioClip(i == 0);
            audioSource.volume = 1.0f;
            audioSource.pitch = 1.0f;
            audioSource.loop = true;
            audioSource.volume = 0;
            audioSource.spatialBlend = 1;
            audioSource.rolloffMode = engineAudioSettings.rolloffMode;
            audioSource.maxDistance = engineAudioSettings.engineMaxDistance;
            audioSource.minDistance = engineAudioSettings.engineMinDistance;
            audioSource.Play();
            audioSources[i] = audioSource;
        }
        engineSound.SetAudioSource(audioSources[0], audioSources[1]);
    }

    public float ULerp(float from, float to, float value) {
        return (1f - value) * from + to * value;
    }
}
