using System;
using UnityEngine;

public class VehicleTrigger : MonoBehaviour
{
    [SerializeField] string targetTag;
    bool triggered;

    public Action OnTriggered;
    public bool Triggered => triggered;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag(targetTag)) {
            triggered = true;
            OnTriggered?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag(targetTag)) {
            triggered = false;
            OnTriggered?.Invoke();
        }
    }
}
