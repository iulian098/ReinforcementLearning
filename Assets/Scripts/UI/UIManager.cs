using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] Vehicle vehicle;
    [SerializeField] TMP_Text kmText;
    [SerializeField] TMP_Text gearText;
    [SerializeField] TMP_Text rpmText;
    [SerializeField] TMP_Text startingTimeText;
    [SerializeField] ResultScreen resultScreen;

    public void Init() {
        resultScreen.Init(RaceManager.Instance.Vehicles);
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
            resultScreen.Show();
    }

    public void GoToMainMenu() {
        SceneManager.LoadScene("MainMenu");
    }
}
