using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "TracksSaveDataContainer", menuName = "ScriptableObjects/Tracks Save Data Container")]
public class TracksSaveDataContainer : ScriptableObject
{
    List<TrackSaveData> tracksSaveData;

    public void SetSaveDatas(RaceData[] tracks, List<TrackSaveData> saveData) {
        if (saveData.IsNullOrEmpty()) {
            for (int i = 0; i < tracks.Length; i++)
                saveData.Add(new TrackSaveData());
        }
        else if (saveData.Count != tracks.Length) {
            for (int i = saveData.Count; i < tracks.Length; i++)
                saveData.Add(new TrackSaveData());
        }

        tracksSaveData = saveData;

        for (int i = 0; i < tracks.Length; i++) {
            tracks[i].saveData = tracksSaveData[i];
        }
    }

}
