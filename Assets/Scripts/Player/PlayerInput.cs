using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerInput {
    public float Vertical;
    public float Horizontal;

    public void SendInput(float v = 0, float h = 0) {
        Vertical = v;
        Horizontal = h;
    }
}
