using System;
using Unity.MLAgents.Policies;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    public enum Drivetrain {
        FWD,
        RWD,
        AWD
    }

    [System.Serializable]
    public class WheelData {
        [SerializeField] WheelCollider wheelCollider;
        [SerializeField] Transform wheelVisual;
        Skidmarks skidmarks;
        int lastSkidIndex;

        public WheelCollider WheelCollider => wheelCollider;

        public void Init() {
            skidmarks = RaceManager.Instance.SkidmarksManager.Skidmarks;
        }

        public void UpdateVisual() {
            wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            wheelVisual.SetPositionAndRotation(pos, rot);
        }

        public float GetSideSlip() {
            wheelCollider.GetGroundHit(out WheelHit hit);
            return hit.sidewaysSlip;
        }

        public float GetForwardSlip() {
            wheelCollider.GetGroundHit(out WheelHit hit);
            return hit.forwardSlip;
        }

        public void UpdateSkidmarks(float velocity) {
            if (skidmarks == null) return;

            wheelCollider.GetGroundHit(out WheelHit hit);
            if (velocity > 10 && (Mathf.Abs(hit.sidewaysSlip) > 0.5f || Mathf.Abs(hit.forwardSlip) > 0.5f))
                lastSkidIndex = skidmarks.Add(hit.point, hit.normal, 1, lastSkidIndex);
            else
                lastSkidIndex = -1;
        }
    }

    public struct InputData {
        public float steer;
        public float acceleration;
        public bool handbrake;
    }

    [SerializeField] Drivetrain drivetrain;
    [SerializeField] Rigidbody rb;
    [SerializeField] float maxSpeed;
    [Header("Torque")]
    [SerializeField] float maxReverseTorque = 200;
    [SerializeField] float maxBrakeTorque = 500;
    [SerializeField] float handbrakeTorque = 1000;
    [SerializeField] AnimationCurve enginePowerCurve;

    [Header("Gears")]
    [SerializeField] float[] gears;
    [SerializeField] float shiftUpRPM = 5600;
    [SerializeField] float shiftDownRPM = 2500;
    [SerializeField] float differentialRatio = 3.6f;

    [Header("Wheels")]
    [SerializeField] WheelData[] frontWheels;
    [SerializeField] WheelData[] rearWheels;

    [Space, Header("ABS")]
    [SerializeField] bool useABS;
    [SerializeField] float absThreshold = 0.2f;
    [SerializeField] float absBrakeFarctor = 0.5f;
    bool absTriggered;

    [Space, Header("TCS")]
    [SerializeField] bool useTCS;
    [SerializeField] float tcsThreshold = 0.8f;
    [SerializeField] float tcsFactor = 0.5f;

    [Space]
    [SerializeField] float steerRadius;
    [SerializeField] float downForce;
    bool tcsTriggered;

    InputData currentInput;
    WheelData[] allWheels;
    WheelData[] drivingWheels;
    Vector3 velocity;

    bool reverse;
    [SerializeField] bool isAgent;
    bool braking;

    float targetSteer;
    float totalPower;
    float engineRPM;
    float sideSlip;
    float forwardSlip;
    float wheelRPM;

    int kmph;
    int currentGear;

    public Rigidbody VehicleRigidBody => rb;
    public WheelData[] FrontWheels => frontWheels;
    public WheelData[] RearWheels => rearWheels;
    public InputData CurrentInput => currentInput;
    public Vector3 Velocity => velocity;
    public bool ABS => absTriggered;
    public bool TCS => tcsTriggered;
    public bool Braking => braking;
    public float ShiftUpRPM => shiftUpRPM;
    public float ShiftDownRPM => shiftDownRPM;
    public float EngineRPM => engineRPM;
    public float SideSlip => sideSlip;
    public float WheelRPM => wheelRPM;
    public int GearsCount => gears.Length;
    public int Kmph => kmph;
    public int CurrentGear => currentGear;

    public Action OnGearChanged;

    void Start()
    {
       // if (TryGetComponent(out BehaviorParameters _))
        //    isAgent = true;

        allWheels = GetAllWheels();

        foreach (var wheel in allWheels) {
            wheel.Init();
        }

        switch (drivetrain) {
            case Drivetrain.FWD:
                drivingWheels = frontWheels; 
                break;
            case Drivetrain.RWD:
                drivingWheels = rearWheels;
                break;
            case Drivetrain.AWD:
                drivingWheels = allWheels;
                break;
        }
    }

    void Update()
    {
        UpdateWheels();
        ApplyDownForce();
        velocity = transform.InverseTransformDirection(rb.velocity);
        kmph = Mathf.FloorToInt(rb.velocity.magnitude * 3.6f);
        sideSlip = GetSideSlip(rearWheels);
        forwardSlip = GetForwardSlip(allWheels);
        float temp = 0;

        float av = velocity.magnitude / frontWheels[0].WheelCollider.radius;
        wheelRPM = (av / (2 * Mathf.PI)) * 60;

        totalPower = enginePowerCurve.Evaluate(engineRPM) * gears[currentGear];

        if (currentInput.handbrake)
            engineRPM = 1000;
        else
            engineRPM = Mathf.SmoothDamp(engineRPM, 1000 + (Mathf.Abs(wheelRPM) * differentialRatio * gears[currentGear]), ref temp, 0.01f);

        if (engineRPM > shiftUpRPM && currentGear < gears.Length - 1)
            currentGear++;
        else if (engineRPM < shiftDownRPM && currentGear > 0)
            currentGear--;

        foreach (var wheel in allWheels)
            wheel.UpdateSkidmarks(VehicleRigidBody.velocity.magnitude);

        if (isAgent) return;

        ReceiveInput(new InputData() {
            steer = Input.GetAxis("Horizontal"),
            acceleration = Input.GetAxis("Vertical"),
            handbrake = Input.GetButton("Jump")
        });
    }

    WheelData[] GetAllWheels() {
        WheelData[] wheels = new WheelData[frontWheels.Length + rearWheels.Length];
        for (int i = 0; i < frontWheels.Length; i++)
            wheels[i] = frontWheels[i];
        for (int j = 0; j < rearWheels.Length; j++)
            wheels[frontWheels.Length + j] = rearWheels[j];

        return wheels;
    }

    void Steer(float val) {

        //Ackerman
        if (val > 0) {
            frontWheels[0].WheelCollider.steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (steerRadius + (1.5f / 2))) * val;
            frontWheels[1].WheelCollider.steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (steerRadius - (1.5f / 2))) * val;
        }else if(val < 0) {
            frontWheels[0].WheelCollider.steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (steerRadius - (1.5f / 2))) * val;
            frontWheels[1].WheelCollider.steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (steerRadius + (1.5f / 2))) * val;
        }
        else {
            frontWheels[0].WheelCollider.steerAngle = 0;
            frontWheels[1].WheelCollider.steerAngle = 0;
        }
    }

    void Accelerate(float val) {

        if (rb.velocity.magnitude < 0.1f)
            reverse = val < 0;

        if (currentInput.handbrake) {
            ApplyHandbrake();
            return;
        }

        float torque = totalPower / drivingWheels.Length;
        float brake = ABSBrake(maxBrakeTorque * Mathf.Abs(val));

        if (val > 0 && kmph >= maxSpeed)
            val = 0;

        if (val < 0) {
            foreach (var wData in drivingWheels) {
                wData.WheelCollider.motorTorque = reverse ? maxReverseTorque * val : 0;
                wData.WheelCollider.brakeTorque = reverse ? 0 : brake;
            }
            braking = !reverse;
        }
        else if(val > 0){
            foreach (var wData in drivingWheels) {
                wData.WheelCollider.motorTorque = reverse ? 0 : TCSAcceleration(wData, torque * val);
                wData.WheelCollider.brakeTorque = reverse ? maxBrakeTorque * Mathf.Abs(val) : 0;
            }
            braking = reverse;
        }
        else {
            foreach (var wData in drivingWheels) {
                wData.WheelCollider.motorTorque = 0;
                wData.WheelCollider.brakeTorque = 0;
            }
            braking = false;
        }
    }

    void ApplyHandbrake() {
        foreach (var wheel in drivingWheels) {
            wheel.WheelCollider.brakeTorque = handbrakeTorque;
            wheel.WheelCollider.motorTorque = 0;
        }
    }

    void UpdateWheels() {
        for (int f = 0; f < frontWheels.Length; f++)
            frontWheels[f].UpdateVisual();
        for (int r = 0; r < rearWheels.Length; r++)
            rearWheels[r].UpdateVisual();
    }

    void ApplyDownForce() {
        rb.AddForce(-transform.up * downForce * rb.velocity.magnitude, ForceMode.Force);
    }

    float ABSBrake(float brakeForce) {
        if (!useABS) return brakeForce;

        float sum = 0;
        for (int i = 0; i < allWheels.Length; i++)
            sum += Mathf.Abs(allWheels[i].GetForwardSlip()) + Mathf.Abs(allWheels[i].GetSideSlip());

        float avg = sum / allWheels.Length;

        if (avg >= absThreshold) {
            absTriggered = true;
            return brakeForce * absBrakeFarctor;
        }
        else
            absTriggered = false;
        return brakeForce;
    }

    float TCSAcceleration(WheelData wheel, float acc) {
        if (!useABS) return acc;

        if(Mathf.Abs(wheel.GetForwardSlip()) >= tcsThreshold) {
            tcsTriggered = true;
            return acc * tcsFactor;
        }else
            tcsTriggered = false;

        return acc;
    }

    float GetForwardSlip(WheelData[] wheels) {
        float sum = 0;
        for (int i = 0; i < wheels.Length; i++)
            sum += wheels[i].GetForwardSlip();

        return sum / wheels.Length;
    }

    float GetSideSlip(WheelData[] wheels) {
        float sum = 0;
        foreach (var wheel in wheels)
            sum += wheel.GetSideSlip();

        return Mathf.Clamp(sum / wheels.Length, -1f, 1f);
    }

    float GetWheelsRPM() {
        float sum = 0;

        for (int i = 0; i < allWheels.Length; i++)
            sum = allWheels[i].WheelCollider.rpm;
        
        return sum == 0 ? 0 : sum / allWheels.Length;
    }

    public void ResetVehicle() {
        foreach (var item in allWheels) {
            item.WheelCollider.motorTorque = 0;
            item.WheelCollider.brakeTorque = 0;
        }

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void ReceiveInput(InputData input) {
        if (RaceManager.Instance.CurrentState != RaceManager.State.Playing) return;
        currentInput = input;
        targetSteer = Mathf.Lerp(targetSteer, input.steer, Time.deltaTime * 25);
        Steer(targetSteer);
        Accelerate(input.acceleration);
    }
}
