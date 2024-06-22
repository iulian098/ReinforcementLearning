using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TracksContainer", menuName = "ScriptableObjects/Tracks Container")]
public class TracksContainer : ScriptableObject
{
    [SerializeField] RaceData[] tracks;
    List<TrackSaveData> tracksSaveData;

    public RaceData[] Tracks => tracks;
    public int selectedTrack;

    public void SetSaveDatas(List<TrackSaveData> saveData) {
        if (saveData.IsNullOrEmpty()) {
            for (int i = 0; i < tracks.Length; i++) 
                saveData.Add(new TrackSaveData());
        }else if(saveData.Count != tracks.Length) {
            for (int i = saveData.Count; i < tracks.Length; i++) 
                saveData.Add(new TrackSaveData());
        }

        tracksSaveData = saveData;

        for (int i = 0; i < tracks.Length; i++) {
            tracks[i].saveData = tracksSaveData[i];
        }
    }
}
