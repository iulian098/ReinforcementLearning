using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] Vehicle vehicle;
    [SerializeField] TMP_Text kmText;
    [SerializeField] TMP_Text gearText;
    [SerializeField] TMP_Text absText;
    [SerializeField] TMP_Text tcsText;
    [SerializeField] TMP_Text rpmText;

    void Update()
    {
        if (vehicle == null) return;

        kmText.text = "Km/h: " + vehicle.Kmph;
        gearText.text = "Gear: " + vehicle.CurrentGear;
        absText.text = "ABS: " + vehicle.ABS;
        tcsText.text = "TCS: " + vehicle.TCS;
        rpmText.text = "RPM: " + vehicle.EngineRPM;
    }
}
