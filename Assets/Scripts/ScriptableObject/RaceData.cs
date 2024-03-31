using UnityEngine;

[CreateAssetMenu(fileName = "NewRaceData", menuName = "ScriptableObjects/New Race Data")]
public class RaceData : ScriptableObject
{
    public enum RaceType {
        Circuit,
        Sprint
    }

    [SerializeField] string sceneName;
    [SerializeField] Track prefab;
    [SerializeField] RaceType raceType;
    [SerializeField] int maxPlayers;
    [SerializeField] int maxLoops;
    [SerializeField] int[] coinsReward;

    public string SceneName => sceneName;
    public Track Prefab => prefab;
    public RaceType Type => raceType;
    public int MaxPlayers => maxPlayers;
    public int MaxLoops => maxLoops;
    public int[] CoinsRewards => coinsReward;

}
