using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Vehicle : MonoBehaviour
{
    public enum Drivetrain {
        FWD,
        RWD,
        AWD
    }

    [Serializable]
    public class WheelData {
        [SerializeField] WheelCollider wheelCollider;
        [SerializeField] Transform wheelVisual;
        Skidmarks skidmarks;
        int lastSkidIndex;
        bool isSliding;

        public WheelCollider WheelCollider => wheelCollider;
        public bool IsSliding => isSliding;

        public void Init() {
            if(SkidmarksManager.Instance != null)
                skidmarks = SkidmarksManager.Instance.Skidmarks;
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
            if (velocity > 10 && (Mathf.Abs(hit.sidewaysSlip) > 0.5f || Mathf.Abs(hit.forwardSlip) > 0.5f)) {
                lastSkidIndex = skidmarks.Add(hit.point, hit.normal, 1, lastSkidIndex);
                isSliding = true;
            }
            else {
                lastSkidIndex = -1;
                isSliding = false;
            }
        }

        public bool IsGrounded() {
            return wheelCollider.isGrounded;
        }
    }

    public struct InputData {
        public float steer;
        public float acceleration;
        public bool handbrake;
        public bool nos;
    }

    [SerializeField] Drivetrain drivetrain;
    [SerializeField] VehicleConfig vehicleConfig;
    [SerializeField] Rigidbody rb;
    [SerializeField] bool isAgent;

    [Header("Wheels")]
    [SerializeField] WheelData[] frontWheels;
    [SerializeField] WheelData[] rearWheels;

    [Space]
    [SerializeField] bool showDebug;

    MyControls controls;
    InputData currentInput;
    Vector3 velocity;
    VehicleSaveData vehicleSaveData;
    WheelData[] allWheels;
    WheelData[] drivingWheels;

    bool reverse;
    bool braking;
    bool absTriggered;
    bool tcsTriggered;
    bool isSliding;

    float steerRadius;
    float targetSteer;
    float totalPower;
    float engineRPM;
    float sideSlip;
    float forwardSlip;
    float wheelRPM;
    float enginePowerMultiplier = 1;
    float nosAmount;
    float nosTime;
    float flippedTime;

    int kmph;
    int currentGear;

    public MyControls Controls => controls;
    public Rigidbody VehicleRigidBody => rb;
    public WheelData[] FrontWheels => frontWheels;
    public WheelData[] RearWheels => rearWheels;
    public InputData CurrentInput => currentInput;
    public Vector3 Velocity => velocity;
    public bool ABS => absTriggered;
    public bool TCS => tcsTriggered;
    public bool Braking => braking;
    public bool IsSliding => isSliding;
    public bool NOSActive => nosAmount > 0 && currentInput.nos && currentInput.acceleration > 0;
    public float ShiftUpRPM => vehicleConfig.ShiftUpRPM;
    public float ShiftDownRPM => vehicleConfig.ShiftDownRPM;
    public float EngineRPM => engineRPM;
    public float SideSlip => sideSlip;
    public float ForwardSlip => forwardSlip;
    public float WheelRPM => wheelRPM;
    public float NOSFraction => nosAmount / vehicleConfig.GetUpgradeValue(UpgradeType.Nos, vehicleConfig.NosAmount);
    public int GearsCount => vehicleConfig.Gears.Length;
    public int Kmph => kmph;
    public int CurrentGear => currentGear;

    public Action OnGearChanged;
    public Action OnNOS;

    public void Init(VehicleConfig config, VehicleSaveData saveData, bool isPlayer = false) {
        vehicleConfig = config;
        vehicleSaveData = saveData;
        isAgent = !isPlayer;
        nosAmount = vehicleConfig.GetUpgradeValue(UpgradeType.Nos, vehicleConfig.NosAmount);
        controls = new MyControls();
        controls.Player.AccelerateDecelerate.Enable();
        controls.Player.LeftRight.Enable();
        controls.Player.Handbrake.Enable();
        controls.Player.Nos.Enable();

        allWheels = GetAllWheels();

        foreach (var wheel in allWheels) {
            wheel.Init();

            WheelFrictionCurve sidewaysFriction = wheel.WheelCollider.sidewaysFriction;
            sidewaysFriction.extremumSlip = vehicleConfig.GetUpgradeValue(UpgradeType.Handling, wheel.WheelCollider.sidewaysFriction.extremumSlip);
            sidewaysFriction.asymptoteSlip = vehicleConfig.GetUpgradeValue(UpgradeType.Handling, wheel.WheelCollider.sidewaysFriction.asymptoteSlip);

            WheelFrictionCurve forwardFriction = wheel.WheelCollider.forwardFriction;
            forwardFriction.extremumSlip = vehicleConfig.GetUpgradeValue(UpgradeType.Handling, wheel.WheelCollider.forwardFriction.extremumSlip);
            forwardFriction.asymptoteSlip = vehicleConfig.GetUpgradeValue(UpgradeType.Handling, wheel.WheelCollider.forwardFriction.asymptoteSlip);


            wheel.WheelCollider.sidewaysFriction = sidewaysFriction;
            wheel.WheelCollider.forwardFriction = forwardFriction;
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

    private void Awake() {
        if (isAgent) return;
    }

    private void OnEnable() {
        if (isAgent || controls == null) return;

        controls.Player.AccelerateDecelerate.Enable();
        controls.Player.LeftRight.Enable();
        controls.Player.Handbrake.Enable();
        controls.Player.Nos.Enable();
    }

    private void OnDisable() {
        if (isAgent) return;

        controls.Player.AccelerateDecelerate.Disable();
        controls.Player.LeftRight.Disable();
        controls.Player.Handbrake.Disable();
        controls.Player.Nos.Disable();
    }

    /*void Start()
    {

    }*/

    void Update()
    {
        if (GlobalData.IsGamePaused) return;
        UpdateWheels();
        ApplyDownForce();
        velocity = transform.InverseTransformDirection(rb.velocity);
        kmph = Mathf.FloorToInt(rb.velocity.magnitude * 3.6f);
        sideSlip = GetSideSlip(rearWheels);
        forwardSlip = GetForwardSlip(allWheels);
        float temp = 0;
        float av = Mathf.Abs(velocity.z) / frontWheels[0].WheelCollider.radius;
        wheelRPM = (av / (2 * Mathf.PI)) * 60;

        totalPower = vehicleConfig.EnginePowerCurve.Evaluate(engineRPM) * vehicleConfig.GetUpgradeValue(UpgradeType.Acceleration, vehicleConfig.EnginePower) * vehicleConfig.Gears[currentGear] * enginePowerMultiplier;

        steerRadius = Mathf.Lerp(vehicleConfig.LowSpeedSteerRadius, vehicleConfig.HighSpeedSteerRadius, (velocity.z * 3.6f) / vehicleConfig.GetUpgradeValue(UpgradeType.Engine, vehicleConfig.MaxSpeed));
        steerRadius = Mathf.Clamp(steerRadius, vehicleConfig.LowSpeedSteerRadius, vehicleConfig.HighSpeedSteerRadius);

        if (currentInput.handbrake)
            engineRPM = 1000;
        else
            engineRPM = Mathf.SmoothDamp(engineRPM, 1000 + (Mathf.Abs(wheelRPM) * vehicleConfig.DifferentialRatio * vehicleConfig.Gears[currentGear]), ref temp, 0.01f);

        if (engineRPM > vehicleConfig.ShiftUpRPM && currentGear < vehicleConfig.Gears.Length - 1)
            currentGear++;
        else if (engineRPM < vehicleConfig.ShiftDownRPM && currentGear > 0)
            currentGear--;

        bool slidingWheelFound = false;
        foreach (var wheel in allWheels) {
            wheel.UpdateSkidmarks(VehicleRigidBody.velocity.magnitude);
            if(wheel.IsSliding)
                slidingWheelFound = true;
        }

        isSliding = slidingWheelFound;

        if (isAgent || controls == null) return;

        ReceiveInput(new InputData() {
            steer = controls.Player.LeftRight.ReadValue<float>(),//Input.GetAxis("Horizontal"),
            acceleration = controls.Player.AccelerateDecelerate.ReadValue<float>(),//Input.GetAxis("Vertical"),
            handbrake = controls.Player.Handbrake.IsPressed(),//Input.GetButton("Jump"),
            nos = controls.Player.Nos.IsPressed()
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

        if (rb.velocity.magnitude < 1f)
            reverse = val < 0;

        if (currentInput.handbrake) {
            ApplyHandbrake();
            return;
        }

        ApplyTCS();

        float torque = tcsTriggered ? 0 : totalPower / drivingWheels.Length;
        float brake = ABSBrake(vehicleConfig.BrakeTorque * Mathf.Abs(val));

        if (val > 0 && kmph >= vehicleConfig.GetUpgradeValue(UpgradeType.Engine, vehicleConfig.MaxSpeed))
            val = 0;
        else if (val < 0 && reverse && kmph >= vehicleConfig.MaxReverseSpeed)
            val = 0;

        if (val < 0) {
            foreach (var wData in drivingWheels) {
                if (reverse) {
                    wData.WheelCollider.motorTorque = vehicleConfig.MaxReverseTorque * val;
                    wData.WheelCollider.brakeTorque = 0;
                    rb.AddForce(transform.forward * totalPower * vehicleConfig.GetUpgradeValue(UpgradeType.Acceleration, vehicleConfig.AccelerationForce) * val * 2);
                }
                else {
                    wData.WheelCollider.motorTorque = 0;
                    wData.WheelCollider.brakeTorque = brake;
                    rb.AddForce(transform.forward * vehicleConfig.BrakeTorque * vehicleConfig.GetUpgradeValue(UpgradeType.Acceleration, vehicleConfig.AccelerationForce) * val * 1.25f);
                }
            }
            braking = !reverse;
        }
        else if(val > 0){
            foreach (var wData in drivingWheels) {
                if (reverse) {
                    wData.WheelCollider.motorTorque = 0;
                    wData.WheelCollider.brakeTorque = vehicleConfig.BrakeTorque * Mathf.Abs(val);
                    rb.AddForce(transform.forward * vehicleConfig.BrakeTorque * val);
                }
                else {
                    wData.WheelCollider.motorTorque = TCSAcceleration(wData, torque * val);
                    wData.WheelCollider.brakeTorque = 0;
                    rb.AddForce(transform.forward * totalPower * vehicleConfig.GetUpgradeValue(UpgradeType.Acceleration, vehicleConfig.AccelerationForce) * enginePowerMultiplier * val);
                }
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

    void ApplyNOS(bool val) {
        if (currentInput.acceleration <= 0) return;

        nosAmount = Mathf.Clamp(nosAmount, 0, vehicleConfig.GetUpgradeValue(UpgradeType.Nos, vehicleConfig.NosAmount));

        if (!val || nosAmount <= 0) {
            enginePowerMultiplier = 1;
            if (nosAmount < vehicleConfig.GetUpgradeValue(UpgradeType.Nos, vehicleConfig.NosAmount)) {
                if (nosTime > 3) nosAmount += Time.deltaTime;
                else nosTime += Time.deltaTime;
            }
            return;
        }
        nosTime = 0;
        enginePowerMultiplier = 1 + vehicleConfig.GetUpgradeValue(UpgradeType.Nos, vehicleConfig.NosPowerMultiplier);
        nosAmount -= Time.deltaTime;
    }

    void ApplyHandbrake() {

        foreach (var wheel in drivingWheels) {
            wheel.WheelCollider.brakeTorque = vehicleConfig.HandbrakeTorque;
            wheel.WheelCollider.motorTorque = 0;
            rb.AddForce(-transform.forward * vehicleConfig.HandbrakeTorque * Mathf.Clamp(-1, 1, velocity.z) / 3);
        }
    }

    void UpdateWheels() {
        for (int f = 0; f < frontWheels.Length; f++)
            frontWheels[f].UpdateVisual();
        for (int r = 0; r < rearWheels.Length; r++)
            rearWheels[r].UpdateVisual();
    }

    void ApplyDownForce() {
        rb.AddForce(-transform.up * vehicleConfig.DownForce * rb.velocity.magnitude, ForceMode.Force);
    }

    float ABSBrake(float brakeForce) {
        if (!vehicleConfig.UseABS) return brakeForce;

        float sum = 0;
        for (int i = 0; i < allWheels.Length; i++)
            sum += Mathf.Abs(allWheels[i].GetForwardSlip()) + Mathf.Abs(allWheels[i].GetSideSlip());

        float avg = sum / allWheels.Length;

        if (avg >= vehicleConfig.AbsThreshold) {
            absTriggered = true;
            return brakeForce * vehicleConfig.AbsBrakeFactor;
        }
        else
            absTriggered = false;
        return brakeForce;
    }

    float TCSAcceleration(WheelData wheel, float acc) {
        return acc;
        if (!vehicleConfig.UseTCS) return acc;

        if(Mathf.Abs(wheel.GetForwardSlip()) >= vehicleConfig.TcsThreshold) {
            tcsTriggered = true;
            return acc * vehicleConfig.TcsFactor;
        }else
            tcsTriggered = false;

        return acc;
    }

    void ApplyTCS() {
        if (!vehicleConfig.UseTCS) return;

        foreach (var wheel in allWheels) {
            if (Mathf.Abs(wheel.GetForwardSlip()) >= vehicleConfig.TcsThreshold)
                tcsTriggered = true;
            else
                tcsTriggered = false;
        }

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

    public void ResetVehicle() {
        if (!allWheels.IsNullOrEmpty()) {
            foreach (var item in allWheels) {
                item.WheelCollider.motorTorque = 0;
                item.WheelCollider.brakeTorque = 0;
            }
        }

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        nosAmount = vehicleConfig.GetUpgradeValue(UpgradeType.Nos, vehicleConfig.NosAmount);
    }

    public void ReceiveInput(InputData input) {
        if (RaceManager.Instance.CurrentState != RaceManager.State.Playing) return;
        currentInput = input;
        targetSteer = Mathf.Lerp(targetSteer, input.steer, Time.deltaTime * 25);
        Steer(targetSteer);
        Accelerate(input.acceleration);
        ApplyNOS(input.nos);
    }

    public bool GroudedWheels() {
        foreach (var wf in frontWheels) {
            if (wf.IsGrounded())
                return true;
        }

        foreach (var wr in rearWheels) {
            if (wr.IsGrounded())
                return true;
        }

        return false;
    }

    private void OnDrawGizmos() {

        if (rb != null) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(rb.centerOfMass, 0.1f);
        }

        if (vehicleConfig == null || !showDebug) return;

        Gizmos.color = Color.red;
        var rot = Quaternion.Euler(0, (reverse ? -1 : 1) * currentInput.steer * 45, 0);
        Gizmos.DrawRay(transform.position, rot * transform.forward * totalPower * vehicleConfig.AccelerationForce * currentInput.acceleration);
    }

}
