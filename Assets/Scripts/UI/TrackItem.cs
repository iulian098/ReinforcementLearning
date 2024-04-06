using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrackItem : MonoBehaviour
{
    [SerializeField] Image thumbnail;
    [SerializeField] TMP_Text trackNameText;

    Action<RaceData> onClick;
    RaceData raceData;

    public void Init(RaceData data, Action<RaceData> onClick) {
        this.onClick = onClick;    
        raceData = data;

        thumbnail.sprite = raceData.Thumbnail;
        trackNameText.text = raceData.TrackName;
    }

    public void OnClick() {
        onClick?.Invoke(raceData);
    }
}
