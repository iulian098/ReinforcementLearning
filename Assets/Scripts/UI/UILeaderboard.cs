using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILeaderboard : MonoBehaviour
{
    [SerializeField] UILeaderboardItem leaderboardItem;
    [SerializeField] Transform itemsContainer;

    Coroutine leaderboardUpdateCoroutine;
    List<(VehicleManager, UILeaderboardItem)> spawnedItems = new List<(VehicleManager, UILeaderboardItem)>();

    public void Init(List<VehicleManager> items) {
        for (int i = 0; i < items.Count; i++) {
            UILeaderboardItem item = Instantiate(leaderboardItem, itemsContainer);

            item.SetText(items[i].PlayerName);
            item.name = "Leaderboard" + items[i].name;

            spawnedItems.Add((items[i], item));
        }

        if(leaderboardUpdateCoroutine != null)
            StopCoroutine(leaderboardUpdateCoroutine);

        leaderboardUpdateCoroutine = StartCoroutine(UpdateLeaderboardCoroutine());
    }

    IEnumerator UpdateLeaderboardCoroutine() {
        while (true) {

            foreach (var item in spawnedItems) {
                item.Item2.transform.SetSiblingIndex(item.Item1.currentPlacement);
                item.Item2.SetText($"{item.Item1.currentPlacement + 1} {item.Item1.PlayerName}");
            }

            yield return new WaitForSeconds(0.2f);
        }
    }
}
