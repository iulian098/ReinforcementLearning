using System;
using TMPro;
using UnityEngine;

public class UpgradeItem : MonoBehaviour
{

    [SerializeField] TMP_Text lable;
    [SerializeField] TMP_Text priceText;
    [SerializeField] GameObject selected;

    int levelIndex;

    public Action<UpgradeItem> OnClicked;

    public int LevelIndex => levelIndex;

    public void Init(int level, int price, Action<UpgradeItem> onClicked = null) {
        levelIndex = level;
        lable.text = $"Level {level + 1}";
        priceText.text = $"{price}$";

        OnClicked = onClicked;
    }

    public void OnClick() {
        OnClicked?.Invoke(this);
    }

    public void Select(bool val) {
        selected.SetActive(val);
    }

    public void SetState(ShopItemState state) {
        switch (state) {
            case ShopItemState.Purchased:
                priceText.text = "Purchased";
                break;
            case ShopItemState.Equipped:
                priceText.text = "Equipped";
                break;
            default:
                break;
        }
    }
}
