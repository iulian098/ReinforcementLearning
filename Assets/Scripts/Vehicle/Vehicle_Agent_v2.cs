using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Vehicle_Agent_v2 : Agent
{
    [SerializeField] Vehicle vehicle;
    [SerializeField] VehicleCheckpointManager checkpointManager;
    [SerializeField] Transform spawnPoint;
    [SerializeField] LayerMask layers;
    [SerializeField] LayerMask forwardLayers;
    [SerializeField] float sideSensorsDistance;
    [SerializeField] float frontSensorsDistance;
    [SerializeField] float velocitySensorMultiplier = 0.5f;

    [SerializeField] bool loopTrack = false;
    [SerializeField] bool showDebug = false;
    [SerializeField] VehicleSensor vehicleSensor;

    [Header("Debugging")]
    [SerializeField] float velocityMagnitude;

    float acc;
    float steer;

    float lastCheckpointDistance;
    float normalizedYRotation;
    float checkpointDirection;
    float roadCenterDirection;

    bool finishReached = false;
    bool leftSensor = false, rightSensor = false, frontSensor = false, backSensor = false;

    Vector3 nextCheckpointPosition;
    Vector3 startingPos;
    Vector3 vehicleVelocity;
    Vector3 vehicleAngularVelocity;
    Quaternion startingRot;

    private void Start() {
        startingPos = vehicle.transform.position;
        startingRot = vehicle.transform.rotation;
        checkpointManager.OnNextCheckpointReached += OnCheckpointReached;
        checkpointManager.OnPreviousCheckpointReached += OnPreviousCheckpointReached;
        checkpointManager.OnPlacementChanged += OnPlacementChanged;
        checkpointManager.OnFinishReached += OnFinishReached;
    }


    private void OnFinishReached() {
        AddReward(1);
        Debug.Log("OnLoopComplete");
    }

    private void OnPlacementChanged(bool obj) {
        //Debug.Log("OnPlacementChanged");
    }

    private void OnPreviousCheckpointReached() {
        Debug.Log("Previous checkpoint reached");
        AddReward(-0.25f);
    }

    private void OnCheckpointReached() {
        AddReward(0.25f);
        nextCheckpointPosition = checkpointManager.vehicleData.nextCheckpoint.position;
        lastCheckpointDistance = Vector3.Distance(vehicle.transform.position, checkpointManager.vehicleData.nextCheckpoint.position);

    }

    public override void OnEpisodeBegin() {
        vehicle.ResetVehicle();
        vehicle.transform.SetLocalPositionAndRotation(startingPos, startingRot);
        checkpointManager.ResetVehicleData();
        nextCheckpointPosition = checkpointManager.vehicleData.nextCheckpoint.position;//VehicleCheckpointsContainer.Instance.Checkpoints[vehicle.currentCheckpoint].transform.position;
        lastCheckpointDistance = Vector3.Distance(vehicle.transform.position, nextCheckpointPosition);

        finishReached = false;
        leftSensor = false;
        rightSensor = false;
        frontSensor = false;
        backSensor = false;
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(new Vector2(vehicleVelocity.x, vehicleVelocity.z));
        sensor.AddObservation(vehicleSensor.HitFractions);
        sensor.AddObservation(checkpointDirection);
        sensor.AddObservation(roadCenterDirection);
        sensor.AddObservation(normalizedYRotation);
        sensor.AddObservation(vehicle.SideSlip);
        sensor.AddObservation(backSensor);
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
        vehicleVelocity = vehicle.transform.InverseTransformDirection(vehicle.VehicleRigidBody.velocity);
        vehicleAngularVelocity = vehicle.VehicleRigidBody.angularVelocity;
    }

    private void FixedUpdate() {
        if (finishReached) return;
        nextCheckpointPosition = checkpointManager.vehicleData.nextCheckpoint.position;
        float checkpointDistance = Vector3.Distance(vehicle.transform.position, nextCheckpointPosition);
        velocityMagnitude = vehicle.VehicleRigidBody.velocity.magnitude;

        float sideSensorCalc = CalcSensorDistance(sideSensorsDistance, vehicleAngularVelocity.y, velocitySensorMultiplier);
        float frontSensorCalc = CalcSensorDistance(frontSensorsDistance, vehicleVelocity.z, velocitySensorMultiplier);
        rightSensor = Physics.Raycast(vehicle.transform.position,
            Vector3.Scale(vehicle.transform.right, new Vector3(1, 0, 1)),
            vehicleAngularVelocity.y <= 0 ? sideSensorsDistance : sideSensorCalc,
            forwardLayers);

        leftSensor = Physics.Raycast(vehicle.transform.position,
            -Vector3.Scale(vehicle.transform.right,
            new Vector3(1, 0, 1)),
            vehicleAngularVelocity.y >= 0 ? sideSensorsDistance : sideSensorCalc,
            forwardLayers);

        frontSensor = Physics.Raycast(vehicle.transform.position,
            Vector3.Scale(vehicle.transform.forward, new Vector3(1, 0, 1)),
            vehicleVelocity.z <= 0 ? frontSensorsDistance : frontSensorCalc,
            forwardLayers);

        backSensor = Physics.Raycast(vehicle.transform.position,
            -Vector3.Scale(vehicle.transform.forward, new Vector3(1, 0, 1)),
            vehicleVelocity.z >= 0 ? frontSensorsDistance : frontSensorCalc,
            forwardLayers);

        if (velocityMagnitude <= 0.3f)
            AddReward(-0.005f);

        Vector3 checkpointDirectionVector = vehicle.transform.position - nextCheckpointPosition;
        checkpointDirection = (Vector3.SignedAngle(vehicle.transform.forward, checkpointDirectionVector, Vector3.up) + 180) / 360;
        Vector3 roadCenterDirectionVector = vehicle.transform.position - checkpointManager.vehicleData.RoadCenter;
        roadCenterDirection = (Vector3.SignedAngle(vehicle.transform.forward, roadCenterDirectionVector, Vector3.up) + 180) / 360;

        if (checkpointDistance < lastCheckpointDistance - 0.2f){
            lastCheckpointDistance = checkpointDistance;
            AddReward(0.01f);
        }
        else if(checkpointDistance > lastCheckpointDistance + 0.2f) {
            AddReward(-0.025f);
        }

    }

    float CalcSensorDistance(float dist, float vel, float mult) {
        return dist + Mathf.Abs(vel * mult);
    }

    private void OnCollisionEnter(Collision collision) {
        AddReward(-1f);
    }

    private void OnCollisionStay(Collision collision) {
        AddReward(-0.01f);
    }

    private void OnDrawGizmosSelected() {
        float sideSensorCalc = CalcSensorDistance(sideSensorsDistance, vehicleAngularVelocity.y, velocitySensorMultiplier);
        float frontSensorCalc = CalcSensorDistance(frontSensorsDistance, vehicleVelocity.z, velocitySensorMultiplier);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(vehicle.transform.position, vehicle.transform.position + Vector3.Scale(vehicle.transform.right, new Vector3(1, 0, 1)) * (vehicleAngularVelocity.y <= 0 ? sideSensorsDistance : sideSensorCalc));
        Gizmos.DrawLine(vehicle.transform.position, vehicle.transform.position - Vector3.Scale(vehicle.transform.right, new Vector3(1, 0, 1)) * (vehicleAngularVelocity.y >= 0 ? sideSensorsDistance : sideSensorCalc));
        Gizmos.DrawLine(vehicle.transform.position, vehicle.transform.position + Vector3.Scale(vehicle.transform.forward, new Vector3(1, 0, 1)) * (vehicleVelocity.z <= 0 ? frontSensorsDistance : frontSensorCalc));
        Gizmos.DrawLine(vehicle.transform.position, vehicle.transform.position - Vector3.Scale(vehicle.transform.forward, new Vector3(1, 0, 1)) * (vehicleVelocity.z >= 0 ? frontSensorsDistance : frontSensorCalc));

    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (horizontal > 0)
            discreteActions[1] = 1;
        else if(horizontal < 0)
            discreteActions[1] = 2;
        else
            discreteActions[1] = 0;

        if (vertical > 0)
            discreteActions[0] = 1;
        else if(vertical < 0)
            discreteActions[0] = 2;
        else 
            discreteActions[0] = 0;
    }


}
