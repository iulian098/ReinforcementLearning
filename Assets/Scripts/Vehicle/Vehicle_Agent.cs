using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Vehicle_Agent : Agent
{
    [SerializeField] Vehicle vehicle;
    [SerializeField] VehicleManager checkpointManager;
    [SerializeField] LayerMask layers;
    [SerializeField] LayerMask forwardLayers;
    [SerializeField] float sideSensorsDistance;
    [SerializeField] float frontSensorsDistance;
    [SerializeField] float rearSensorDistance;
    [SerializeField] float velocitySensorMultiplier = 0.5f;

    [SerializeField] bool showDebug = false;
    [SerializeField] VehicleSensor vehicleSensor;
    [SerializeField] VehicleTrigger frontTrigger;


    //Inputs
    float acc;
    float steer;
    bool handbrake;
    bool nos;

    int lastSteerAction = 0;

    float lapTime;
    float bestLapTime;
    float lastCheckpointDistance;
    float normalizedYRotation;
    float checkpointDirection;
    float roadCenterDirection;
    float checkpointDotProduct;
    float lastCheckpointDotProduct;
    float velocityMagnitude;
    HashSet<Transform> prevCheckpoints = new HashSet<Transform>();

    bool finishReached = false;
    bool frontSensor = false, backSensor = false;
    bool canUseNOS;
    bool wasGoingWrongWay;

    Vector3 nextCheckpointPosition;
    Vector3 startingPos;
    Vector3 vehicleVelocity;
    Vector3 vehicleAngularVelocity;
    Quaternion startingRot;

    private void Reset() {
        vehicle = GetComponent<Vehicle>();
    }

    private void Start() {
        startingPos = vehicle.transform.position;
        startingRot = vehicle.transform.rotation;
        checkpointManager.OnNextCheckpointReached += OnCheckpointReached;
        checkpointManager.OnPreviousCheckpointReached += OnPreviousCheckpointReached;
        checkpointManager.OnFinishReached += OnFinishReached;
        frontTrigger.OnTriggered += OnFrontVehicleDetected;
    }

    private void OnFinishReached() {
        AddReward(1);
        if(showDebug)
            Debug.Log("OnLoopComplete");

        if (bestLapTime != -1 && lapTime < bestLapTime) {
            AddReward(1);
            bestLapTime = lapTime;
        }
        else if (bestLapTime == -1)
            bestLapTime = lapTime;
        lapTime = 0;
    }

    private void OnPlacementChanged(bool obj) {

        if(showDebug)
            Debug.Log($"OnPlacementChanged {obj}");
        AddReward(obj ? 0.75f : -0.75f);
    }

    private void OnPreviousCheckpointReached(Transform checkpoint) {
        Debug.Log("Previous checkpoint reached");
        AddReward(-1f);
        if(!prevCheckpoints.Contains(checkpoint))
            prevCheckpoints.Add(checkpoint);
    }

    private void OnCheckpointReached(Transform checkpoint) {
        AddReward(0.25f);
        nextCheckpointPosition = checkpointManager.vehicleData.nextCheckpoint.position;
        lastCheckpointDistance = Vector3.Distance(vehicle.transform.position, checkpointManager.vehicleData.nextCheckpoint.position);
        prevCheckpoints.Clear();
    }

    public override void OnEpisodeBegin() {
        Debug.Log("OnEpisodeBegin");

        checkpointManager.OnPlacementChanged -= OnPlacementChanged;

        vehicle.ResetVehicle();
        vehicle.transform.SetLocalPositionAndRotation(startingPos, startingRot);
        checkpointManager.ResetVehicleData();
        nextCheckpointPosition = checkpointManager.vehicleData.nextCheckpoint.position;
        lastCheckpointDistance = Vector3.Distance(vehicle.transform.position, nextCheckpointPosition);
        prevCheckpoints.Clear();
        finishReached = false;
        frontSensor = false;
        backSensor = false;
        bestLapTime = -1;
        lapTime = 0;

        checkpointManager.OnPlacementChanged += OnPlacementChanged;
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(new Vector2(vehicleVelocity.x, vehicleVelocity.z));
        sensor.AddObservation(vehicleSensor.HitFractions);
        sensor.AddObservation(checkpointDirection);
        sensor.AddObservation(roadCenterDirection);
        sensor.AddObservation(normalizedYRotation);
        sensor.AddObservation(RearSideSlip());
        sensor.AddObservation(backSensor);
        sensor.AddObservation(vehicleSensor.TagHit);
        sensor.AddObservation(canUseNOS);
    }

    public override void OnActionReceived(ActionBuffers actions) {
        if (finishReached) return;
        handbrake = false;
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
            case 3: //Not used
                acc = 0;
                handbrake = true;
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

        nos = actions.DiscreteActions[2] == 1;
            

        if (showDebug) {
            Debug.Log($"[VehicleAgent]Steer: {steer}");
            Debug.Log($"[VehicleAgent]Acc: {acc}");
        }

        vehicle.ReceiveInput(new Vehicle.InputData() {
            steer = steer,
            acceleration = acc,
            handbrake = handbrake,
            nos = nos
        });

        if (backSensor && acc < 0)
            AddReward(-0.005f);

        if (!frontSensor && acc > 0 && checkpointDotProduct > 0) {
            float speedReward = vehicleVelocity.z * 0.0001f;//Mathf.InverseLerp(20, 120, vehicle.Velocity.z) / 100;
            AddReward(speedReward);
            //AddReward(0.005f);
        }
        if (frontSensor && acc > 0)
            AddReward(-0.005f);
        else if (!frontSensor && acc < 0)
            AddReward(-0.005f);
        else if (frontSensor && acc < 0)
            AddReward(0.005f);

        /*if (rightSensor && steer > 0)
            AddReward(-0.005f);

        if(leftSensor && steer < 0)
            AddReward(-0.005f);*/

        if (steer != lastSteerAction) {
            if(actions.DiscreteActions[1] != 0)
                AddReward(-0.001f);
            lastSteerAction = actions.DiscreteActions[1];
        }

        //Vehicle it's not facing the checkpoint
        if (checkpointDotProduct < 0) {
            if (!wasGoingWrongWay) {
                wasGoingWrongWay = true;
                AddReward(-1);
            }

            AddReward(-0.001f);

            if (checkpointDotProduct > lastCheckpointDotProduct)
                AddReward(0.005f);
            else
                AddReward(-0.001f);
            lastCheckpointDotProduct = checkpointDotProduct;

        }else {
            if (wasGoingWrongWay) {
                wasGoingWrongWay = false;
                AddReward(0.5f);
            }
        }

        if (velocityMagnitude <= 1f)
            AddReward(-0.005f);

        if (nos && (!canUseNOS || acc != 1 || vehicle.Kmph < 10))
            AddReward(-0.001f);

    }

    private void OnFrontVehicleDetected() {
        AddReward(-0.1f);
    }

    public void ResetVehicleData() {
        vehicle.ResetVehicle();
        vehicle.transform.SetLocalPositionAndRotation(startingPos, startingRot);
        checkpointManager.ResetVehicleData();
        nextCheckpointPosition = checkpointManager.vehicleData.nextCheckpoint.position;
        lastCheckpointDistance = Vector3.Distance(vehicle.transform.position, nextCheckpointPosition);
        prevCheckpoints.Clear();
    }

    private void Update() {
        normalizedYRotation = vehicle.transform.rotation.eulerAngles.y / 180f - 1f;
        vehicleVelocity = vehicle.transform.InverseTransformDirection(vehicle.VehicleRigidBody.velocity);
        vehicleAngularVelocity = vehicle.VehicleRigidBody.angularVelocity;
        checkpointDotProduct = Vector3.Dot(vehicle.transform.forward, (nextCheckpointPosition - vehicle.transform.position).normalized);
        lapTime += Time.deltaTime;
        canUseNOS = vehicle.NOSFraction > 0.4f;
    }

    private void FixedUpdate() {
        if (finishReached) return;
        nextCheckpointPosition = checkpointManager.vehicleData.nextCheckpoint.position;
        float checkpointDistance = Vector3.Distance(vehicle.transform.position, nextCheckpointPosition);
        velocityMagnitude = vehicle.VehicleRigidBody.velocity.magnitude;

        float frontSensorCalc = CalcSensorDistance(frontSensorsDistance, vehicleVelocity.z < 0 ? 0 : vehicleVelocity.z, velocitySensorMultiplier);
        float rearSensorCalc = CalcSensorDistance(rearSensorDistance, vehicleVelocity.z > 0 ? 0 : vehicleVelocity.z, velocitySensorMultiplier);

        frontSensor = Physics.Raycast(vehicle.transform.position,
            Vector3.Scale(vehicle.transform.forward, new Vector3(1, 0, 1)),
            vehicleVelocity.z <= 0 ? frontSensorsDistance : frontSensorCalc,
            forwardLayers);

        backSensor = Physics.Raycast(vehicle.transform.position,
            -Vector3.Scale(vehicle.transform.forward, new Vector3(1, 0, 1)),
            vehicleVelocity.z >= 0 ? rearSensorDistance : rearSensorCalc,
            forwardLayers);

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

        if(prevCheckpoints.Count >= 2) {
            vehicle.GetComponent<VehicleManager>().ResetVehiclePosition();
            prevCheckpoints.Clear();
            AddReward(-1);
        }

    }

    float RearSideSlip() {
        float sum = 0;
        for (int i = 0; i < vehicle.RearWheels.Length; i++)
            sum += vehicle.RearWheels[i].GetSideSlip();

        return sum / 2f;//Mathf.Clamp(sum / 2f, -1f, 1f);
    }

    float CalcSensorDistance(float dist, float vel, float mult) {
        return dist + Mathf.Abs(vel * mult);
    }

    private void OnCollisionEnter(Collision collision) {

        if (collision.collider.CompareTag("Player")) {
            /*if(Vector3.Dot(transform.forward, transform.position - collision.collider.transform.position) < 0)
                AddReward(-0.5f);*/
            return;
        }

        AddReward(-0.5f);
    }

    private void OnCollisionStay(Collision collision) {
        if (collision.collider.CompareTag("Player")) {
            return;
        }
        AddReward(-0.0001f);
    }

    private void OnDrawGizmosSelected() {
        float frontSensorCalc = CalcSensorDistance(frontSensorsDistance, vehicleVelocity.z, velocitySensorMultiplier);
        float rearSensorCalc = CalcSensorDistance(rearSensorDistance, vehicleVelocity.z > 0 ? 0 : vehicleVelocity.z, velocitySensorMultiplier);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(vehicle.transform.position, vehicle.transform.position + Vector3.Scale(vehicle.transform.forward, new Vector3(1, 0, 1)) * (vehicleVelocity.z <= 0 ? frontSensorsDistance : frontSensorCalc));
        Gizmos.DrawLine(vehicle.transform.position, vehicle.transform.position - Vector3.Scale(vehicle.transform.forward, new Vector3(1, 0, 1)) * (vehicleVelocity.z >= 0 ? rearSensorDistance : rearSensorCalc));
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        bool handbrake = Input.GetButton("Jump");
        bool nos = Input.GetButton("NOS");

        if (horizontal > 0)
            discreteActions[1] = 1;
        else if(horizontal < 0)
            discreteActions[1] = 2;
        else
            discreteActions[1] = 0;

        if (handbrake) {
            discreteActions[0] = 3;
        }
        else {
            if (vertical > 0)
                discreteActions[0] = 1;
            else if (vertical < 0)
                discreteActions[0] = 2;
            else
                discreteActions[0] = 0;
        }

        discreteActions[2] = nos ? 1 : 0;
    }


}
