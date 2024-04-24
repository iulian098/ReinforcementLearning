using System;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine;

public class VehicleManager : MonoBehaviour
{
    const float NO_COLLISION_TIME = 3;
    const string NO_COLLISION_LAYER = "NoCollision";
    const string VEHICLE_LAYER = "Vehicle";

    [SerializeField] string playerName;
    [SerializeField] bool isPlayer;
    [SerializeField] Vehicle vehicle;

    public VehicleConfig vehicleConfig;
    public VehicleData vehicleData;
    public VehicleSaveData vehicleSaveData;
    public int currentPlacement;

    Collider[] colls;
    Dictionary<Collider, int> defaultLayers = new Dictionary<Collider, int>();
    float nextCheckpointDistance;
    float prevCheckpointDistance;
    float noCollisionTimer;

    public Action<Transform> OnNextCheckpointReached;
    public Action<Transform> OnPreviousCheckpointReached;
    public Action OnFinishReached;
    public Action<bool> OnPlacementChanged;
    public Action<VehicleManager> OnRaceFinished;
    public Action OnPositionReset;

    bool initialized = false;

    public string PlayerName => playerName;
    public bool Initialized => initialized;
    public bool IsPlayer => isPlayer;

    public Vehicle Vehicle => vehicle;
    VehicleCheckpointsContainer CheckpointsContainer => VehicleCheckpointsContainer.Instance;

    public void Init(VehicleSaveData vehicleSaveData, bool isPlayer = false) {
        Init(vehicleConfig, vehicleSaveData, isPlayer);
        /*vehicle.Init(vehicleConfig, vehicleSaveData, isPlayer);
        ResetVehicleData();
        this.vehicleSaveData = vehicleSaveData;
        this.isPlayer = isPlayer;
        initialized = true;

        if (isPlayer) {
            if (gameObject.TryGetComponent<DecisionRequester>(out var decReq))
                DestroyImmediate(decReq);
            if (gameObject.TryGetComponent<Vehicle_Agent_v2>(out var agent))
                DestroyImmediate(agent);
            if (gameObject.TryGetComponent<BehaviorParameters>(out var behaviorParameters))
                Destroy(behaviorParameters);
        }*/
    }

    public void Init(VehicleConfig config, VehicleSaveData vehicleSaveData, bool isPlayer = false) {
        ResetVehicleData();
        this.vehicleSaveData = vehicleSaveData;
        this.isPlayer = isPlayer;
        vehicleConfig = config;
        initialized = true;
        vehicle.Init(vehicleConfig, vehicleSaveData, isPlayer);

        if (isPlayer) {
            if (gameObject.TryGetComponent<DecisionRequester>(out var decReq))
                Destroy(decReq);
            if(gameObject.TryGetComponent<BehaviorParameters>(out var behaviorParameters))
                Destroy(behaviorParameters);
            if(gameObject.TryGetComponent<Vehicle_Agent>(out var agent))
                Destroy(agent);
        }

        colls = GetComponentsInChildren<Collider>().Where(x => !x.isTrigger && x.gameObject.layer == LayerMask.NameToLayer(VEHICLE_LAYER)).ToArray();
    }

    void Update() {
        if (!initialized) return;
        nextCheckpointDistance = Vector3.Distance(vehicle.transform.position, vehicleData.nextCheckpoint.position);
        prevCheckpointDistance = Vector3.Distance(vehicle.transform.position, vehicleData.currentCheckpoint.position);
        float checkpointDist = GetCurrentCheckpointDistance(vehicle.transform.position, vehicleData.currentCheckpoint.position, vehicleData.nextCheckpoint.position);
        float dist = Vector3.Distance(vehicleData.currentCheckpoint.position, vehicleData.nextCheckpoint.position);
        float distFactor = checkpointDist == 0 ? 0 : checkpointDist / dist;
        vehicleData.totalDistance = RaceManager.Instance.Distances[vehicleData.checkpointIndex] + distFactor * dist;

        //Set vehicle road center
        vehicleData.RoadCenter = GetRoadCenter(vehicle.transform.position, vehicleData.currentCheckpoint.position, vehicleData.nextCheckpoint.position);

        bool goingForward = vehicle.VehicleRigidBody.velocity.magnitude >= 0.1f && Vector3.Dot(vehicle.VehicleRigidBody.velocity, (vehicleData.nextCheckpoint.position - vehicleData.currentCheckpoint.position).normalized) > 0;

        // Check if the vehicle is moving forward based on velocity
        if (!vehicleData.finished && vehicleData.checkpointIndex == 0 && vehicleData.PassedCheckpoints.Count >= CheckpointsContainer.Checkpoints.Length && goingForward && PassedAllCheckpoints()) {
            vehicleData.loopCount++;
            vehicleData.PassedCheckpoints.Clear();
            OnFinishReached?.Invoke();
            if (vehicleData.loopCount >= RaceManager.Instance.RaceData.MaxLoops && !RaceManager.Instance.enableLearning)
                OnRaceFinished?.Invoke(this);
        }

        if (!CheckpointsContainer.UseTriggers && (nextCheckpointDistance < CheckpointsContainer.CheckpointRadius || prevCheckpointDistance < CheckpointsContainer.CheckpointRadius)) {
            if (distFactor >= 1f && goingForward)
                SetNextCheckpoint(goingForward);
            else if (distFactor <= 0 && !goingForward)
                SetPreviousCheckpoint(goingForward);
        }

        if(noCollisionTimer > 0) {
            noCollisionTimer -= Time.deltaTime;
            if(noCollisionTimer <= 0) {
                foreach (var item in colls) {
                    item.gameObject.layer = LayerMask.NameToLayer(VEHICLE_LAYER);//LayerMask.GetMask(VEHICLE_LAYER);
                }
            }
        }
    }

