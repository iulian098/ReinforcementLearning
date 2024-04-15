using System.Collections.Generic;
using UnityEngine;

public class Track : MonoBehaviour
{
    [SerializeField] Transform spawnPointsContainer;

    public List<Transform> GetSpawnPoints() {
        List<Transform> list = new List<Transform>();

        for (int i = 0; i < spawnPointsContainer.childCount; i++)
            list.Add(spawnPointsContainer.GetChild(i));

        return list;
    }

    private void OnDrawGizmos() {
        if (spawnPointsContainer == null) return;

        Gizmos.color = Color.blue;

        for (int i = 0; i < spawnPointsContainer.childCount; i++) {
            Gizmos.DrawWireSphere(spawnPointsContainer.GetChild(i).position, 0.5f);
        }
    }
}
