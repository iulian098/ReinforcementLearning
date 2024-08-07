using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VehiclesContainer", menuName = "ScriptableObjects/Vehicles Container")]
public class VehiclesContainer : ScriptableObject
{
    [SerializeField] VehicleConfig[] vehicles;
    public List<VehicleSaveData> vehicleSaveDatas;
    public int selectedVehicle = 0;

    public VehicleConfig[] Vehicles => vehicles;
    
    public void SetSaveData(List<VehicleSaveData> vehicleSaveDatas) {
        if (vehicleSaveDatas.IsNullOrEmpty()) {
            UserManager.playerData.SetInt(PlayerPrefsStrings.SELECTED_VEHICLE, 0);
            for (int i = 0; i < vehicles.Length; i++)
                vehicleSaveDatas.Add(CreateSaveData(i));
        }
        else if (vehicleSaveDatas.Count != vehicles.Length) {
            for (int i = vehicleSaveDatas.Count; i < vehicles.Length; i++)
                vehicleSaveDatas.Add(CreateSaveData(i));
        }

        for (int i = 0; i < vehicles.Length; i++)
            vehicles[i].saveData = vehicleSaveDatas[i];

        this.vehicleSaveDatas = vehicleSaveDatas;
    }

    VehicleSaveData CreateSaveData(int index) {
        VehicleSaveData saveData = new VehicleSaveData(index);

        for (int i = 0; i < Enum.GetNames(typeof(UpgradeType)).Length; i++) {
            saveData.EquippedLevels.Set((UpgradeType)i, -1);
            saveData.PurchasedUpgrades.Set((UpgradeType)i, new List<int>());
        }

        if (index == 0)
            saveData.purchased = true;

        return saveData;
    }

    public VehicleConfig GetEquippedVehicle() {
        return vehicles[UserManager.playerData.GetInt(PlayerPrefsStrings.SELECTED_VEHICLE)];
    }

    public VehicleSaveData GetSaveData(int vehicleIndex) {
        VehicleSaveData saveData = vehicleSaveDatas.Find(x => x.vehicleIndex == vehicleIndex);

        if (saveData == null) saveData = new VehicleSaveData(vehicleIndex);

        return saveData;
    }
}
