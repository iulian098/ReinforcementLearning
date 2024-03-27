using UnityEngine;
using UnityEngine.UI;

public class StatsItem : MonoBehaviour
{
    [SerializeField] Slider currentValueSlider;
    [SerializeField] Slider newValueSlider;
    [SerializeField] Image currentImg;
    [SerializeField] Image newImg;
    [SerializeField] Color increasedValueColor = Color.green;
    [SerializeField] Color decreasedValueColor = Color.red;

    public void SetValue(float currentValue, float newValue) {
        if(newValue > currentValue) {
            currentValueSlider.value = currentValue;
            newValueSlider.value = newValue;

            currentImg.color = Color.white;
            newImg.color = increasedValueColor;

        }
        else {
            currentValueSlider.value = newValue;
            newValueSlider.value = currentValue;

            currentImg.color = Color.white;
            newImg.color = decreasedValueColor;
        }
    }
}
