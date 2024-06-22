using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class ResultData {
    public int placement;
    public VehicleManager vehicleManager;
    public float time;
}

public class ResultScreen : MonoBehaviour
{
    [SerializeField] TMP_Text raceTypeText;
    [SerializeField] TMP_Text cashWonText;
    [SerializeField] TMP_Text expWonText;
    [SerializeField] Transform resultItemsContainer;
    [SerializeField] ResultItem resultItemPrefab;

    RaceData raceData;
    List<ResultData> results = new List<ResultData>();
    List<ResultItem> spawnedResultItems =new List<ResultItem>();

    public void Init(List<VehicleManager> vehicles) {
        raceData = RaceManager.Instance.RaceData;
        if (spawnedResultItems.Count < vehicles.Count) {
            ClearResults();
            SpawnResults(vehicles);
        }
    }

    public void Show(int playerPlacement) {
        gameObject.SetActive(true);
        raceTypeText.text = raceData.Type.ToString();

        if (playerPlacement < raceData.CoinsRewards.Length)
            cashWonText.text = $"+{raceData.CoinsRewards[playerPlacement]}$";
        else
            cashWonText.text = "0$";

        if (playerPlacement < raceData.ExpReward.Length)
            expWonText.text = $"+{raceData.ExpReward[playerPlacement]} EXP";
        else
            expWonText.text = "0";
    }

    public void SetResult(int index, ResultData result) {
        results[index] = result;
        spawnedResultItems[index].SetData(result);
    }

    public void SpawnResults(List<VehicleManager> vehicles) {
        for(int i = 0; i < vehicles.Count; i++) { 
            ResultItem item = Instantiate(resultItemPrefab, resultItemsContainer);
            spawnedResultItems.Add(item);
            results.Add(new ResultData());
        }
    }

    public void ClearResults() {
        foreach (var item in spawnedResultItems)
            Destroy(item.gameObject);

        spawnedResultItems.Clear();
        results.Clear();
    }
}
