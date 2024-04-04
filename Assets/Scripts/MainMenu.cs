using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string coreGamePlayScene;
    [SerializeField] VehicleSelection vehicleSelection;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject trackSelection;
    [SerializeField] PreviewManager previewManager;

    public async void OnPlay(RaceData data) {
        Debug.Log("[MainMenu] Start loading main scene");
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(coreGamePlayScene, LoadSceneMode.Single);
        while (!asyncOperation.isDone)
            await Task.Yield();

        Debug.Log("[MainMenu] Start loading track scene");
        AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(data.sceneReference, LoadSceneMode.Additive);

        await handle.Task;

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
}
