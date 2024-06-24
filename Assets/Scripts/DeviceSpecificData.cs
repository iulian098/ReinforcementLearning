using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DeviceSpecificData : MonoBehaviour
{
    [SerializeField] VolumeProfile desktopProfile;
    [SerializeField] VolumeProfile mobileProfile;
    [SerializeField] Volume volume;

    void Awake()
    {
#if UNITY_ANDROID
        volume.profile = mobileProfile;
#else
        volume.profile = desktopProfile;
#endif

    }
}
