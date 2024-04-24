using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.ResourceManagement.AsyncOperations;

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

        [Space]
        public bool randomPitch;
        public float minPitch;
        public float maxPitch;

        [Space]
        public float pitch = 1;
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
    [SerializeField] AssetReferenceT<AudioMixer> mixerReference;
    [SerializeField] EngineSoundData lowSounds;
    [SerializeField] EngineSoundData highSounds;
    [SerializeField] AudioData hitSound;
    [SerializeField] AudioData highSpeedWindSound;
    [SerializeField] AudioData skidSound;

    [SerializeField] AudioClip nosIn;
    [SerializeField] AudioClip nosOut;
    [SerializeField] AudioClip nosFlame;
    [SerializeField] AudioData nosLoopSoundSource;
    [SerializeField] AudioData nosMiscSoundSource;

    [SerializeField] AudioSettings engineAudioSettings;

    AudioMixer mixer;
    GameObject parent;
    float pitch;
    float currentAcc;
    bool initialized = false;

    private async void Start() {
        parent = new GameObject("Audio");
        parent.transform.SetParent(transform);
        parent.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        AsyncOperationHandle op = Addressables.LoadAssetAsync<AudioMixer>(mixerReference);
        await op.Task;
        if (op.Result != null)
            mixer = op.Result as AudioMixer;
        CreateEngineAudioSource(lowSounds);
        CreateEngineAudioSource(highSounds);
        CreateAudioSource(hitSound, false);
        CreateAudioSource(highSpeedWindSound, false);
        CreateAudioSource(skidSound, true, true);
        CreateAudioSource(nosLoopSoundSource, false);
        CreateAudioSource(nosMiscSoundSource, false);

        initialized = true;
    }

    private void Update() {
        if (!initialized) return;
        pitch = ULerp(lowSounds.minPitch, lowSounds.maxPitch, vehicle.EngineRPM / vehicle.ShiftUpRPM);
        currentAcc = Mathf.Lerp(currentAcc, vehicle.Braking ? 0 : Mathf.Abs(vehicle.CurrentInput.acceleration), Time.deltaTime * 30);

        lowSounds.AccSource.pitch = pitch;
        lowSounds.DecSource.pitch = pitch;
        highSounds.AccSource.pitch = pitch;
        highSounds.DecSource.pitch = pitch;

        float accFade = currentAcc;
        float decFade = 1 - accFade;

        float highFade = Mathf.InverseLerp(0.2f, 0.8f, vehicle.EngineRPM / vehicle.ShiftUpRPM);
        float lowFade = 1 - highFade;

        highFade = 1 - ((1 - highFade) * (1 - highFade));
        lowFade = 1 - ((1 - lowFade) * (1 - lowFade));
        accFade = 1 - ((1 - accFade) * (1 - accFade));
        decFade = 1 - ((1 - decFade) * (1 - decFade));

        lowSounds.AccSource.volume = Mathf.Min(lowSounds.maxVolume, lowFade * accFade);
        lowSounds.DecSource.volume = Mathf.Min(lowSounds.maxVolume, lowFade * decFade);
        highSounds.AccSource.volume = Mathf.Min(highSounds.maxVolume, highFade * accFade);
        highSounds.DecSource.volume = Mathf.Min(highSounds.maxVolume, highFade * decFade);

        float windVolume = Mathf.Clamp(Mathf.InverseLerp(60, 150, vehicle.Kmph), 0, highSpeedWindSound.volume);
        highSpeedWindSound.AudioSource.volume = windVolume;

        float skidVolume = Mathf.Clamp(Mathf.InverseLerp(10, 75, vehicle.Kmph), 0, skidSound.volume);
        skidSound.AudioSource.volume = vehicle.IsSliding ? skidVolume : 0;

        UpdateNosSound();
    }

    bool nosActive;
    void UpdateNosSound() {
        if (!nosActive && vehicle.NOSActive) {
            nosMiscSoundSource.AudioSource.PlayOneShot(nosIn);
            nosMiscSoundSource.AudioSource.PlayOneShot(nosFlame);
            nosLoopSoundSource.AudioSource.Play();

            nosActive = true;
        }else if(nosActive && !vehicle.NOSActive) {
            nosMiscSoundSource.AudioSource.PlayOneShot(nosOut);
            nosLoopSoundSource.AudioSource.Stop();

            nosActive = false;
        }
    }

    void CreateEngineAudioSource(EngineSoundData engineSound) {
        AudioSource[] audioSources = new AudioSource[2];
        for (int i = 0; i < 2; i++) {
            AudioSource audioSource = parent.AddComponent<AudioSource>();
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
            audioSource.outputAudioMixerGroup = AudioManager.Instance.Mixer.FindMatchingGroups("Master/SFX")[0];
        }
        engineSound.SetAudioSource(audioSources[0], audioSources[1]);
    }

    void CreateAudioSource(AudioData audioData, bool autoPlay = true, bool muteOnCreate = false) {
        AudioSource audioSource = parent.AddComponent<AudioSource>();
        audioSource.clip = audioData.clip;
        audioSource.volume = muteOnCreate ? 0 : audioData.volume;
        audioSource.loop = audioData.loop;
        audioSource.spatialBlend = 1;
        audioSource.pitch = audioData.pitch;
        audioSource.rolloffMode = engineAudioSettings.rolloffMode;
        audioSource.maxDistance = engineAudioSettings.maxDistance;
        audioSource.minDistance = engineAudioSettings.minDistance;
        audioSource.outputAudioMixerGroup = AudioManager.Instance.Mixer.FindMatchingGroups("Master/SFX")[0];

        if (autoPlay)
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
