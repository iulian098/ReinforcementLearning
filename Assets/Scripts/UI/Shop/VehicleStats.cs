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

    public void UpdateValues(VehicleConfig config, UpgradeType upgradeType, int upgradeLevel) {
        acceleration.SetValue(GetAcc(config));
        speed.SetValue(GetMaxSpeed(config));
        nos.SetValue(GetNosPower(config));
        handling.SetValue(GetHandling(config));
        switch (upgradeType) {
            case UpgradeType.Engine:
                speed.SetValue(GetMaxSpeed(config), GetMaxSpeed(config, upgradeLevel));
                break;
            case UpgradeType.Acceleration:
                acceleration.SetValue(GetAcc(config), GetAcc(config, upgradeLevel));
                break;
            case UpgradeType.Nos:
                nos.SetValue(GetNosPower(config), GetNosPower(config, upgradeLevel));
                break;
            case UpgradeType.Handling:
                handling.SetValue(GetHandling(config), GetHandling(config, upgradeLevel));
                break;

        }
    }

    float GetAcc(VehicleConfig config, int level = -1) {
        float val = ((config.AccelerationForce * config.EnginePower) + config.EnginePower) / 2000;

        if(level != -1)
            return config.GetUpgradeValue(UpgradeType.Acceleration, val, level);
        return config.GetUpgradeValue(UpgradeType.Acceleration, val);
    }

    float GetMaxSpeed(VehicleConfig config, int level = -1) {
        float val = (config.MaxSpeed) / 300;
        if (level != -1)
            return config.GetUpgradeValue(UpgradeType.Engine, val, level);
        return config.GetUpgradeValue(UpgradeType.Engine, val);
    }

    float GetNosPower(VehicleConfig config, int level = -1) {
        float val = (config.NosPowerMultiplier + config.NosAmount) / 20;
        if (level != -1)
            return config.GetUpgradeValue(UpgradeType.Nos, val, level);
        return config.GetUpgradeValue(UpgradeType.Nos, val);
    }
    
    float GetHandling(VehicleConfig config, int level = -1) {
        WheelCollider wheel = config.Prefab.GetComponentInChildren<WheelCollider>();
        float total = wheel.sidewaysFriction.extremumSlip + wheel.sidewaysFriction.asymptoteSlip + wheel.forwardFriction.extremumSlip + wheel.forwardFriction.asymptoteSlip;
        float val = total / 4;
        if (level != -1)
            return config.GetUpgradeValue(UpgradeType.Handling, val, level);
        return config.GetUpgradeValue(UpgradeType.Handling, val);
    }
}
