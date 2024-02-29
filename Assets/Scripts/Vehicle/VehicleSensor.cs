using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

[ExecuteInEditMode]
public class VehicleSensor : MonoBehaviour
{
    [SerializeField] int raysPerDirection = 3;
    [SerializeField] float raysAngle = 70;
    [SerializeField] float sensorsLength;
    [SerializeField] float sensorsCastRadius;
    [SerializeField] LayerMask sensorMask;
    float[] hitFractions;
    float[] angles;

    public float[] HitFractions => hitFractions;

    private void Awake() {
        UpdateRaysCount();
    }

    private void Update() {
        if (angles.Length != raysPerDirection * 2 + 1)
            UpdateRaysCount();

        for (var rayIndex = 0; rayIndex < angles.Length; rayIndex++) {
            hitFractions[rayIndex] = PerceiveSingleRay(rayIndex).HitFraction;
        }
    }

    void UpdateRaysCount() {
        angles = GetRayAngles(raysPerDirection, raysAngle);
        hitFractions = new float[angles.Length];
    }

    public (Vector3 StartPositionWorld, Vector3 EndPositionWorld) RayExtents(int rayIndex) {
        var angle = angles[rayIndex];
        Vector3 startPositionLocal, endPositionLocal;

        startPositionLocal = new Vector3(0, 0, 0);
        endPositionLocal = PolarToCartesian3D(sensorsLength, angle);

        var startPositionWorld = transform.TransformPoint(startPositionLocal);
        var endPositionWorld = transform.TransformPoint(endPositionLocal);

        return (StartPositionWorld: startPositionWorld, EndPositionWorld: endPositionWorld);
    }

    Vector3 PolarToCartesian3D(float radius, float angleDegrees) {
        var x = radius * Mathf.Cos(Mathf.Deg2Rad * angleDegrees);
        var z = radius * Mathf.Sin(Mathf.Deg2Rad * angleDegrees);
        return new Vector3(x, 0f, z);
    }

    float[] GetRayAngles(int raysPerDirection, float maxRayDegrees) {
        // Example:
        // { 90, 90 - delta, 90 + delta, 90 - 2*delta, 90 + 2*delta }
        var anglesOut = new float[2 * raysPerDirection + 1];
        var delta = maxRayDegrees / raysPerDirection;
        anglesOut[0] = 90f;
        for (var i = 0; i < raysPerDirection; i++) {
            anglesOut[2 * i + 1] = 90 - (i + 1) * delta;
            anglesOut[2 * i + 2] = 90 + (i + 1) * delta;
        }
        return anglesOut;
    }

    RayPerceptionOutput.RayOutput PerceiveSingleRay(int rayIndex) {

        var unscaledRayLength = sensorsLength; //input.RayLength;
        var unscaledCastRadius = sensorsCastRadius;

        var extents = RayExtents(rayIndex);
        var startPositionWorld = extents.StartPositionWorld;
        var endPositionWorld = extents.EndPositionWorld;

        var rayDirection = endPositionWorld - startPositionWorld;
        var scaledRayLength = rayDirection.magnitude;
        // Avoid 0/0 if unscaledRayLength is 0
        var scaledCastRadius = unscaledRayLength > 0 ?
            unscaledCastRadius * scaledRayLength / unscaledRayLength :
            unscaledCastRadius;

        // Do the cast and assign the hit information for each detectable tag.
        var castHit = false;
        var hitFraction = 1.0f;
        GameObject hitObject = null;

        RaycastHit rayHit;
        if (scaledCastRadius > 0f) {
            castHit = Physics.SphereCast(startPositionWorld, scaledCastRadius, rayDirection, out rayHit,
                scaledRayLength, sensorMask);
        }
        else {
            castHit = Physics.Raycast(startPositionWorld, rayDirection, out rayHit,
                scaledRayLength, sensorMask);
        }

        // If scaledRayLength is 0, we still could have a hit with sphere casts (maybe?).
        // To avoid 0/0, set the fraction to 0.
        hitFraction = castHit ? (scaledRayLength > 0 ? rayHit.distance / scaledRayLength : 0.0f) : 1.0f;
        hitObject = castHit ? rayHit.collider.gameObject : null;

        var rayOutput = new RayPerceptionOutput.RayOutput {
            HasHit = castHit,
            HitFraction = hitFraction,
            HitTaggedObject = false,
            HitTagIndex = -1,
            HitGameObject = hitObject,
            StartPositionWorld = startPositionWorld,
            EndPositionWorld = endPositionWorld,
            ScaledCastRadius = scaledCastRadius
        };


        return rayOutput;
    }

    private void OnDrawGizmosSelected() {
        if (angles == null || angles.Length == 0 || angles.Length != raysPerDirection * 2 + 1)
            angles = GetRayAngles(raysPerDirection, raysAngle);

        for (var rayIndex = 0; rayIndex < angles.Length; rayIndex++) {
            var rayOutput = PerceiveSingleRay(rayIndex);
            DrawRaycastGizmos(rayOutput);
        }
    }

    void DrawRaycastGizmos(RayPerceptionOutput.RayOutput rayOutput, float alpha = 1.0f) {
        var startPositionWorld = rayOutput.StartPositionWorld;
        var endPositionWorld = rayOutput.EndPositionWorld;
        var rayDirection = endPositionWorld - startPositionWorld;
        rayDirection *= rayOutput.HitFraction;

        // hit fraction ^2 will shift "far" hits closer to the hit color
        var lerpT = rayOutput.HitFraction * rayOutput.HitFraction;
        var color = Color.Lerp(Color.red, Color.white, lerpT);
        color.a *= alpha;
        Gizmos.color = color;
        Gizmos.DrawRay(startPositionWorld, rayDirection);

        // Draw the hit point as a sphere. If using rays to cast (0 radius), use a small sphere.
        if (rayOutput.HasHit) {
            var hitRadius = Mathf.Max(rayOutput.ScaledCastRadius, .05f);
            Gizmos.DrawWireSphere(startPositionWorld + rayDirection, hitRadius);
        }
    }

}
