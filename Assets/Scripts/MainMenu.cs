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
    [SerializeField] PreviewManager previewManager;

    private void Start() {
        PanelManager.Instance.OnExitPopupShow += OnShowExitPopup;
    }

    private void OnDestroy() {
        PanelManager.Instance.OnExitPopupShow -= OnShowExitPopup;
    }

    public async void OnPlay(RaceData data) {
        /*Debug.Log("[MainMenu] Start loading main scene");

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(coreGamePlayScene, LoadSceneMode.Single);
        while (!asyncOperation.isDone)
            await Task.Yield();

        Debug.Log("[MainMenu] Start loading track scene");
        AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(data.sceneReference, LoadSceneMode.Additive);

        await handle.Task;

        Debug.Log("[MainMenu] Initialize RaceManager");
        RaceManager.Instance.Init();*/

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

    void OnShowExitPopup() {
        PopupPanel.Instance.Show("", "Are you sure you want to quit?", () => Application.Quit(), true, () => PanelManager.Instance.HidePanel());
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            UserManager.playerData.SetInt(PlayerPrefsStrings.CASH, UserManager.playerData.GetInt(PlayerPrefsStrings.CASH) + 10);
        }
    }
}
