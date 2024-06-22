using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "NewRaceData", menuName = "ScriptableObjects/New Race Data")]
public class RaceData : ScriptableObject
{
    public enum RaceType {
        Circuit,
        Sprint,
        TimeAttack
    }

    public AssetReferenceT<Object> sceneReference;
    [SerializeField] int buyIn;
    [SerializeField] string sceneName;
    [SerializeField] string trackName;
    [SerializeField] Sprite thumbnail;
    [SerializeField] RaceType raceType;
    [SerializeField] int maxPlayers;
    [SerializeField] int maxLoops;
    [SerializeField] int[] coinsReward;
    [SerializeField] int[] expReward;

    public int BuyIn => buyIn;
    public string SceneName => sceneName;
    public string TrackName => trackName;
    public Sprite Thumbnail => thumbnail;
    public RaceType Type => raceType;
    public int MaxPlayers => maxPlayers;
    public int MaxLoops => maxLoops;
    public int[] CoinsRewards => coinsReward;
    public int[] ExpReward => expReward;

    public TrackSaveData saveData;

}
