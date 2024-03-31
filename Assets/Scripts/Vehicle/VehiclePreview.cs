using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehiclePreview : MonoBehaviour
{
    [SerializeField] WheelCollider[] wheelsColl;
    [SerializeField] Transform[] wheelsMesh;

    private void FixedUpdate() {
        for (int i = 0; i < wheelsColl.Length; i++) {
            wheelsColl[i].GetWorldPose(out var pos, out var rot);
            wheelsMesh[i].SetPositionAndRotation(pos, rot);
        }
    }


}
