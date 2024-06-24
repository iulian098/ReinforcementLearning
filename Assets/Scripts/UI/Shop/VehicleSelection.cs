using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VehicleSelection : MonoBehaviour
{
    [SerializeField] VehicleStats vehicleStats;
    [SerializeField] VehicleShopItem shopItemPrefab;
    [SerializeField] VehiclesContainer vehiclesContainer;
    [SerializeField] Transform itemsContainer;
    [SerializeField] Transform previewContainer;
    [SerializeField] PreviewManager previewManager;
    [Space, Header("Scrolls")]
    [SerializeField] GameObject vehicleScrollPanel;
    [SerializeField] GameObject categoryScrollPanel;
    [SerializeField] GameObject levelsScrollPanel;

    [Space, Header("Buttons")]
    [SerializeField] Button buyButton;
    [SerializeField] TMP_Text priceText;
    [SerializeField] Button equipButton;
    [SerializeField] Button upgradeButton;

    [Space, Header("Upgrades")]
    [SerializeField] UpgradeCategory upgradeCategoryPrefab;
    [SerializeField] Transform upgradeCategoryContainer;
    [SerializeField] UpgradeItem upgradeItemPrefab;
    [SerializeField] Transform upgradeItemsContainer;

    VehicleShopItem[] spawnedItems;
    UpgradeCategory[] spawnedCategoryItems;
    UpgradeItem[] spawnedUpgradeLevels;

    VehicleConfig selectedVehicleConfig;
    VehicleConfig equippedVehicleConfig;
    VehicleConfig selectedVehicleToUpgrade;
    VehicleSaveData selectedSaveData;
    VehicleShopItem selectedItem;

    UpgradeData selectedUpgradeData;
    UpgradeItem selectedUpgradeItem;

    int selectedLevelIndex = 0;
    bool inUpgradeMenu = false;

    private void Start() {
        spawnedItems = new VehicleShopItem[vehiclesContainer.Vehicles.Length];
        equippedVehicleConfig = vehiclesContainer.GetEquippedVehicle();

        for (int i = 0; i < vehiclesContainer.Vehicles.Length; i++) {
            VehicleShopItem item = Instantiate(shopItemPrefab, itemsContainer);
            spawnedItems[i] = item;
            item.Init(vehiclesContainer.Vehicles[i]);
            item.OnSelected += OnVehicleSelected;

            if (i == UserManager.playerData.GetInt(PlayerPrefsStrings.SELECTED_VEHICLE))
                OnVehicleSelected(item);
        }

        UpdateEquip();
        equipButton.onClick.AddListener(OnEquipVehicle);
        buyButton.onClick.AddListener(OnBuyVehicle);
    }

    private void OnVehicleSelected(VehicleShopItem item) {
        if (selectedItem != null) 
            selectedItem.IsSelected = false;

        selectedItem = item;
        selectedItem.IsSelected = true;

        previewManager.ShowPreview(item.Config);

        selectedSaveData = vehiclesContainer.vehicleSaveDatas.Find(x => x.vehicleIndex == Array.IndexOf(vehiclesContainer.Vehicles, item.Config));
        selectedVehicleConfig = item.Config;
        UpdateUIButtons();
        vehicleStats.UpdateValues(equippedVehicleConfig, selectedVehicleConfig);
        priceText.text = $"{selectedVehicleConfig.Price}$";
    }

    void UpdateUIButtons() {
        buyButton.gameObject.SetActive(UserManager.playerData.GetInt(PlayerPrefsStrings.LEVEL) >= selectedVehicleConfig.UnlockLevel && selectedSaveData != null && !selectedSaveData.purchased);
        equipButton.gameObject.SetActive(UserManager.playerData.GetInt(PlayerPrefsStrings.LEVEL) >= selectedVehicleConfig.UnlockLevel && selectedSaveData != null && selectedSaveData.purchased && vehiclesContainer.GetEquippedVehicle() != selectedItem.Config);
        upgradeButton.gameObject.SetActive(!inUpgradeMenu && UserManager.playerData.GetInt(PlayerPrefsStrings.LEVEL) >= selectedVehicleConfig.UnlockLevel && selectedSaveData != null && selectedSaveData.purchased);
    }

    void UpdateUpgradeUIButtons() {
    }

    void UpdateEquip() {
        for (int i = 0; i < spawnedItems.Length; i++) {
            if (UserManager.playerData.GetInt(PlayerPrefsStrings.SELECTED_VEHICLE, 0) == i)
                spawnedItems[i].SetState(ShopItemState.Equipped);
            else if(vehiclesContainer.vehicleSaveDatas[i].purchased)
                spawnedItems[i].SetState(ShopItemState.Purchased);
        }
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    public void OnBackButtonClicked() {
        PanelManager.Instance.HidePanel();
    }

    public void OnBuyVehicle() {
        if (selectedItem == null) {
            Debug.Log("selectedItem = null");
            return;
        }
        int vehicleIndex = Array.IndexOf(vehiclesContainer.Vehicles, selectedItem.Config);
        if(vehicleIndex == -1) {
            Debug.LogError("Vehicle not found");
            return;
        }

        if(UserManager.playerData.GetInt(PlayerPrefsStrings.CASH) < selectedItem.Config.Price) {
            Debug.LogError("Not enough money");
            PopupPanel.Instance.Show("", "Not enough money.", null);
            return;
        }

        /*VehicleSaveData newSaveData = new VehicleSaveData(vehicleIndex);
        newSaveData.purchased = true;
        vehiclesContainer.vehicleSaveDatas.Add(newSaveData);
        selectedSaveData = newSaveData;*/

        PopupPanel.Instance.Show("", "Do you want to buy this vehicle?", () => OnPurchaseConfirmed(vehicleIndex), true);
    }

    void OnPurchaseConfirmed(int vehicleIndex) {
        vehiclesContainer.vehicleSaveDatas[vehicleIndex].purchased = true;
        selectedSaveData = vehiclesContainer.vehicleSaveDatas[vehicleIndex];
        UserManager.playerData.SubtractInt(PlayerPrefsStrings.CASH, selectedItem.Config.Price);
        selectedItem.SetState(ShopItemState.Purchased);
        UpdateUIButtons();

        AudioManager.Instance.PlayCommonAudio(AudioManager.Instance.AudioDataContainer.PurchaseClip);
    }

    public void OnEquipVehicle() {
        if(selectedItem == null) return;

        equippedVehicleConfig = selectedItem.Config;
        UserManager.playerData.SetInt(PlayerPrefsStrings.SELECTED_VEHICLE, Array.IndexOf(vehiclesContainer.Vehicles, selectedItem.Config));
        UpdateUIButtons();
        UpdateEquip();
        vehicleStats.UpdateValues(equippedVehicleConfig, selectedVehicleConfig);
        AudioManager.Instance.PlayCommonAudio(AudioManager.Instance.AudioDataContainer.EquipClip);

    }

    public void OpenUpgradeMenu() {
        if (spawnedCategoryItems == null || spawnedCategoryItems.Length == 0) {
            spawnedCategoryItems = new UpgradeCategory[selectedVehicleConfig.Upgrades.Length];
            for (int i = 0; i < selectedVehicleConfig.Upgrades.Length; i++) {
                UpgradeCategory categ = Instantiate(upgradeCategoryPrefab, upgradeCategoryContainer);
                categ.Init(selectedVehicleConfig.Upgrades[i].upgradeType.ToString(), selectedVehicleConfig.Upgrades[i], OnCategorySelected);
                spawnedCategoryItems[i] = categ;
            }
        }
        else {
            for (int i = 0; i < spawnedCategoryItems.Length; i++)
                spawnedCategoryItems[i].Init(selectedVehicleConfig.Upgrades[i].upgradeType.ToString(), selectedVehicleConfig.Upgrades[i], OnCategorySelected);
        }

        inUpgradeMenu = true;
        PanelManager.Instance.ShowPanel("CategoryPanel", () => {
            vehicleScrollPanel.SetActive(false);
            categoryScrollPanel.SetActive(true);
            upgradeButton.gameObject.SetActive(false);
            equipButton.gameObject.SetActive(false);

            vehicleStats.UpdateValues(selectedVehicleConfig, null);
        },
        () => {
            vehicleScrollPanel.SetActive(true);
            categoryScrollPanel.SetActive(false);
            inUpgradeMenu = false;
            UpdateUIButtons();
        });
    }

    public void OnCategorySelected(UpgradeData upgradeData) {
        selectedUpgradeData = upgradeData;
        OpenLevelsMenu();
    }

    public void OpenLevelsMenu() {
        if (spawnedUpgradeLevels == null || spawnedUpgradeLevels.Length == 0) {
            spawnedUpgradeLevels = new UpgradeItem[selectedUpgradeData.val.Length];
            for (int i = 0; i < selectedUpgradeData.val.Length; i++) {
                UpgradeItem upgrade = Instantiate(upgradeItemPrefab, upgradeItemsContainer);
                upgrade.Init(i, selectedUpgradeData.price[i], OnUpgradeLevelSelected);
                spawnedUpgradeLevels[i] = upgrade;

                if (selectedSaveData.PurchasedUpgrades.Get(selectedUpgradeData.upgradeType).Contains(i))
                    spawnedUpgradeLevels[i].SetState(ShopItemState.Purchased);

                if (selectedSaveData.EquippedLevels.Get(selectedUpgradeData.upgradeType, -1) == i)
                    spawnedUpgradeLevels[i].SetState(ShopItemState.Equipped);
            }
        }
        else {
            for (int i = 0; i < spawnedUpgradeLevels.Length; i++) {
                spawnedUpgradeLevels[i].Init(i, selectedUpgradeData.price[i], OnUpgradeLevelSelected);

                if (selectedSaveData.PurchasedUpgrades.Get(selectedUpgradeData.upgradeType).Contains(i))
                    spawnedUpgradeLevels[i].SetState(ShopItemState.Purchased);

                if (selectedSaveData.EquippedLevels.Get(selectedUpgradeData.upgradeType, -1) == i)
                    spawnedUpgradeLevels[i].SetState(ShopItemState.Equipped);
            }
        }

        if (selectedSaveData.EquippedLevels.Get(selectedUpgradeData.upgradeType, -1) == -1)
            OnUpgradeLevelSelected(null);
        else
            OnUpgradeLevelSelected(spawnedUpgradeLevels[selectedSaveData.EquippedLevels.Get(selectedUpgradeData.upgradeType, -1)]);

        UpdateUpgradeUIButtons();
        UpdateUpgradeEquip();

        PanelManager.Instance.ShowPanel("LevelsPanel", () => {
            categoryScrollPanel.SetActive(false);
            levelsScrollPanel.SetActive(true);

            buyButton.onClick.RemoveAllListeners();
            equipButton.onClick.RemoveAllListeners();

            buyButton.onClick.AddListener(OnBuyUpgrade);
            equipButton.onClick.AddListener(OnEquipUpgrade);
        },
        () => {
            categoryScrollPanel.SetActive(true);
            levelsScrollPanel.SetActive(false);

            buyButton.onClick.RemoveAllListeners();
            equipButton.onClick.RemoveAllListeners();

            buyButton.onClick.AddListener(OnBuyVehicle);
            equipButton.onClick.AddListener(OnEquipVehicle);

            buyButton.gameObject.SetActive(false);
            equipButton.gameObject.SetActive(false);
        });
    }

    public void OnUpgradeLevelSelected(UpgradeItem newSelected) {
        if (selectedUpgradeItem != null)
            selectedUpgradeItem.Select(false);

        if(newSelected == null) {
            selectedUpgradeItem = null;
            selectedLevelIndex = -1;
            buyButton.gameObject.SetActive(false);
            equipButton.gameObject.SetActive(false);
            return;
        }

        selectedUpgradeItem = newSelected;
        newSelected.Select(true);

        selectedLevelIndex = newSelected.LevelIndex;

        List<int> purchasedUpgrades = selectedSaveData.PurchasedUpgrades.Get(selectedUpgradeData.upgradeType);
        priceText.text = $"{selectedUpgradeData.price[selectedLevelIndex]}$";
        //priceText.text = $"{selectedVehicleConfig.Upgrades.Price}$";

        vehicleStats.UpdateValues(selectedVehicleConfig, selectedUpgradeData.upgradeType, newSelected.LevelIndex);

        if (selectedSaveData.EquippedLevels.Get(selectedUpgradeData.upgradeType, -1) == selectedLevelIndex) {
            buyButton.gameObject.SetActive(false);
            equipButton.gameObject.SetActive(false);
        }
        else if (purchasedUpgrades.Contains(selectedLevelIndex)) {
            buyButton.gameObject.SetActive(false);
            equipButton.gameObject.SetActive(true);
        }
        else {
            buyButton.gameObject.SetActive(true);
            equipButton.gameObject.SetActive(false);
        }
    }

    public void OnBuyUpgrade() {
        if (UserManager.playerData.GetInt(PlayerPrefsStrings.CASH) < selectedUpgradeData.price[selectedUpgradeItem.LevelIndex]) {
            Debug.LogError("Not enough money");
            PopupPanel.Instance.Show("", "Not enough money.", null);
            return;
        }

        List<int> purchasedUpgrades = selectedSaveData.PurchasedUpgrades.Get(selectedUpgradeData.upgradeType);
        purchasedUpgrades.Add(selectedLevelIndex);
        UserManager.playerData.SubtractInt(PlayerPrefsStrings.CASH, selectedUpgradeData.price[selectedUpgradeItem.LevelIndex]);
        AudioManager.Instance.PlayCommonAudio(AudioManager.Instance.AudioDataContainer.PurchaseClip);
        if (selectedUpgradeItem != null) {
            OnEquipUpgrade();
            return;
        }
        Debug.Log("Bought upgrade");
        UpdateUpgradeEquip();
        UpdateUIButtons();

    }

    public void OnEquipUpgrade() {
        Debug.Log("Equipped upgrade");
        selectedSaveData.EquippedLevels.Set(selectedUpgradeData.upgradeType, selectedLevelIndex);

        selectedUpgradeItem.SetState(ShopItemState.Equipped);

        vehicleStats.UpdateValues(selectedVehicleConfig, selectedUpgradeData.upgradeType, selectedUpgradeItem.LevelIndex);


        UpdateUpgradeEquip();
        UpdateUIButtons();
        equipButton.gameObject.SetActive(false);
        upgradeButton.gameObject.SetActive(false);
        AudioManager.Instance.PlayCommonAudio(AudioManager.Instance.AudioDataContainer.EquipClip);

    }

    void UpdateUpgradeEquip() {
        for (int i = 0; i < spawnedUpgradeLevels.Length; i++) {
            if (selectedSaveData.EquippedLevels.Get(selectedUpgradeData.upgradeType) == i)
                spawnedUpgradeLevels[i].SetState(ShopItemState.Equipped);
            else if (selectedSaveData.PurchasedUpgrades.Get(selectedUpgradeData.upgradeType).Any(x => x == i))
                spawnedUpgradeLevels[i].SetState(ShopItemState.Purchased);
        }
    }
}
