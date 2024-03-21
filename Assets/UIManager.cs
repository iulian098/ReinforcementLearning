using TMPro;
using UnityEditor.Build.Content;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] Vehicle vehicle;
    [SerializeField] TMP_Text kmText;
    [SerializeField] TMP_Text gearText;
    [SerializeField] TMP_Text rpmText;
    [SerializeField] TMP_Text startingTimeText;

    void FixedUpdate()
    {
        if (vehicle == null) return;

        if (RaceManager.Instance.CurrentStartingTime > 0 && RaceManager.Instance.CurrentState == RaceManager.State.Starting) {
            startingTimeText.gameObject.SetActive(true);
            startingTimeText.text = Mathf.CeilToInt(RaceManager.Instance.CurrentStartingTime).ToString();
        }
        else
            startingTimeText.gameObject.SetActive(false);

        kmText.text = "Km/h: " + vehicle.Kmph;
        gearText.text = "Gear: " + vehicle.CurrentGear;
        rpmText.text = "RPM: " + vehicle.EngineRPM;
    }
}
