using UnityEngine;

public class VehicleStats : MonoBehaviour
{
    [SerializeField] StatsItem acceleration;
    [SerializeField] StatsItem speed;
    [SerializeField] StatsItem nos;
    [SerializeField] StatsItem handling;

    public void UpdateValues(VehicleConfig currentConfig, VehicleConfig newConfig) {

        if (newConfig == null)
            newConfig = currentConfig;

        acceleration.SetValue(currentConfig.AccelerationForce / 2, newConfig.AccelerationForce / 2);
        speed.SetValue(currentConfig.MaxSpeed / 300, newConfig.MaxSpeed / 300);
        nos.SetValue(currentConfig.NosPowerMultiplier / 3, newConfig.NosPowerMultiplier / 3);
        handling.SetValue(currentConfig.WheelHandlingBonus[0] / 2, newConfig.WheelHandlingBonus[0] / 2);
    }
}
