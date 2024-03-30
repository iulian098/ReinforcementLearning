using System.Collections.Generic;

public class SaveFile : SaveFileBase
{
    public PlayerData playerData;
    public List<VehicleSaveData> vehicleSaveData;

    public SaveFile() {
        vehicleSaveData = new List<VehicleSaveData>();
        playerData = new PlayerData();
    }
}
