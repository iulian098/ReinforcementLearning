using UnityEngine;

public class TrackSelection : MonoBehaviour
{
    [SerializeField] MainMenu mainMenu;
    [SerializeField] TracksContainer tracksContainer;
    [SerializeField] TrackItem trackItemPrefab;
    [SerializeField] Transform trackItemsContainer;
    [SerializeField] UITrackInfo trackInfo;

    TrackItem[] spawnedItems;

    TrackItem selectedTrack;

    void Start()
    {
        spawnedItems = new TrackItem[tracksContainer.Tracks.Length];
        
        for (int i = 0; i < tracksContainer.Tracks.Length; i++) {
            spawnedItems[i] = Instantiate(trackItemPrefab, trackItemsContainer);
            spawnedItems[i].Init(tracksContainer.Tracks[i], OnTrackSelected/*mainMenu.OnPlay*/);
        }
    }

    void OnTrackSelected(TrackItem trackItem, RaceData raceData) {
        if (selectedTrack != null)
            selectedTrack.SetSelected(false);

        selectedTrack = trackItem;
        selectedTrack.SetSelected(true);

        trackInfo.SetRaceData(raceData);
    }
}
