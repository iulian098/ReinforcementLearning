using UnityEngine;

[CreateAssetMenu(fileName = "NewRaceData", menuName = "ScriptableObjects/New Race Data")]
public class RaceData : ScriptableObject
{
    public enum RaceType {
        Circuit,
        Sprint
    }

    [SerializeField] Track prefab;
    [SerializeField] RaceType raceType;
    [SerializeField] int maxLoops;
    [SerializeField] int[] coinsReward;

    public Track Prefab => prefab;
    public RaceType Type => raceType;
    public int MaxLoops => maxLoops;
    public int[] CoinsRewards => coinsReward;

}
