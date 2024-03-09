using System;
using UnityEngine;

public class VehicleCheckpointManager : MonoBehaviour
{
    [SerializeField] Vehicle vehicle;

    public VehicleData vehicleData;
    public int currentPlacement;
    float nextCheckpointDistance;
    float prevCheckpointDistance;

    public Action OnNextCheckpointReached;
    public Action OnPreviousCheckpointReached;
    public Action OnFinishReached;
    public Action<bool> OnPlacementChanged;

    bool initialized = false;

    public bool Initialized => initialized;

    VehicleCheckpointsContainer checkpointsContainer;
    VehicleCheckpointsContainer CheckpointsContainer { get {
            if (checkpointsContainer == null)
                checkpointsContainer = VehicleCheckpointsContainer.Instance;
            return checkpointsContainer;
        } 
    }

    public void Init() {
        ResetVehicleData();

        initialized = true;
    }

    void Update() {
        if (!initialized) return;
        nextCheckpointDistance = Vector3.Distance(vehicle.transform.position, vehicleData.nextCheckpoint.position);
        prevCheckpointDistance = Vector3.Distance(vehicle.transform.position, vehicleData.currentCheckpoint.position);
        float checkpointDist = GetCurrentCheckpointDistance(vehicleData.Vehicle.transform.position, vehicleData.currentCheckpoint.position, vehicleData.nextCheckpoint.position);
        float dist = Vector3.Distance(vehicleData.currentCheckpoint.position, vehicleData.nextCheckpoint.position);
        float distFactor = checkpointDist == 0 ? 0 : checkpointDist / dist;
        vehicleData.totalDistance = RaceManager.Instance.Distances[vehicleData.checkpointIndex] + distFactor * dist;

        //Set vehicle road center
        vehicleData.RoadCenter = GetRoadCenter(vehicleData.Vehicle.transform.position, vehicleData.currentCheckpoint.position, vehicleData.nextCheckpoint.position);

        bool goingForward = vehicleData.Vehicle.VehicleRigidBody.velocity.magnitude >= 0.1f && Vector3.Dot(vehicleData.Vehicle.VehicleRigidBody.velocity, (vehicleData.nextCheckpoint.position - vehicleData.currentCheckpoint.position).normalized) > 0;

        // Check if the vehicle is moving forward based on velocity
        if (vehicleData.checkpointIndex == 0 && vehicleData.PassedCheckpoints.Count >= CheckpointsContainer.Checkpoints.Length && goingForward && PassedAllCheckpoints()) {
            vehicleData.loopCount++;
            vehicleData.PassedCheckpoints.Clear();
            OnFinishReached?.Invoke();
        }

        if (nextCheckpointDistance < CheckpointsContainer.CheckpointRadius || prevCheckpointDistance < CheckpointsContainer.CheckpointRadius) {
            if (distFactor >= 1f && goingForward)
                SetNextCheckpoint(goingForward);
            else if (distFactor <= 0 && !goingForward)
                SetPreviousCheckpoint(goingForward);
        }
    }

    void SetNextCheckpoint(bool goingForward) {
        if (!goingForward) return;
        bool checkpointAdded = false;

        if (CheckpointsContainer.Checkpoints[vehicleData.PassedCheckpoints.Count] == vehicleData.currentCheckpoint) {
            vehicleData.PassedCheckpoints.Add(vehicleData.currentCheckpoint);
            checkpointAdded = true;
        }

        vehicleData.currentCheckpoint = vehicleData.nextCheckpoint;

        if (vehicleData.checkpointIndex + 1 >= CheckpointsContainer.Checkpoints.Length)
            vehicleData.checkpointIndex = 0;
        else
            vehicleData.checkpointIndex++;

        vehicleData.nextCheckpoint = CheckpointsContainer.Checkpoints[vehicleData.checkpointIndex + 1 >= CheckpointsContainer.Checkpoints.Length ? 0 : vehicleData.checkpointIndex + 1];
        
        if (checkpointAdded)
            OnNextCheckpointReached?.Invoke();
    }

    void SetPreviousCheckpoint(bool goingForward) {
        if (goingForward) return;
        
        vehicleData.nextCheckpoint = vehicleData.currentCheckpoint;
        if (vehicleData.checkpointIndex - 1 < 0)
            vehicleData.checkpointIndex = CheckpointsContainer.Checkpoints.Length - 1;
        else
            vehicleData.checkpointIndex--;
        vehicleData.currentCheckpoint = CheckpointsContainer.Checkpoints[vehicleData.checkpointIndex];

        OnPreviousCheckpointReached?.Invoke();
    }

    bool PassedAllCheckpoints() {
        for (int i = 0; i < vehicleData.PassedCheckpoints.Count; i++)
            if (vehicleData.PassedCheckpoints[i] != checkpointsContainer.Checkpoints[i])
                return false;

        return true;
    }

    float GetCurrentCheckpointDistance(Vector3 vehicle, Vector3 curr, Vector3 next) {
        float checkpointDistance = Vector3.Distance(curr, next);
        float currentCheckpointDistance = Vector3.Distance(curr, vehicle);
        float nextCheckpointDistance = Vector3.Distance(next, vehicle);

        float AriaT = Aria(checkpointDistance, currentCheckpointDistance, nextCheckpointDistance);
        float h = (2 * AriaT) / checkpointDistance;

        if (Vector3.Angle(next - curr, vehicle - curr) >= 90)
            return 0;
        else if (Vector3.Angle(curr - next, vehicle - next) >= 90)
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
        if (t < 0) t = 0;
        return Mathf.Sqrt(t) * (t < 0 ? -1 : 1);
    }

    float Aria(float a, float b, float c) {
        float s = (a + b + c) / 2;
        float t = s * (s - a) * (s - b) * (s - c);
        return Mathf.Sqrt(t);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        if(vehicleData.currentCheckpoint != null && vehicleData.nextCheckpoint != null)
            Gizmos.DrawLine(vehicleData.currentCheckpoint.position, vehicleData.nextCheckpoint.position);

        Gizmos.DrawSphere(vehicleData.RoadCenter, 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(vehicle.transform.position, vehicleData.RoadCenter);

    }

    public void ResetVehicleData() {
        vehicleData = new VehicleData(vehicle, CheckpointsContainer.Checkpoints[0], CheckpointsContainer.Checkpoints[1]);
    }

}
