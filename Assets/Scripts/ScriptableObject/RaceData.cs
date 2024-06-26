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
    [SerializeField] int unlockLevel;
    [SerializeField] string sceneName;
    [SerializeField] string trackName;
    [SerializeField] Sprite thumbnail;
    [SerializeField] RaceType raceType;
    [SerializeField] int maxPlayers;
    [SerializeField] int maxLoops;
    [SerializeField] int[] coinsReward;
    [SerializeField] int[] expReward;
    [SerializeField] bool useEvent;

    public int BuyIn => buyIn;
    public int UnlockLevel => unlockLevel;
    public string SceneName => sceneName;
    public string TrackName => trackName;
    public Sprite Thumbnail => thumbnail;
    public RaceType Type => raceType;
    public int MaxPlayers => maxPlayers;
    public int MaxLoops => maxLoops;
    public int[] CoinsRewards => coinsReward;
    public int[] ExpReward => expReward;
    public bool UseEvent => useEvent;

    public TrackSaveData saveData;

}
