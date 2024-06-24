using UnityEngine;
using UnityEngine.AddressableAssets;

public class TrackSelection : MonoBehaviour
{
    [SerializeField] MainMenu mainMenu;
    [SerializeField] AssetReferenceT<TracksContainer> tracksContainerRef;
    [SerializeField] TrackItem trackItemPrefab;
    [SerializeField] Transform trackItemsContainer;
    [SerializeField] UITrackInfo trackInfo;

    TrackItem[] spawnedItems;

    TrackItem selectedTrack;
    TracksContainer tracksContainer;
    async void Start()
    {
        tracksContainer = await AssetsManager<TracksContainer>.Load(tracksContainerRef);
        trackInfo.Init(tracksContainer);
        spawnedItems = new TrackItem[tracksContainer.Tracks.Length];
        
        for (int i = 0; i < tracksContainer.Tracks.Length; i++) {
            spawnedItems[i] = Instantiate(trackItemPrefab, trackItemsContainer);
            spawnedItems[i].Init(tracksContainer.Tracks[i], OnTrackSelected/*mainMenu.OnPlay*/);
        }

        if (GlobalData.lastPlayedTrack != -1)
            spawnedItems[GlobalData.lastPlayedTrack].OnClick();
        else
            spawnedItems[0].OnClick();
    }

    void OnTrackSelected(TrackItem trackItem, RaceData raceData) {
        if (selectedTrack != null)
            selectedTrack.SetSelected(false);

        selectedTrack = trackItem;
        selectedTrack.SetSelected(true);

        trackInfo.SetRaceData(raceData);
    }

    public void Close() {
        PanelManager.Instance.HidePanel("TrackSelection");
    }
}
