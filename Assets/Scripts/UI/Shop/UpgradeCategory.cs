using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UpgradeCategory : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] Button btn;

    UpgradeData upgradeData;
    Action<UpgradeData> onClicked;

    public void Init(string name, UpgradeData data, Action<UpgradeData> onClicked) {
        nameText.text = name;
        upgradeData = data;
        this.onClicked = onClicked;
    }

    public void OnClicked() {
        onClicked?.Invoke(upgradeData);
    }

}
