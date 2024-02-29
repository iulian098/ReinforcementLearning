using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VehicleEffects : MonoBehaviour
{
    [System.Serializable]
    public class WheelEffect {
        public WheelCollider wheelColl;
        //public ParticleSystem smoke;
        public VisualEffect smokeVFX;
    }
    [SerializeField] Vehicle vehicle;
    [SerializeField] ParticleSystem[] exhaustParticles;
    [SerializeField] WheelEffect[] wheelCollider;

    private void Start() {
        
    }

    void FixedUpdate()
    {
        for (int i = 0; i < exhaustParticles.Length; i++) {
            UpdateExhaust(exhaustParticles[i]);
        }
        for (int i = 0;i < wheelCollider.Length; i++) {
            UpdateWheelSmole(wheelCollider[i]);
        }
    }

    void UpdateWheelSmole(WheelEffect wheelEffect) {
        wheelEffect.wheelColl.GetGroundHit(out WheelHit hit);
        if (Mathf.Abs(hit.sidewaysSlip) > 0.5f) {
            wheelEffect.smokeVFX.Play();
            wheelEffect.smokeVFX.SetInt("SpawnRate", 40 + (int)(Mathf.Abs(hit.sidewaysSlip) * 25));
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
}
