using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VehicleShopItem : MonoBehaviour
{
    [SerializeField] TMP_Text price;
    [SerializeField] Image icon;
    [SerializeField] Image selected;
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

    }

    public void Equip(bool val) {
        price.text = val ? "Equipped" : $"{config.Price}$";
    }

    public void OnItemSelected() {
        OnSelected?.Invoke(this);
    }

}
