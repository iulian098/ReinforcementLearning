using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Skidmarks))]
public class SkidmarksManager : MonoBehaviour
{
    [SerializeField] Skidmarks skidmarks;

    public Skidmarks Skidmarks => skidmarks;

    private void Start() {
        if(skidmarks == null)
            skidmarks = GetComponent<Skidmarks>();
    }
}
