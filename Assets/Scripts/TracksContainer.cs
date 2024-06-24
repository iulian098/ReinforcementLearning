using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "TracksContainer", menuName = "ScriptableObjects/Tracks Container")]
public class TracksContainer : ScriptableObject
{
    [SerializeField] AssetReferenceT<RaceData>[] tracksRefs;

    RaceData[] tracks;
    public RaceData[] Tracks => tracks;

    public async Task Init() {
        tracks = new RaceData[tracksRefs.Length];

        for (int i = 0; i < tracksRefs.Length; i++) {
            RaceData track = await AssetsManager<RaceData>.Load(tracksRefs[i]);
            tracks[i] = track;
        }
    }
}
