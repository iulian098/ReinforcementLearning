using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VehicleShopItem : MonoBehaviour
{
    [SerializeField] TMP_Text price;
    [SerializeField] Image icon;
    [SerializeField] Image selected;
    [SerializeField] GameObject lockedObj;
    [SerializeField] TMP_Text unlockingText;
    VehicleConfig config;

    bool isSelected;
    public bool IsSelected {
        get {
            return isSelected;
        }
        set {
            isSelected = value;
            selected.gameObject.SetActive(value);
        }
    }
    public VehicleConfig Config => config;

    public Action<VehicleShopItem> OnSelected;

    public void Init(VehicleConfig config) {
        this.config = config;

        price.text = $"{config.Price}$";

        icon.sprite = IconCreator.CreateSprite(config.PreviewPrefab, new Vector3(0, 0, -2), new Vector3(0, -90, 10), new Rect(0, 0, icon.rectTransform.rect.width, icon.rectTransform.rect.height), 2);

        lockedObj.SetActive(UserManager.playerData.GetInt(PlayerPrefsStrings.LEVEL) < config.UnlockLevel);
        if (UserManager.playerData.GetInt(PlayerPrefsStrings.LEVEL) < config.UnlockLevel)
            unlockingText.text = $"Unlocking at level {config.UnlockLevel + 1}";
    }

    public void SetState(ShopItemState state) {
        switch (state) {
            case ShopItemState.Purchased:
                price.text = "Purchased";
                break;
            case ShopItemState.Equipped:
                price.text = "Equipped";
                break;
            default:
                break;
        }
    }

    public void OnItemSelected() {
        OnSelected?.Invoke(this);
    }

}
