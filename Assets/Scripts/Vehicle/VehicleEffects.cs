using System;
using UnityEngine;
using UnityEngine.VFX;

public class VehicleEffects : MonoBehaviour
{
    const string BRAKING_EMISSION = "_EmissionPower";

    [System.Serializable]
    public class WheelEffect {
        public WheelCollider wheelColl;
        public VisualEffect smokeVFX;
    }

    [SerializeField] Vehicle vehicle;
    [SerializeField] MeshRenderer chassisMesh;
    [SerializeField] ParticleSystem[] exhaustParticles;
    [SerializeField] WheelEffect[] wheelCollider;
    [SerializeField] VisualEffect sparksEffect;
    Material brakingMaterial;

    float sideSlip;

    private void Start() {
        brakingMaterial = Array.Find(chassisMesh.materials, x => x.name.Contains("Braking"));
    }

    void FixedUpdate()
    {
        for (int i = 0; i < exhaustParticles.Length; i++)
            UpdateExhaust(exhaustParticles[i]);

        for (int i = 0;i < wheelCollider.Length; i++)
            UpdateWheelSmoke(wheelCollider[i]);

        if(brakingMaterial != null)
            brakingMaterial.SetFloat(BRAKING_EMISSION, Mathf.Lerp(brakingMaterial.GetFloat(BRAKING_EMISSION), vehicle.Braking ? 1 : 0, Time.deltaTime * 25f));
    }

    void UpdateWheelSmoke(WheelEffect wheelEffect) {
        wheelEffect.wheelColl.GetGroundHit(out WheelHit hit);
        if(wheelEffect == wheelCollider[0])
            sideSlip = hit.sidewaysSlip;
        if(vehicle.VehicleRigidBody.velocity.magnitude > 10 && Mathf.Abs(hit.sidewaysSlip) > 0.5f) {
            wheelEffect.smokeVFX.Play();
            wheelEffect.smokeVFX.SetInt("SpawnRate", 40 + (int)(Mathf.Abs(hit.sidewaysSlip) * 30));
        }
        else{
            wheelEffect.smokeVFX.Stop();
        }
    }

    void UpdateExhaust(ParticleSystem exhaust) {
        if (vehicle.Kmph > 10 && exhaust.isEmitting)
            exhaust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        else if (!exhaust.isEmitting)
            exhaust.Play(true);

        ParticleSystem.EmissionModule emissionModule = exhaust.emission;
        emissionModule.rateOverTime = 20 + 2 * vehicle.Velocity.z;
        ParticleSystem.MainModule mainModule = exhaust.main;
        float lifetime = 0.5f - Mathf.Abs(vehicle.Velocity.z) / 10;
        lifetime = Mathf.Clamp(lifetime, 0, 0.5f);
        mainModule.startLifetime = lifetime;
    }

    void UpdateSparks(Vector3 point, Vector3 normal) {
        sparksEffect.transform.position = point;

        float dotProduct = Vector3.Dot(normal, vehicle.VehicleRigidBody.velocity.normalized);
        float crossProduct = Vector3.Cross(normal, vehicle.VehicleRigidBody.velocity.normalized).y;
        float angleDegrees = (float)(Mathf.Atan2(crossProduct, dotProduct) * (180f / Mathf.PI)) + 90;
        float angle = (angleDegrees + 360) % 360;

        sparksEffect.transform.localRotation = Quaternion.Euler(0, angle, 0);
    }

    private void OnCollisionEnter(Collision collision) {
        if (Mathf.Abs(vehicle.Velocity.z) < 10)
            return;
        UpdateSparks(collision.GetContact(0).point, collision.GetContact(0).normal);
        sparksEffect.SetInt("SpawnRate", 50 + (int)(vehicle.Velocity.z * 2));
        sparksEffect.SendEvent("SpawnBurst");
    }

    private void OnCollisionStay(Collision collision) {
        if ((collision.rigidbody == null && Mathf.Abs(vehicle.Velocity.z) < 2) ||
            (collision.rigidbody != null && Mathf.Abs(collision.rigidbody.velocity.magnitude - vehicle.VehicleRigidBody.velocity.magnitude) < 2)) {
            if (sparksEffect.HasAnySystemAwake())
                sparksEffect.SendEvent("StopSpawn");
            return;
        }
    
        UpdateSparks(collision.GetContact(0).point, collision.GetContact(0).normal);
        sparksEffect.SetInt("SpawnRate", 50 + (int)(vehicle.Velocity.z * 5));
        sparksEffect.SendEvent("StartSpawn");
    }

    private void OnCollisionExit(Collision collision) {
        sparksEffect.SendEvent("StopSpawn");
    }
}
