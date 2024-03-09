using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UILeaderboard : MonoBehaviour
{
    [SerializeField] GameObject leaderboardItem;
    [SerializeField] Transform itemsContainer;

    Coroutine leaderboardUpdateCoroutine;
    List<(VehicleCheckpointManager, GameObject)> spawnedItems = new List<(VehicleCheckpointManager, GameObject)>();

    public void Init(List<VehicleCheckpointManager> items) {
        for (int i = 0; i < items.Count; i++) {
            GameObject go = Instantiate(leaderboardItem, itemsContainer);
            go.GetComponent<TMP_Text>().text = items[i].name;
            go.name = "Leaderboard" + items[i].name;
            spawnedItems.Add((items[i], go));
        }
        if(leaderboardUpdateCoroutine != null)
            StopCoroutine(leaderboardUpdateCoroutine);
        leaderboardUpdateCoroutine = StartCoroutine(UpdateLeaderboardCoroutine());
    }

    IEnumerator UpdateLeaderboardCoroutine() {
        while (true) {
            foreach (var item in spawnedItems)
                item.Item2.transform.SetSiblingIndex(item.Item1.currentPlacement);
            yield return new WaitForSeconds(0.2f);
        }
    }
}
