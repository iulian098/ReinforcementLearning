using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResultData {
    public int placement;
    public VehicleManager vehicleManager;
    public float time;
}

public class ResultScreen : MonoBehaviour
{

    [SerializeField] Transform resultItemsContainer;
    [SerializeField] ResultItem resultItemPrefab;

    List<ResultData> results = new List<ResultData>();
    List<ResultItem> spawnedResultItems =new List<ResultItem>();

    private void OnEnable() {

        for (int i = 0; i < spawnedResultItems.Count; i++)
            spawnedResultItems[i].SetData(results[i]);
    }

    public void Init(List<VehicleManager> vehicles) {
        if (spawnedResultItems.Count < vehicles.Count) {
            ClearResults();
            SpawnResults(vehicles);
        }
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void SetResult(int index, ResultData result) {
        results[index] = result;
        spawnedResultItems[index].SetData(result);
    }

    public void SpawnResults(List<VehicleManager> vehicles) {
        foreach (var vehicle in vehicles) {
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
