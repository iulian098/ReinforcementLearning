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

    [SerializeField] Transform checkpointsContainer;
    [SerializeField] Transform[] checkpoints;
    [SerializeField] float checkpointRaidus = 5;
    [SerializeField] bool useTriggers;

    public Transform[] Checkpoints => checkpoints;
    public float CheckpointRadius => checkpointRaidus;
    public bool UseTriggers => useTriggers;

    private void Start() {
        if (checkpointsContainer != null) {
            checkpoints = new Transform[checkpointsContainer.childCount];
            for (int i = 0; i < checkpointsContainer.childCount; i++) {
                checkpoints[i] = checkpointsContainer.GetChild(i);
            }
            //checkpoints = checkpointsContainer.GetComponentsInChildren<Transform>();
        }
    }

    private void OnDrawGizmosSelected() {
        foreach (var checkpoint in checkpoints) {

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(checkpoint.transform.position, checkpointRaidus);
        }
    }
}
