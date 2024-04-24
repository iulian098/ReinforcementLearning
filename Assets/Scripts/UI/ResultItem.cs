using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultItem : MonoBehaviour
{
    [SerializeField] TMP_Text placementText;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text timeText;
    [SerializeField] TMP_Text rewardText;

    public void SetPlacement(int val) {
        placementText.text = val.ToString();
    }

    public void SetData(ResultData resultData) {
        TimeSpan time = TimeSpan.FromSeconds(resultData.time);
        nameText.text = resultData.vehicleManager.PlayerName;
        timeText.text = $"{time.Minutes}:{time.Seconds}";
        rewardText.text = "0";
    }
}
