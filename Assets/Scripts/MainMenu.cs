using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] AssetReference coreGamePlayScene;
    [SerializeField] VehicleSelection vehicleSelection;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject trackSelection;
    [SerializeField] SettingsPanel settingsPanel;
    [SerializeField] PreviewManager previewManager;

    private void Start() {
        PanelManager.Instance.OnExitPopupShow += OnShowExitPopup;
    }

    private void OnDestroy() {
        PanelManager.Instance.OnExitPopupShow -= OnShowExitPopup;
    }

    public async void OnPlay(RaceData data) {
        object[] scenesToLoad = new object[] {
            data.sceneReference,
            coreGamePlayScene
        };

        Racing.SceneManager.Instance.OnSceneLoaded += InitRaceManager;
        Racing.SceneManager.Instance.LoadScenes(scenesToLoad);
    }

    void InitRaceManager() {
        RaceManager.Instance.Init();
    }

    public void OnVehiclesClicked() {
        PanelManager.Instance.ShowPanel("VehicleSelection", () => {
            vehicleSelection.Show();
            mainMenu.SetActive(false);
        },
        () => {
            vehicleSelection.Hide();
            mainMenu.SetActive(true);
            previewManager.ShowCurrentVehicle();
        });
    }

    public void OnPlayClicked() {
        PanelManager.Instance.ShowPanel("TrackSelection", () => {
            trackSelection.SetActive(true);
            mainMenu.SetActive(false);
        },
        () => {
            trackSelection.SetActive(false);
            mainMenu.SetActive(true);
        });

    }

    public void OnOptionsClicked() {
        PanelManager.Instance.ShowPanel("Options", () => {
            settingsPanel.Open();
            mainMenu.SetActive(false);
        },
        () => {
            settingsPanel.Close();
            mainMenu.SetActive(true);
        });
    }

    void OnShowExitPopup() {
        PopupPanel.Instance.Show("", "Are you sure you want to quit?", () => Application.Quit(), true, () => PanelManager.Instance.HidePanel());
    }
}
