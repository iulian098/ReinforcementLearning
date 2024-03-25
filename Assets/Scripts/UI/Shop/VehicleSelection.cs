using System;
using UnityEngine;

public class VehicleSelection : MonoBehaviour
{
    [SerializeField] VehicleShopItem shopItemPrefab;
    [SerializeField] VehiclesContainer vehiclesContainer;
    [SerializeField] Transform itemsContainer;
    [SerializeField] Transform previewContainer;

    VehicleShopItem[] spawnedItems;

    VehicleShopItem selectedItem;
    GameObject vehiclePreview;

    private void Start() {
        spawnedItems = new VehicleShopItem[vehiclesContainer.Vehicles.Length];

        for (int i = 0; i < vehiclesContainer.Vehicles.Length; i++) {
            VehicleShopItem item = Instantiate(shopItemPrefab, itemsContainer);
            spawnedItems[i] = item;
            item.Init(vehiclesContainer.Vehicles[i]);
            item.OnSelected += OnVehicleSelected;
        }
    }

    private void OnVehicleSelected(VehicleShopItem item) {
        if (selectedItem != null) selectedItem.IsSelected = false;
        selectedItem = item;
        selectedItem.IsSelected = true;

        if (vehiclePreview != null)
            Destroy(vehiclePreview);

        vehiclePreview = Instantiate(item.Config.PreviewPrefab, previewContainer);
        vehiclePreview.transform.localScale = Vector3.one;
    }
}
