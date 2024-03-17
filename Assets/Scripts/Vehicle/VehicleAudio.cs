using System;
using UnityEngine;

public class VehicleAudio : MonoBehaviour
{
    [Serializable]
    public class AudioSettings {
        public float minDistance;
        public float maxDistance;
        public AudioRolloffMode rolloffMode;
    }

    [Serializable]
    public class EngineSoundData {
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

    [Serializable]
    public class AudioData {
        [SerializeField] AudioSettings settings;
        public AudioClip clip;
        public bool loop;
        public bool randomPitch;
        public float minPitch;
        public float maxPitch;
        public float volume;

        AudioSource audioSource;

        public AudioSource AudioSource => audioSource;

        public void SetAudioSource(AudioSource source) {
            audioSource = source;
            audioSource.maxDistance = settings.maxDistance;
            audioSource.minDistance = settings.minDistance;
        }

        public void PlayOnce() {
            if (randomPitch)
                audioSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(clip);
        }

    }

    [SerializeField] Vehicle vehicle;
    [SerializeField] EngineSoundData lowSounds;
    [SerializeField] EngineSoundData highSounds;
    [SerializeField] AudioData hitSound;
    [SerializeField] AudioSettings engineAudioSettings;

    float pitch;
    float currentAcc;

    private void Start() {
        CreateEngineAudioSource(lowSounds);
        CreateEngineAudioSource(highSounds);
        CreateAudioSource(hitSound);
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
            audioSource.maxDistance = engineAudioSettings.maxDistance;
            audioSource.minDistance = engineAudioSettings.minDistance;
            audioSource.Play();
            audioSources[i] = audioSource;
        }
        engineSound.SetAudioSource(audioSources[0], audioSources[1]);
    }

    void CreateAudioSource(AudioData audioData) {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioData.clip;
        audioSource.volume = audioData.volume;
        audioSource.loop = true;
        audioSource.volume = 0;
        audioSource.spatialBlend = 1;
        audioSource.rolloffMode = engineAudioSettings.rolloffMode;
        audioSource.maxDistance = engineAudioSettings.maxDistance;
        audioSource.minDistance = engineAudioSettings.minDistance;
        audioSource.Play();
        audioData.SetAudioSource(audioSource);
    }

    public float ULerp(float from, float to, float value) {
        return (1f - value) * from + to * value;
    }

    private void OnCollisionEnter(Collision collision) {
        if (vehicle.Kmph > 15) {
            hitSound.AudioSource.volume = Mathf.InverseLerp(0.05f, 0.4f, vehicle.Kmph / 30);
            hitSound.PlayOnce();
        }
    }
}
