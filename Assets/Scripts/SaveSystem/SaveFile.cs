using System.Collections.Generic;

public class SaveFile
{
    public PlayerData playerData;
    public List<VehicleSaveData> vehicleSaveData;
    public List<TrackSaveData> tracksSaveData;

    public SaveFile() {
        playerData = new PlayerData();
        vehicleSaveData = new List<VehicleSaveData>();
        tracksSaveData = new List<TrackSaveData>();
    }
}
