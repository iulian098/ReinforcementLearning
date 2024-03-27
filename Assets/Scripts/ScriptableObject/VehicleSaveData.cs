[System.Serializable]
public class VehicleSaveData
{
    public int vehicleIndex;
    public int engineLevel;
    public int accelerationLevel;
    public int nosLevel;
    public int handlingLevel;

    public VehicleSaveData(int vehicleIndex) {
        this.vehicleIndex = vehicleIndex;
    }

    public void Randomize(VehicleConfig config) {
        engineLevel = UnityEngine.Random.Range(0, config.EnginePowerBonus.Length);
        accelerationLevel = UnityEngine.Random.Range(0, config.AccelerationBonus.Length);
        nosLevel = UnityEngine.Random.Range(0, config.NosAmountBonus.Length);
        handlingLevel = UnityEngine.Random.Range(0, config.WheelHandlingBonus.Length);
    }
}
