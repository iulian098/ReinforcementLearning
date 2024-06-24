using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VehicleData {
    public Transform currentCheckpoint;
    public Transform nextCheckpoint;
    public int checkpointIndex;
    public int loopCount;
    public float totalDistance;
    public bool finished;

    public List<Transform> passedCheckpoints = new List<Transform>();

    public List<Transform> PassedCheckpoints => passedCheckpoints;
    public Vector3 RoadCenter { get; set; }

    public VehicleData(Transform currentCheckpoint, Transform nextCheckpoint) {
        this.currentCheckpoint = currentCheckpoint;
        this.nextCheckpoint = nextCheckpoint;
    }
}
