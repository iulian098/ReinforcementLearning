using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public void SetTimeScale(float val) {
        Time.timeScale = val;
    }
}
