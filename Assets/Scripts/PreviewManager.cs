using UnityEngine;

public class PreviewManager : MonoBehaviour
{
    [SerializeField] Transform previewContainer;
    [SerializeField] VehiclesContainer vehiclesContainer;

    GameObject currentPreview;

    private void Start() {
        ShowCurrentVehicle();
    }

    public void ShowCurrentVehicle() {
        ShowPreview(vehiclesContainer.GetEquippedVehicle());
    }

    public void ShowPreview(VehicleConfig config) {
        if(currentPreview != null)
            Destroy(currentPreview);

        currentPreview = Instantiate(config.PreviewPrefab, previewContainer);
        currentPreview.transform.localScale = Vector3.one;
    }

}
