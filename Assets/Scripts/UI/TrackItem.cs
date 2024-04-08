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
    [SerializeField] Sprite starFilled;
    [SerializeField] Image[] stars;

    Action<RaceData> onClick;
    RaceData raceData;

    public void Init(RaceData data, Action<RaceData> onClick) {
        this.onClick = onClick;    
        raceData = data;

        thumbnail.sprite = raceData.Thumbnail;
        trackNameText.text = raceData.TrackName;

        if (data.saveData.placement == -1) return;

        for (int i = 0; i < stars.Length; i++) {
            if(i < 3 - data.saveData.placement)
                stars[i].overrideSprite = starFilled;
        }
    }

    public void OnClick() {
        onClick?.Invoke(raceData);
    }
}
