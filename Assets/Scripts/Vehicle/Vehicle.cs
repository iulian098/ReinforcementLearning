using System;
using Unity.MLAgents.Policies;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    [SerializeField] float maxSteerAngle;
    [SerializeField] float maxTorque;
    [SerializeField] float maxReverseTorque;
    [SerializeField] float maxBrakeTorque;

    [SerializeField] Rigidbody rb;
    [SerializeField] WheelCollider[] wheelsColls;
    [SerializeField] Transform[] wheelsTransforms;

    bool reverse;
    bool isAgent;
    public int currentCheckpoint;
    public int currentPlacement;

    public Rigidbody VehicleRigidBody => rb;

    void Start()
    {
        if (TryGetComponent(out Vehicle_Agent agent) || TryGetComponent(out BehaviorParameters behavior))
            isAgent = true;
    }

    void Update()
    {
        UpdateWheels();
        if (isAgent) return;

        ReceiveInput(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    private void FixedUpdate() {
        if (isAgent) return;
       /* if (Vector3.Distance(transform.position, VehicleCheckpoints.Instance.Checkpoints[currentCheckpoint].position) < 2) {
            currentCheckpoint++;
            if (currentCheckpoint >= VehicleCheckpoints.Instance.Checkpoints.Length)
                currentCheckpoint = VehicleCheckpoints.Instance.Checkpoints.Length - 1;
        }*/
    }

    public void Steer(float val) {
        wheelsColls[0].steerAngle = val * maxSteerAngle;
        wheelsColls[1].steerAngle = val * maxSteerAngle;
    }

    public void Accelerate(float val) {

        if (rb.velocity.magnitude < 0.1f) {
            reverse = val < 0;
        }
        /*}else if(val > 0 && rb.velocity.magnitude > 0.1f) {
            reverse = false;
        }*/
        //Debug.Log(val);
        if (val < 0) {
            foreach (var wColl in wheelsColls) {
                wColl.motorTorque = reverse ? maxReverseTorque * val : 0;
                wColl.brakeTorque = reverse ? 0 : maxBrakeTorque * Mathf.Abs(val);
            }
        }
        else if(val > 0){
            foreach (var wColl in wheelsColls) {
                wColl.motorTorque = reverse ? 0 : maxTorque * val;
                wColl.brakeTorque = reverse ? maxBrakeTorque * Mathf.Abs(val) : 0;
            }
        }
        else {
            foreach (var wColl in wheelsColls) {
                wColl.motorTorque = 0;
                wColl.brakeTorque = 0;
            }
        }
    }

    public void UpdateWheels() {
        for (int i = 0; i < wheelsColls.Length; i++) {
            Quaternion wRot;
            Vector3 wPos;

            wheelsColls[i].GetWorldPose(out wPos, out wRot);
            wheelsTransforms[i].SetPositionAndRotation(wPos, wRot);
        }
    }

    public void ResetVehicle() {
        foreach (var item in wheelsColls) {
            item.motorTorque = 0;
            item.brakeTorque = 0;
        }

        rb.velocity = Vector3.zero;
    }

    public void ReceiveInput(float steering, float acc) {
        Steer(steering);
        Accelerate(acc);
    }
}
