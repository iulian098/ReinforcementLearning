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
            for (int i = 0; i < vehicles.Length; i++)
                vehicleSaveDatas.Add(CreateSaveData(i));
        }
        else if (this.vehicleSaveDatas.Count != vehicleSaveDatas.Count) {
            for (int i = vehicleSaveDatas.Count; i < vehicles.Length; i++)
                vehicleSaveDatas.Add(CreateSaveData(i));
        }

        this.vehicleSaveDatas = vehicleSaveDatas;
    }

    VehicleSaveData CreateSaveData(int index) {
        VehicleSaveData saveData = new VehicleSaveData(index);

        for (int i = 0; i < Enum.GetNames(typeof(UpgradeType)).Length; i++) {
            saveData.EquippedLevels.Set((UpgradeType)i, -1);
            saveData.PurchasedUpgrades.Set((UpgradeType)i, new List<int>());
        }

        return saveData;
    }

    public VehicleConfig GetEquippedVehicle() {
        return vehicles[selectedVehicle];
    }

    public VehicleSaveData GetSaveData(int vehicleIndex) {
        VehicleSaveData saveData = vehicleSaveDatas.Find(x => x.vehicleIndex == vehicleIndex);

        if (saveData == null) saveData = new VehicleSaveData(vehicleIndex);

        return saveData;
    }
}
