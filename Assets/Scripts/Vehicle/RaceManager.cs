using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents.Policies;
using UnityEngine;

public class RaceManager : MonoBehaviour {
    public static RaceManager Instance;

    [SerializeField] UIManager uiManager;
    [SerializeField] UILeaderboard leaderboard;
    [SerializeField] VehicleCheckpointsContainer vehicleCheckpoints;
    [SerializeField] List<VehicleCheckpointManager> vehicles;
    [SerializeField] int startingCheckpointIndex = 0;
    [SerializeField] bool useSelfPlay;
    float[] distances;
    bool stopUpdate;

    public float[] Distances => distances;
    public bool StopUpdate { get { return stopUpdate; } set {  stopUpdate = value; } }
    private void Awake() {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        if (useSelfPlay) {
            for (int i = 0; i < vehicles.Count; ++i)
                vehicles[i].GetComponent<BehaviorParameters>().TeamId = i;
        }
    }

    private void Start() {
        distances = new float[vehicleCheckpoints.Checkpoints.Length + 1];

        for (int i = 1; i < distances.Length; i++) {
            if (i < vehicleCheckpoints.Checkpoints.Length)
                distances[i] = distances[i - 1] + Vector3.Distance(vehicleCheckpoints.Checkpoints[i - 1].position, vehicleCheckpoints.Checkpoints[i].position);
            else
                distances[i] = distances[i - 1] + Vector3.Distance(vehicleCheckpoints.Checkpoints[i - 1].position, vehicleCheckpoints.Checkpoints[0].position);
        }

        UpdateVehiclesPlacements(false);

        foreach (var vehicle in vehicles)
            vehicle.Init();

        leaderboard.Init(vehicles);
    }

    private void FixedUpdate() {
        if (stopUpdate) return;
        UpdateVehiclesPlacements(true);
    }

    public void UpdateVehiclesPlacements(bool sendCallback) {
        vehicles = vehicles.OrderByDescending(x => x.vehicleData.loopCount).ThenByDescending(x => x.vehicleData.totalDistance).ToList();

        for (int i = 0; i < vehicles.Count; i++) {
            if (vehicles[i].Initialized && sendCallback) {
                if (i > vehicles[i].currentPlacement) {
                    Debug.Log($"[RaceManager] {vehicles[i].name} Current placement {vehicles[i].currentPlacement} increased to {i}");
                    vehicles[i].OnPlacementChanged?.Invoke(false);
                }
                else if (i < vehicles[i].currentPlacement) {
                    Debug.Log($"[RaceManager] {vehicles[i].name} Current placement {vehicles[i].currentPlacement} decreased to {i}");
                    vehicles[i].OnPlacementChanged?.Invoke(true);
                }
            }

            vehicles[i].currentPlacement = i;
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.white;
        int checkpointsCount = vehicleCheckpoints.Checkpoints.Length;
        for (int i = 0; i < checkpointsCount; i++) {
            Gizmos.DrawLine(vehicleCheckpoints.Checkpoints[i].position, vehicleCheckpoints.Checkpoints[i + 1 > checkpointsCount - 1 ? 0 : i + 1].position);
        }
    }
}
