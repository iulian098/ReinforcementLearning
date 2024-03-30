using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string playScene;
    [SerializeField] VehicleSelection vehicleSelection;
    [SerializeField] GameObject mainMenu;

    public void OnPlay() {
        SceneManager.LoadScene(playScene);
    }

    public void OnVehiclesClicked() {
        PanelManager.Instance.ShowPanel("VehicleSelection", () => {
            vehicleSelection.Show();
            mainMenu.SetActive(false);
        },
        () => {
            vehicleSelection.Hide();
            mainMenu.SetActive(true);
        });
    }
}
