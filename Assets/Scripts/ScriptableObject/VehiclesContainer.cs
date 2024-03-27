using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VehiclesContainer", menuName = "ScriptableObjects/Vehicles Container")]
public class VehiclesContainer : ScriptableObject
{
    [SerializeField] VehicleConfig[] vehicles;
    public List<VehicleSaveData> vehicleSaveDatas;
    public int selectedVehicle = 0;

    public VehicleConfig[] Vehicles => vehicles;
    
    public VehicleSaveData GetSaveData(int vehicleIndex) {
        VehicleSaveData saveData = vehicleSaveDatas.Find(x => x.vehicleIndex == vehicleIndex);

        if (saveData == null) saveData = new VehicleSaveData(vehicleIndex);

        return saveData;
    }

    public VehicleConfig GetEquippedVehicle() {
        return vehicles[selectedVehicle];
    }

    public VehicleSaveData GetSaveData(VehicleConfig vehicleConfig) {
        int configIndex = System.Array.IndexOf(vehicles, vehicleConfig);
        return vehicleSaveDatas.Find(x => x.vehicleIndex ==  configIndex);
    }
}
