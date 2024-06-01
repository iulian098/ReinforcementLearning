using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITrackRewardInfo : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] TMP_Text value;

    public void Init(string labelText, string valueText) {
        label.text = labelText;
        value.text = valueText;
    }
}
