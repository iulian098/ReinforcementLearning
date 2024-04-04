using UnityEngine;

public class TrackSelection : MonoBehaviour
{
    [SerializeField] MainMenu mainMenu;
    [SerializeField] TracksContainer tracksContainer;
    [SerializeField] TrackItem trackItemPrefab;
    [SerializeField] Transform trackItemsContainer;

    TrackItem[] spawnedItems;

    void Start()
    {
        spawnedItems = new TrackItem[tracksContainer.Tracks.Length];
        
        for (int i = 0; i < tracksContainer.Tracks.Length; i++) {
            spawnedItems[i] = Instantiate(trackItemPrefab, trackItemsContainer);
            spawnedItems[i].Init(tracksContainer.Tracks[i], mainMenu.OnPlay);
        }
    }
}
