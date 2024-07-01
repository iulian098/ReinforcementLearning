using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Test2 : MonoBehaviour
{
    [SerializeField] Transform point1, point2, point3;

    public float point23Direction;

    public Vector3 direction12Vector; 
    public Vector3 direction23Vector; 
    void Update()
    {
        if(point1 ==null || point2 == null || point3 == null) 
            return;
        direction23Vector = point3.position - point2.position;
        direction12Vector = point2.position - point1.position;
        float point23Temp = (Vector3.SignedAngle(direction12Vector, direction23Vector, Vector3.up) + 180) / 360;
        point23Direction = Mathf.Round(point23Temp * 8f) / 8f;
    }
}