    public void SetPlayerName(string name) {
        playerName = name;
    }

    void SetNextCheckpoint(bool goingForward) {
        if (!goingForward) return;
        Transform checkpoint = vehicleData.nextCheckpoint;
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
            OnNextCheckpointReached?.Invoke(checkpoint);
    }

    void SetPreviousCheckpoint(bool goingForward) {
        if (goingForward) return;
        Transform checkpoint = vehicleData.currentCheckpoint;
        vehicleData.nextCheckpoint = vehicleData.currentCheckpoint;
        if (vehicleData.checkpointIndex - 1 < 0)
            vehicleData.checkpointIndex = CheckpointsContainer.Checkpoints.Length - 1;
        else
            vehicleData.checkpointIndex--;
        vehicleData.currentCheckpoint = CheckpointsContainer.Checkpoints[vehicleData.checkpointIndex];

        OnPreviousCheckpointReached?.Invoke(checkpoint);
    }

    bool PassedAllCheckpoints() {
        for (int i = 0; i < vehicleData.PassedCheckpoints.Count; i++)
            if (vehicleData.PassedCheckpoints[i] != CheckpointsContainer.Checkpoints[i])
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

        if (vehicleData.currentCheckpoint == null || vehicleData.nextCheckpoint == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(vehicleData.RoadCenter, 0.5f);
        Gizmos.DrawLine(vehicle.transform.position, vehicleData.RoadCenter);

        Gizmos.color = Color.yellow;
        if(vehicleData.currentCheckpoint != null && vehicleData.nextCheckpoint != null)
            Gizmos.DrawLine(vehicleData.currentCheckpoint.position, vehicleData.nextCheckpoint.position);

    }

    public void ResetVehicleData() {
        vehicleData = new VehicleData(CheckpointsContainer.Checkpoints[0], CheckpointsContainer.Checkpoints[1]);
    }

    public void ResetVehiclePosition() {
        Vector3 relativePos = vehicleData.nextCheckpoint.position - vehicleData.currentCheckpoint.position;
        Vector3 vehPos = (vehicleData.nextCheckpoint.position + vehicleData.currentCheckpoint.position) / 2;
        Vector3 roadPos = GetRoadCenter(vehPos, vehicleData.currentCheckpoint.position, vehicleData.nextCheckpoint.position);
        vehicle.transform.position = roadPos;
        vehicle.transform.rotation = Quaternion.LookRotation(relativePos, Vector3.up);
        vehicle.VehicleRigidBody.velocity = Vector3.zero;
        noCollisionTimer = NO_COLLISION_TIME;

        foreach (var item in colls) {
            item.gameObject.layer = LayerMask.NameToLayer(NO_COLLISION_LAYER);
        }

        OnPositionReset?.Invoke();

    }

    private void OnTriggerEnter(Collider other) {
        if (CheckpointsContainer.UseTriggers) {

            if (other.CompareTag("Checkpoint")) {
                bool goingForward = vehicle.VehicleRigidBody.velocity.magnitude >= 0.1f && Vector3.Dot(vehicle.VehicleRigidBody.velocity, (vehicleData.nextCheckpoint.position - vehicleData.currentCheckpoint.position).normalized) > 0;

                if (other.transform == vehicleData.nextCheckpoint && goingForward)
                    SetNextCheckpoint(goingForward);
                else if (other.transform == vehicleData.currentCheckpoint && !goingForward)
                    SetPreviousCheckpoint(goingForward);
            }
        }
    }

    private void OnTriggerStay(Collider other) {
        if (CheckpointsContainer.UseTriggers) {

            if (other.CompareTag("Checkpoint")) {
                bool goingForward = vehicle.VehicleRigidBody.velocity.magnitude >= 0.1f && Vector3.Dot(vehicle.VehicleRigidBody.velocity, (vehicleData.nextCheckpoint.position - vehicleData.currentCheckpoint.position).normalized) > 0;

                if (other.transform == vehicleData.nextCheckpoint && goingForward)
                    SetNextCheckpoint(goingForward);
                else if (other.transform == vehicleData.currentCheckpoint && !goingForward)
                    SetPreviousCheckpoint(goingForward);
            }
        }
    }
}
