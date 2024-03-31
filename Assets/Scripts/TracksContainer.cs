using UnityEngine;

[CreateAssetMenu(fileName = "TracksContainer", menuName = "ScriptableObjects/Tracks Container")]
public class TracksContainer : ScriptableObject
{
    [SerializeField] RaceData[] tracks;

    public RaceData[] Tracks => tracks;
}
