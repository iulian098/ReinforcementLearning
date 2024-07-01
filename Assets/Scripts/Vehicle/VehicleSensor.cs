using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

[ExecuteInEditMode]
public class VehicleSensor : MonoBehaviour {
    [SerializeField] int raysPerDirection = 3;
    [SerializeField] float raysAngle = 70;
    [SerializeField] float sensorsLength;
    [SerializeField] float sensorsCastRadius;
    [SerializeField] string[] tags;
    [SerializeField] LayerMask sensorMask;
    [SerializeField] bool showGizmos;
    float[] hitFractions;
    float[] angles;
    float[] tagHit;

    public float[] Angles => angles;
    public float[] HitFractions {
        get {
            if (hitFractions.IsNullOrEmpty()) {
                hitFractions = new float[raysPerDirection * 2 + 1];
            }
            return hitFractions;
        }
    }
    public float[] TagHit => tagHit;

    private void Awake() {
        UpdateRaysCount();
    }

    private void Update() {
        if (angles.Length != raysPerDirection * 2 + 1 || hitFractions.Length != angles.Length || tagHit.Length != angles.Length)
            UpdateRaysCount();

        for (var rayIndex = 0; rayIndex < angles.Length; rayIndex++) {
            HitOutput output = PerceiveSingleRay(rayIndex);
            hitFractions[rayIndex] = output.HitFraction;
            tagHit[rayIndex] = output.HitTag;
        }
    }

    void UpdateRaysCount() {
        Debug.Log("[Sensor] Updated Rays Count");
        angles = GetRayAngles(raysPerDirection, raysAngle);
        hitFractions = new float[angles.Length];
        tagHit = new float[angles.Length];
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

    public (Vector3 StartPositionWorld, Vector3 EndPositionWorld) RayExtents(float angle, float length) {
        Vector3 startPositionLocal, endPositionLocal;

        startPositionLocal = new Vector3(0, 0, 0);
        endPositionLocal = PolarToCartesian3D(length, angle);

        var startPositionWorld = transform.TransformPoint(startPositionLocal);
        var endPositionWorld = transform.TransformPoint(endPositionLocal);

        return (StartPositionWorld: startPositionWorld, EndPositionWorld: endPositionWorld);
    }

    Vector3 PolarToCartesian3D(float radius, float angleDegrees) {
        var x = radius * Mathf.Cos(Mathf.Deg2Rad * angleDegrees);
        var z = radius * Mathf.Sin(Mathf.Deg2Rad * angleDegrees);
        return new Vector3(x, 0f, z);
    }

    public float[] GetRayAngles(int raysPerDirection, float maxRayDegrees) {
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

    HitOutput PerceiveSingleRay(int rayIndex) {

        var unscaledRayLength = sensorsLength;
        var unscaledCastRadius = sensorsCastRadius;

        var extents = RayExtents(rayIndex);
        var startPositionWorld = extents.StartPositionWorld;
        var endPositionWorld = extents.EndPositionWorld;

        var rayDirection = endPositionWorld - startPositionWorld;
        var scaledRayLength = rayDirection.magnitude;

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
        hitFraction = castHit ? (scaledRayLength > 0 ? rayHit.distance / scaledRayLength : 0.0f) : 1.0f;
        hitObject = castHit ? rayHit.collider.gameObject : null;

        var output = new HitOutput {
            StartPosition = startPositionWorld,
            EndPosition = endPositionWorld,
            HitFraction = hitFraction,
            HitTag = 0f,
            HasHit = castHit,
            ScaledCastRadius = scaledCastRadius
        };

        if (castHit) {
            for (int i = 0; i < tags.Length; i++) {
                bool tagFound = false;
                if (!string.IsNullOrEmpty(tags[i]))
                    tagFound = hitObject.CompareTag(tags[i]);

                output.HitTag = tagFound ? 1 : 0;
            }
        }

        return output;
    }

    private void OnDrawGizmosSelected() {
        if (!showGizmos) return;
        if (angles == null || angles.Length == 0 || angles.Length != raysPerDirection * 2 + 1)
            angles = GetRayAngles(raysPerDirection, raysAngle);

        for (var rayIndex = 0; rayIndex < angles.Length; rayIndex++) {
            var rayOutput = PerceiveSingleRay(rayIndex);
            DrawRaycastGizmos(rayOutput);
        }
    }

    void DrawRaycastGizmos(HitOutput hitOutput, float alpha = 1.0f) {
        var startPositionWorld = hitOutput.StartPosition;
        var endPositionWorld = hitOutput.EndPosition;
        var rayDirection = endPositionWorld - startPositionWorld;
        rayDirection *= hitOutput.HitFraction;

        // hit fraction ^2 will shift "far" hits closer to the hit color
        var lerpT = hitOutput.HitFraction * hitOutput.HitFraction;
        var color = Color.Lerp(Color.red, Color.white, lerpT);
        color.a *= alpha;
        Gizmos.color = color;
        Gizmos.DrawRay(startPositionWorld, rayDirection);

        // Draw the hit point as a sphere. If using rays to cast (0 radius), use a small sphere.
        if (hitOutput.HasHit) {
            var hitRadius = Mathf.Max(hitOutput.ScaledCastRadius, .05f);
            Gizmos.DrawWireSphere(startPositionWorld + rayDirection, hitRadius);
        }
    }

    struct HitOutput {
        public Vector3 StartPosition;
        public Vector3 EndPosition;
        public float HitFraction;
        public float HitTag;
        public bool HasHit;
        public float ScaledCastRadius;
    }
}
