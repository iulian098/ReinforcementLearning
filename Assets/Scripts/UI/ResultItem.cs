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

    public void SetPlacement(int val) {
        placementText.text = val.ToString();
    }

    public void SetData(ResultData resultData) {
        TimeSpan time = TimeSpan.FromSeconds(resultData.time);
        placementText.text = resultData.placement.ToString();
        nameText.text = resultData.vehicleManager.PlayerName;
        timeText.text = $"{time.Minutes}:{time.Seconds}";
    }
}
