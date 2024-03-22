using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RacersNames", menuName = "ScriptableObjects/Racers Name")]
public class RacersNames : ScriptableObject
{
    [SerializeField] TextAsset file;
    [SerializeField] string[] names;

    public string[] Names => names;

    [ContextMenu("Update Racers names")]
    void UpdateNames() {
        names = file.text.Split("\n");
    }
}
