using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlappyPlayer : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] float force;
    [SerializeField] float maxYVelocity;
    [SerializeField] float rightSpeed;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Fly();
        rb.velocity = new Vector3(rightSpeed, rb.velocity.y, 0);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxYVelocity);
        
    }

    public void Fly() {
        rb.AddForce(Vector3.up * force, ForceMode.Acceleration);
    }
}
