using System.Collections.Generic;

public class SaveFile : SaveFileBase
{
    public List<VehicleSaveData> vehicleSaveData;

    public SaveFile() {
        vehicleSaveData = new List<VehicleSaveData>();
    }
}
