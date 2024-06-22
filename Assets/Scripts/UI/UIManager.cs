using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] Vehicle vehicle;
    [SerializeField] TMP_Text kmText;
    [SerializeField] TMP_Text gearText;
    [SerializeField] TMP_Text rpmText;
    [SerializeField] TMP_Text startingTimeText;
    [SerializeField] TMP_Text nosText;
    [SerializeField] ResultScreen resultScreen;
    [SerializeField] GameObject controlsObjects;

    public void Init() {
        resultScreen.Init(RaceManager.Instance.Vehicles);

#if UNITY_ANDROID
        controlsObjects.SetActive(true);
#else
        controlsObjects.SetActive(false);
#endif
    }

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
        rpmText.text = "RPM: " + (int)vehicle.EngineRPM;
        nosText.text = "NOS: " + vehicle.NOSFraction;
    }

    public void SetVehicle(VehicleManager vehicle) {
        this.vehicle = vehicle.Vehicle;
    }

    public void RaceFinished(VehicleManager vehicle, bool isPlayer) {
        resultScreen.SetResult(vehicle.currentPlacement, 
            new ResultData() {
                placement = vehicle.currentPlacement,
                time = RaceManager.Instance.RaceTimeSeconds,
                vehicleManager = vehicle
            }
        );

        if (isPlayer)
            resultScreen.Show(vehicle.currentPlacement);
    }

    public void GoToMainMenu() {
        SceneManager.LoadScene("MainMenu");
    }
}
