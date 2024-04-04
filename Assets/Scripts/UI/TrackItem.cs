using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackItem : MonoBehaviour
{
    Action<RaceData> onClick;
    RaceData raceData;

    public void Init(RaceData data, Action<RaceData> onClick) {
        this.onClick = onClick;    
        raceData = data;
    }

    public void OnClick() {
        onClick?.Invoke(raceData);
    }
}
