using System;
using UnityEngine;

public class VehicleSelection : MonoBehaviour
{
    [SerializeField] VehicleStats vehicleStats;
    [SerializeField] VehicleShopItem shopItemPrefab;
    [SerializeField] VehiclesContainer vehiclesContainer;
    [SerializeField] Transform itemsContainer;
    [SerializeField] Transform previewContainer;
    [SerializeField] GameObject buyButton;
    [SerializeField] GameObject equipButton;
    [SerializeField] GameObject upgradeButton;

    [Space, Header("Scrolls")]
    [SerializeField] GameObject vehicleScrollPanel;
    [SerializeField] GameObject upgradesScrollPanel;
    [SerializeField] GameObject levelsScrollPanel;

    VehicleShopItem[] spawnedItems;

    GameObject vehiclePreview;

    VehicleConfig selectedVehicleConfig;
    VehicleConfig equippedVehicleConfig;
    VehicleSaveData selectedSaveData;
    VehicleShopItem selectedItem;

    private void Start() {
        spawnedItems = new VehicleShopItem[vehiclesContainer.Vehicles.Length];
        equippedVehicleConfig = vehiclesContainer.GetEquippedVehicle();

        for (int i = 0; i < vehiclesContainer.Vehicles.Length; i++) {
            VehicleShopItem item = Instantiate(shopItemPrefab, itemsContainer);
            spawnedItems[i] = item;
            item.Init(vehiclesContainer.Vehicles[i]);
            item.OnSelected += OnVehicleSelected;
            if (i == vehiclesContainer.selectedVehicle) {
                OnVehicleSelected(item);
            }
        }

    }

    private void OnVehicleSelected(VehicleShopItem item) {
        if (selectedItem != null) 
            selectedItem.IsSelected = false;

        selectedItem = item;
        selectedItem.IsSelected = true;

        if (vehiclePreview != null)
            Destroy(vehiclePreview);

        vehiclePreview = Instantiate(item.Config.PreviewPrefab, previewContainer);
        vehiclePreview.transform.localScale = Vector3.one;
        selectedSaveData = vehiclesContainer.vehicleSaveDatas.Find(x => x.vehicleIndex == Array.IndexOf(vehiclesContainer.Vehicles, item.Config));
        selectedVehicleConfig = item.Config;
        UpdateUI();

        vehicleStats.UpdateValues(equippedVehicleConfig, selectedVehicleConfig);
    }

    void UpdateUI() {
        buyButton.SetActive(selectedSaveData == null);
        equipButton.SetActive(selectedSaveData != null && vehiclesContainer.GetEquippedVehicle() != selectedItem.Config);
        upgradeButton.SetActive(selectedSaveData != null);
    }

    public void OnBuyVehicle() {
        if (selectedItem == null) return;
        int vehicleIndex = Array.IndexOf(vehiclesContainer.Vehicles, selectedItem.Config);
        if(vehicleIndex == -1) {
            Debug.LogError("Vehicle not found");
            return;
        }

        VehicleSaveData newSaveData = new VehicleSaveData(vehicleIndex);

        vehiclesContainer.vehicleSaveDatas.Add(newSaveData);
        selectedSaveData = newSaveData;
        UpdateUI();
    }

    public void OnEquipVehicle() {
        if(selectedItem == null) return;

        equippedVehicleConfig = selectedItem.Config;
        vehiclesContainer.selectedVehicle = Array.IndexOf(vehiclesContainer.Vehicles, selectedItem.Config);
        UpdateUI();
    }

    public void OnUpgradeVehicle() {
        throw new NotImplementedException();
    }
}
