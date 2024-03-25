using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VehiclesContainer", menuName = "ScriptableObjects/Vehicles Container")]
public class VehiclesContainer : ScriptableObject
{
    [SerializeField] VehicleConfig[] vehicles;

    public VehicleConfig[] Vehicles => vehicles;
}
