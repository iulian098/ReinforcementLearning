using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VehicleData {
    public Transform currentCheckpoint;
    public Transform nextCheckpoint;
    public int checkpointIndex;
    public int loopCount;
    public float totalDistance;

    List<Transform> passedCheckpoints = new List<Transform>();
    Vehicle vehicle;

    public Vehicle Vehicle => vehicle;
    public List<Transform> PassedCheckpoints => passedCheckpoints;
    public Vector3 RoadCenter { get; set; }

    public VehicleData(Vehicle vehicle, Transform currentCheckpoint, Transform nextCheckpoint) {
        this.vehicle = vehicle;
        this.currentCheckpoint = currentCheckpoint;
        this.nextCheckpoint = nextCheckpoint;
    }
}
