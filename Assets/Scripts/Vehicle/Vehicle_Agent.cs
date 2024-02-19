using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Vehicle_Agent : Agent
{
    [SerializeField] Vehicle vehicle;
    [SerializeField] Transform spawnPoint;
    [SerializeField] LayerMask layers;
    [SerializeField] LayerMask forwardLayers;
    [SerializeField] float sideSensorsDistance;
    [SerializeField] float frontSensorsDistance;

    [SerializeField] bool showDebug = false;

    [Header("Debugging")]
    [SerializeField] float velocityMagnitude;

    float acc;
    float steer;

    float lastCheckpointDistance;
    float farthestDistance;
    float normalizedYRotation;
    float prevYRotation;

    bool finishReached = false;
    bool leftSensor = false, rightSensor = false, frontSensor = false, backSensor = false;

    Vector3 nextCheckpointPosition;
    Vector3 startingPos;
    Quaternion startingRot;

    private void Start() {
        startingPos = vehicle.transform.position;
        startingRot = vehicle.transform.rotation;
    }

    public override void OnEpisodeBegin() {
        vehicle.ResetVehicle();
        vehicle.transform.SetLocalPositionAndRotation(startingPos, startingRot);
        vehicle.currentCheckpoint = 0;

        nextCheckpointPosition = VehicleCheckpoints.Instance.Checkpoints[vehicle.currentCheckpoint].transform.position;
        lastCheckpointDistance = Vector3.Distance(vehicle.transform.position, nextCheckpointPosition);
        farthestDistance = Vector3.Distance(vehicle.transform.position, nextCheckpointPosition);

        finishReached = false;
        leftSensor = false;
        rightSensor = false;
        frontSensor = false;
        backSensor = false;
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(velocityMagnitude / 20);
        sensor.AddObservation(new Vector2(vehicle.transform.position.x, vehicle.transform.position.z));
        sensor.AddObservation(new Vector2(nextCheckpointPosition.x, nextCheckpointPosition.z));
        sensor.AddObservation(normalizedYRotation);
        sensor.AddObservation(backSensor);
        sensor.AddObservation(frontSensor);
        sensor.AddObservation(leftSensor);
        sensor.AddObservation(rightSensor);
    }

    public override void OnActionReceived(ActionBuffers actions) {
        if (finishReached) return;

        switch (actions.DiscreteActions[0]) {
            case 0:
                acc = 0;
                break;
            case 1:
                acc = 1;
                break;
             case 2:
                acc = -1;
                break;
        }

        switch (actions.DiscreteActions[1]) {
            case 0:
                steer = 0;
                break;
            case 1:
                steer = 1;
                break;
            case 2:
                steer = -1;
                break;
        }

        if (showDebug) {
            Debug.Log($"[VehicleAgent]Steer: {steer}");
            Debug.Log($"[VehicleAgent]Acc: {acc}");
        }

        vehicle.ReceiveInput(steer, acc);


        if (!frontSensor && acc > 0 && velocityMagnitude > 0.3f)
            AddReward(0.005f);

        if (frontSensor && acc > 0)
            AddReward(-0.005f);
        if (backSensor && acc < 0)
            AddReward(-0.005f);
        
        if(rightSensor && steer > 0)
            AddReward(-0.005f);
        if(leftSensor && steer < 0)
            AddReward(-0.005f);

    }

    private void Update() {
        normalizedYRotation = vehicle.transform.rotation.eulerAngles.y / 180f - 1f;
    }

    private void FixedUpdate() {
        if (finishReached) return;

        float checkpointDistance = Vector3.Distance(vehicle.transform.position, nextCheckpointPosition);
        velocityMagnitude = vehicle.VehicleRigidBody.velocity.magnitude;

        rightSensor = Physics.Raycast(vehicle.transform.position, Vector3.Scale(vehicle.transform.right, new Vector3(1, 0, 1)), sideSensorsDistance, forwardLayers);
        leftSensor = Physics.Raycast(vehicle.transform.position, -Vector3.Scale(vehicle.transform.right, new Vector3(1, 0, 1)), sideSensorsDistance, forwardLayers);
        frontSensor = Physics.Raycast(vehicle.transform.position, Vector3.Scale(vehicle.transform.forward, new Vector3(1, 0, 1)), frontSensorsDistance, forwardLayers);
        backSensor = Physics.Raycast(vehicle.transform.position, -Vector3.Scale(vehicle.transform.forward, new Vector3(1, 0, 1)), frontSensorsDistance, forwardLayers);

        if (velocityMagnitude <= 0.3f)
            AddReward(-0.005f);

        if (vehicle.currentCheckpoint < VehicleCheckpoints.Instance.Checkpoints.Length - 1 && checkpointDistance < 5) {
            vehicle.currentCheckpoint++;
            nextCheckpointPosition = VehicleCheckpoints.Instance.Checkpoints[vehicle.currentCheckpoint].transform.position;
            checkpointDistance = Vector3.Distance(vehicle.transform.position, nextCheckpointPosition);
            lastCheckpointDistance = checkpointDistance;
            AddReward(0.25f);
        }

        /*if (checkpointDistance < lastCheckpointDistance - 0.2f){
            lastCheckpointDistance = checkpointDistance;
            farthestDistance = checkpointDistance;
            AddReward(0.01f);
        }
        else if(checkpointDistance > farthestDistance + 0.2f) {
            AddReward(-0.025f);
            farthestDistance = checkpointDistance;
        }*/

        if (checkpointDistance < lastCheckpointDistance - 0.2f){
            lastCheckpointDistance = checkpointDistance;
            AddReward(0.01f);
        }
        else if(checkpointDistance > lastCheckpointDistance + 0.2f) {
            AddReward(-0.025f);
        }

    }

    private void OnTriggerEnter(Collider other) {
        if (!finishReached && other.CompareTag("Finish") && vehicle.currentCheckpoint == VehicleCheckpoints.Instance.Checkpoints.Length - 1) {
            AddReward(1f);
            AddReward(1 - StepCount / MaxStep);
            finishReached = true;
            Debug.Log("<color=green>Finish</color>");
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision) {
        AddReward(-1f);
    }

    private void OnCollisionStay(Collision collision) {
        AddReward(-0.01f);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(vehicle.transform.position, vehicle.transform.position + Vector3.Scale(vehicle.transform.right, new Vector3(1, 0, 1)) * sideSensorsDistance);
        Gizmos.DrawLine(vehicle.transform.position, vehicle.transform.position - Vector3.Scale(vehicle.transform.right, new Vector3(1, 0, 1)) * sideSensorsDistance);
        Gizmos.DrawLine(vehicle.transform.position, vehicle.transform.position + Vector3.Scale(vehicle.transform.forward, new Vector3(1, 0, 1)) * frontSensorsDistance);
        Gizmos.DrawLine(vehicle.transform.position, vehicle.transform.position - Vector3.Scale(vehicle.transform.forward, new Vector3(1, 0, 1)) * frontSensorsDistance);
    }
}
