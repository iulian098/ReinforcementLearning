using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[Serializable]
public class VehicleSaveData
{
    public int vehicleIndex;
    public Dictionary<UpgradeType, int> equippedLevels = new Dictionary<UpgradeType, int>();
    public Dictionary<UpgradeType, List<int>> purchasedUpgrades = new Dictionary<UpgradeType, List<int>>();

    public Dictionary<UpgradeType, List<int>> PurchasedUpgrades {
        get {
            if (purchasedUpgrades == null)
                purchasedUpgrades = new Dictionary<UpgradeType, List<int>>();
            return purchasedUpgrades;
        }
    }

    public Dictionary<UpgradeType, int> EquippedLevels {
        get {
            if(equippedLevels == null)
                equippedLevels= new Dictionary<UpgradeType, int>();
            return equippedLevels;
        }
    }

    public VehicleSaveData(int vehicleIndex) {
        this.vehicleIndex = vehicleIndex;

        for (int i = 0; i < Enum.GetNames(typeof(UpgradeType)).Length; i++) {
            EquippedLevels.Add((UpgradeType)i, -1);
            PurchasedUpgrades.Add((UpgradeType)i, new List<int>());
        }
    }

    public void Randomize(VehicleConfig config) {
        for (int i = 0; i < Enum.GetNames(typeof(UpgradeType)).Length; i++) {
            UpgradeData data = Array.Find(config.Upgrades, x => x.upgradeType == (UpgradeType)i);
            EquippedLevels.Add((UpgradeType)i, Random.Range(0, data.val.Length));
        }
    }
}
