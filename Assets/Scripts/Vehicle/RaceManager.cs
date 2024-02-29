using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceManager : MonoBehaviour {
    public static RaceManager Instance;

    [SerializeField] VehicleCheckpointsContainer vehicleCheckpoints;
    [SerializeField] List<VehicleCheckpointManager> vehicles;
    [SerializeField] int startingCheckpointIndex = 0;
    float[] distances;

    public float[] Distances => distances;

    private void Awake() {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
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
    }

    private void FixedUpdate() {
        UpdateVehiclesPlacements(true);
    }

    public void UpdateVehiclesPlacements(bool sendCallback) {
        vehicles = vehicles.OrderByDescending(x => x.vehicleData.loopCount).ThenByDescending(x => x.vehicleData.totalDistance).ToList();

        for (int i = 0; i < vehicles.Count; i++) {
            if (vehicles[i].Initialized && sendCallback) {
                if (i > vehicles[i].currentPlacement)
                    vehicles[i].OnPlacementChanged?.Invoke(true);
                else if (i < vehicles[i].currentPlacement)
                    vehicles[i].OnPlacementChanged?.Invoke(false);
            }

            vehicles[i].currentPlacement = i;
        }
    }

    /*void Update() {

        foreach (var vehicle in vehicles) {
            VehicleData vehicleData = vehicle.vehicleData;
            float checkpointDist = GetCurrentCheckpointDistance(vehicleData.Vehicle.transform.position, vehicleData.currentCheckpoint.position, vehicleData.nextCheckpoint.position);
            float dist = Vector3.Distance(vehicleData.currentCheckpoint.position, vehicleData.nextCheckpoint.position);
            float distFactor = checkpointDist == 0 ? 0 : checkpointDist / dist;
            vehicleData.totalDistance = distances[vehicleData.checkpointIndex] + distFactor * dist;

            //Set vehicle road center
            vehicleData.RoadCenter = GetRoadCenter(vehicleData.Vehicle.transform.position, vehicleData.currentCheckpoint.position, vehicleData.nextCheckpoint.position);

            bool goingForward = vehicleData.Vehicle.VehicleRigidBody.velocity.magnitude >= 0.1f && Vector3.Dot(vehicleData.Vehicle.VehicleRigidBody.velocity, (vehicleData.nextCheckpoint.position - vehicleData.currentCheckpoint.position).normalized) > 0;

            // Check if the vehicle is moving forward based on velocity
            //if(currentCheckpointIndex == 0 && passedCheckpoints.Count >= vehicleCheckpoints.Checkpoints.Length && goingForward && PassedAllCheckpoints()) {
            if(vehicleData.checkpointIndex == 0 && vehicleData.PassedCheckpoints.Count >= vehicleCheckpoints.Checkpoints.Length && goingForward && PassedAllCheckpoints(vehicleData)) {
                //loopIndex++;
                vehicleData.loopCount++;
                vehicleData.PassedCheckpoints.Clear();
                vehicleData.Vehicle.OnFinishReached?.Invoke();
                //passedCheckpoints.Clear();
            }

            if (distFactor >= 1f && goingForward)
                SetNextCheckpoint(vehicleData, goingForward);
            else if (distFactor <= 0 && !goingForward)
                SetPreviousCheckpoint(vehicleData, goingForward);
        }
    }

    public void ResetVehicles() {
        for (int vehIndex = 0; vehIndex < vehicles.Count; vehIndex++)
            vehicles[vehIndex].vehicleData = new VehicleData(vehicles[vehIndex], currentCheckpoint, nextCheckpoint);
        
    }

    void SetNextCheckpoint(VehicleData data, bool goingForward) {
        if (!goingForward) return;

        if (vehicleCheckpoints.Checkpoints[data.PassedCheckpoints.Count] == data.currentCheckpoint) {
            data.PassedCheckpoints.Add(data.currentCheckpoint);
            data.Vehicle.OnCheckpointReached?.Invoke();
        }

        data.currentCheckpoint = data.nextCheckpoint;

        if (data.checkpointIndex + 1 >= vehicleCheckpoints.Checkpoints.Length)
            data.checkpointIndex = 0;
        else
            data.checkpointIndex++;

        data.nextCheckpoint = vehicleCheckpoints.Checkpoints[data.checkpointIndex + 1 >= vehicleCheckpoints.Checkpoints.Length ? 0 : data.checkpointIndex + 1];
    }

    void SetPreviousCheckpoint(VehicleData data, bool goingForward) {
        if (goingForward) return;

        data.nextCheckpoint = data.currentCheckpoint;
        if (data.checkpointIndex - 1 < 0)
            data.checkpointIndex = vehicleCheckpoints.Checkpoints.Length - 1;
        else
            data.checkpointIndex--;
        data.currentCheckpoint = vehicleCheckpoints.Checkpoints[data.checkpointIndex];
    }

    bool PassedAllCheckpoints(VehicleData data) {
        for (int i = 0; i < data.PassedCheckpoints.Count; i++)
            if (data.PassedCheckpoints[i] != vehicleCheckpoints.Checkpoints[i])
                return false;

        return true;
    }

    float GetCurrentCheckpointDistance(Vector3 vehicle, Vector3 curr, Vector3 next) {
        float checkpointDistance = Vector3.Distance(curr, next);
        float currentCheckpointDistance = Vector3.Distance(curr, vehicle);
        float nextCheckpointDistance = Vector3.Distance(next, vehicle);

        float AriaT = Aria(checkpointDistance, currentCheckpointDistance, nextCheckpointDistance);
        float h = (2 * AriaT) / checkpointDistance;

        if(Vector3.Angle(next - curr, vehicle - curr) >= 90) 
            return 0;
        else if(Vector3.Angle(curr - next, vehicle - next) >= 90) 
            return checkpointDistance;

        return GetC(currentCheckpointDistance, h);
    }

    Vector3 GetRoadCenter(Vector3 vehicle, Vector3 curr, Vector3 next) {
        float dist = GetCurrentCheckpointDistance(vehicle, curr, next);
        Vector3 dir = next - curr;
        return curr + dir.normalized * dist;
    }

    float GetC(float ip, float c2) {
        float t = ip * ip - c2 * c2;
        return Mathf.Sqrt(t) * (t < 0 ? -1 : 1);
    }

    float Aria(float a, float b, float c) {
        float s = (a + b + c) / 2;
        float t = s * (s - a) * (s - b) * (s - c);
        return Mathf.Sqrt(t);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.white;
        int checkpointsCount = vehicleCheckpoints.Checkpoints.Length;
        for (int i = 0; i < checkpointsCount; i++) {
            Gizmos.DrawLine(vehicleCheckpoints.Checkpoints[i].position, vehicleCheckpoints.Checkpoints[i + 1 > checkpointsCount - 1 ? 0 : i + 1].position);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(currentCheckpoint.position, nextCheckpoint.position);

        foreach (var item in vehicles) {
            VehicleData vehicleData = item.vehicleData;
            Gizmos.DrawSphere(vehicleData.RoadCenter, 0.5f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(item.transform.position, item.vehicleData.RoadCenter);
        }

    }*/
    private void OnDrawGizmos() {
        Gizmos.color = Color.white;
        int checkpointsCount = vehicleCheckpoints.Checkpoints.Length;
        for (int i = 0; i < checkpointsCount; i++) {
            Gizmos.DrawLine(vehicleCheckpoints.Checkpoints[i].position, vehicleCheckpoints.Checkpoints[i + 1 > checkpointsCount - 1 ? 0 : i + 1].position);
        }
    }
}
