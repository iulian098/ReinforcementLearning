using Unity.MLAgents.Policies;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    [System.Serializable]
    public class WheelData {
        [SerializeField] WheelCollider wheelCollider;
        [SerializeField] Transform wheelVisual;

        public WheelCollider WheelCollider => wheelCollider;

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
    }
    [SerializeField] float maxReverseTorque;
    [SerializeField] float maxBrakeTorque;
    [SerializeField] float steerRadius;
    [SerializeField] float downForce;
    [SerializeField] float[] gears;
    [SerializeField] float maxRPM;
    [SerializeField] float minRPM;
    [SerializeField] float differentialRatio = 3.6f;

    [SerializeField] AnimationCurve enginePowerCurve;

    [SerializeField] Rigidbody rb;
    [SerializeField] WheelData[] frontWheels;
    [SerializeField] WheelData[] rearWheels;

    WheelData[] allWheels;
    Vector3 velocity;
    bool reverse;
    bool isAgent;
    float totalPower;
    float engineRPM;
    float sideSlip;
    float forwardSlip;
    float wheelRPM;
    int kmph;
    int currentGear;

    public Rigidbody VehicleRigidBody => rb;
    public Vector3 Velocity => velocity;
    public float SideSlip => sideSlip;
    public int Kmph => kmph;

    void Start()
    {
        if (TryGetComponent(out BehaviorParameters _))
            isAgent = true;

        allWheels = GetAllWheels();
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
        wheelRPM = GetWheelsRPM();
        totalPower = enginePowerCurve.Evaluate(engineRPM) * gears[currentGear];
        engineRPM = Mathf.SmoothDamp(engineRPM, 1000 + (Mathf.Abs(wheelRPM) * differentialRatio * gears[currentGear]), ref temp, 0.1f);

        if (engineRPM > maxRPM && currentGear < gears.Length - 1)
            currentGear++;
        else if (engineRPM < minRPM && currentGear > 0)
            currentGear--;

        if (isAgent) return;
        ReceiveInput(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    private void FixedUpdate() {

        if (isAgent) return;
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

        //Ackerman steering
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

        float torque = totalPower / 4;

        if (val < 0) {
            foreach (var wColl in allWheels) {
                wColl.WheelCollider.motorTorque = reverse ? maxReverseTorque * val : 0;
                wColl.WheelCollider.brakeTorque = reverse ? 0 : maxBrakeTorque * Mathf.Abs(val);
            }
        }
        else if(val > 0){
            foreach (var wColl in allWheels) {
                wColl.WheelCollider.motorTorque = reverse ? 0 : torque * val;
                wColl.WheelCollider.brakeTorque = reverse ? maxBrakeTorque * Mathf.Abs(val) : 0;
            }
        }
        else {
            foreach (var wColl in allWheels) {
                wColl.WheelCollider.motorTorque = 0;
                wColl.WheelCollider.brakeTorque = 0;
            }
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

    float GetForwardSlip(WheelData[] wheels) {
        float sum = 0;
        for (int i = 0; i < wheels.Length; i++) {
            sum += wheels[i].GetForwardSlip();
        }

        return sum / wheels.Length;
    }

    float GetSideSlip(WheelData[] wheels) {
        float totalSideSlip = 0;
        foreach (var wheel in wheels) {
            totalSideSlip += wheel.GetSideSlip();
        }

        return Mathf.Clamp(totalSideSlip / wheels.Length, -1f, 1f);
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
    }

    public void ReceiveInput(float steering, float acc) {
        Steer(steering);
        Accelerate(acc);
    }
}
