using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewVehicleConfig", menuName = "ScriptableObjects/New Vehicle Config")]
public class VehicleConfig : ScriptableObject
{
    [SerializeField] string vehicleName;
    [SerializeField] VehicleManager prefab;
    [SerializeField] GameObject previewPrefab;
    [SerializeField] int price;
    [SerializeField] int unlockLevel;

    [Space]
    [SerializeField] float maxSpeed = 185;
    [SerializeField] float maxReverseSpeed = 20;
    [SerializeField] float lowSpeedSteerRadius;
    [SerializeField] float highSpeedSteerRadius;
    [SerializeField] float downForce;

    [Space, Header("Torque")]
    [SerializeField] float maxReverseTorque = 200;
    [SerializeField] float brakeTorque = 500;
    [SerializeField] float handbrakeTorque = 1000;
    [SerializeField] AnimationCurve enginePowerCurve;
    [SerializeField] float enginePowerMultiplier;

    [Header("Acceleration")]
    [SerializeField] float accelerationForce;

    [Space, Header("Gears")]
    [SerializeField] float[] gears;
    [SerializeField] float totalRPM;
    [SerializeField] float shiftUpRPM = 5600;
    [SerializeField] float shiftDownRPM = 2500;
    [SerializeField] float differentialRatio = 3.6f;

    [Space, Header("ABS")]
    [SerializeField] bool useABS;
    [SerializeField] float absThreshold = 0.2f;
    [SerializeField] float absBrakeFactor = 0.5f;

    [Space, Header("TCS")]
    [SerializeField] bool useTCS;
    [SerializeField] float tcsThreshold = 0.8f;
    [SerializeField] float tcsFactor = 0.5f;

    [Space, Header("NOS")]
    [SerializeField] float nosPowerMultiplier;
    [SerializeField] float nosAmount;

    [Space]
    [SerializeField] UpgradeData[] upgrades;

    public VehicleSaveData saveData;

    public string VehicleName => vehicleName;

    public VehicleManager Prefab => prefab;
    public GameObject PreviewPrefab => previewPrefab;
    public int Price => price;
    public int UnlockLevel => unlockLevel;

    public float MaxSpeed => maxSpeed;
    public float MaxReverseSpeed => maxReverseSpeed;
    public float LowSpeedSteerRadius => lowSpeedSteerRadius;
    public float HighSpeedSteerRadius => highSpeedSteerRadius;
    public float DownForce => downForce;
    public float MaxReverseTorque => maxReverseSpeed;
    public float BrakeTorque => brakeTorque;
    public float HandbrakeTorque => handbrakeTorque;
    public AnimationCurve EnginePowerCurve => enginePowerCurve;
    public float EnginePower => enginePowerMultiplier;
    public float AccelerationForce => accelerationForce;
    public float[] Gears => gears;
    public float TotalRPM => totalRPM;
    public float ShiftUpRPM => shiftUpRPM;
    public float ShiftDownRPM => shiftDownRPM;
    public float DifferentialRatio => differentialRatio;

    public bool UseABS => useABS;
    public float AbsThreshold => absThreshold;
    public float AbsBrakeFactor => absBrakeFactor;

    public bool UseTCS => useTCS;
    public float TcsThreshold => tcsThreshold;
    public float TcsFactor => tcsFactor;

    public float NosPowerMultiplier => nosPowerMultiplier;
    public float NosAmount => nosAmount;

    public UpgradeData[] Upgrades => upgrades;

    public float GetUpgradeValue(UpgradeType type, float currentValue, int level) {
        if (level == -1)
            return currentValue;

        UpgradeData upgradeData = Array.Find(upgrades, x => x.upgradeType == type);
        return currentValue + upgradeData.val[level] * currentValue;
    }

    public float GetUpgradeValue(UpgradeType type, float currentValue) {
        if(!saveData.EquippedLevels.TryGetValue(type, out int level))
            return currentValue;

        UpgradeData upgradeData = Array.Find(upgrades, x => x.upgradeType == type);
        return currentValue + upgradeData.val[level] * currentValue;
    }
}
