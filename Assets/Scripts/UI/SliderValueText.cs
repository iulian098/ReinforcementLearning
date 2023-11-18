using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueText : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text valueText;

    private void OnValidate() {
        if (slider != null) return;

        if (TryGetComponent(out Slider s))
            slider = s;
    }

    private void Start() {
        slider.onValueChanged.AddListener(SetValue);
        SetValue(slider.value);
    }

    public void SetValue(float val) {
        valueText.text = val.ToString("0.00");
    }
}
