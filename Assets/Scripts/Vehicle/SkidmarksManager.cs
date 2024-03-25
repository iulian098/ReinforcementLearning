using UnityEngine;

[RequireComponent(typeof(Skidmarks))]
public class SkidmarksManager : MonoSingleton<SkidmarksManager>
{
    [SerializeField] Skidmarks skidmarks;

    public Skidmarks Skidmarks => skidmarks;

    private void Start() {
        if(skidmarks == null)
            skidmarks = GetComponent<Skidmarks>();
    }
}
