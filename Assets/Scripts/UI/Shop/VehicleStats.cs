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

        acceleration.SetValue(GetAcc(currentConfig), GetAcc(newConfig));
        speed.SetValue(GetMaxSpeed(currentConfig), GetMaxSpeed(newConfig));
        nos.SetValue(GetNosPower(currentConfig), GetNosPower(newConfig));
        handling.SetValue(GetHandling(currentConfig), GetHandling(newConfig));
    }

    float GetAcc(VehicleConfig config) {
        return ((config.AccelerationForce * config.EnginePower) + config.EnginePower) / 2000;
    }

    float GetMaxSpeed(VehicleConfig config) {
        return (config.MaxSpeed) / 300;
    }

    float GetNosPower(VehicleConfig config) {
        return (config.NosPowerMultiplier + config.NosAmount) / 20;
    }
    
    float GetHandling(VehicleConfig config) {
        WheelCollider wheel = config.Prefab.GetComponentInChildren<WheelCollider>();
        float total = wheel.sidewaysFriction.extremumSlip + wheel.sidewaysFriction.asymptoteSlip + wheel.forwardFriction.extremumSlip + wheel.forwardFriction.asymptoteSlip;
        return total / 4;
    }
}
