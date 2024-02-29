using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleCheckpointsContainer : MonoBehaviour {

    #region Singleton

    public static VehicleCheckpointsContainer Instance;

    private void Awake() {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    #endregion

    [SerializeField] Transform[] checkpoints;
    [SerializeField] float checkpointRaidus = 5;

    public Transform[] Checkpoints => checkpoints;
    public float CheckpointRadius => checkpointRaidus;
    private void OnDrawGizmosSelected() {
        foreach (var checkpoint in checkpoints) {

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(checkpoint.transform.position, checkpointRaidus);
        }
    }
}
