using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ModifyTerrain))]
public class ModifyTerrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ModifyTerrain modifyTerrain = (ModifyTerrain)target;
        if(GUILayout.Button("Modify terrain"))
        {
            modifyTerrain.UpdateTerrain();
        }
    }
}
