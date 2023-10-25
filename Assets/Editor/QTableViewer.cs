using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class QTableViewer : EditorWindow {

    Vector2 scrollPos;
    Vector2 qTableScrollPos;
    public Dictionary<Vector2Int, float[]> qTable = new Dictionary<Vector2Int, float[]>();
    [MenuItem("Tools/My Custom Editor")]
    public static void ShowMyEditor() {
        // This method is called when the user selects the menu item in the Editor
        CreateInstance<QTableViewer>().Show();
       
    }

    void OnGUI() {
        if (GridEnvironment.instance == null) return;
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        int index = 0;
        EditorGUILayout.BeginHorizontal();

        foreach (var agent in GridEnvironment.instance.agentsList) {
            if (GUILayout.Button($"{agent.gameObject.name}")) {
                qTable = agent.qTable;
            }

            index++;
        }

        if(GUILayout.Button("Best Q")) {
            //qTable = MultipleAgentsEnvironment.instance.BestQTable;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
        UpdateQTable();
    }

    void UpdateQTable() {
        if (qTable == null) return;

        qTableScrollPos = EditorGUILayout.BeginScrollView(qTableScrollPos,false, true);
        EditorGUILayout.BeginVertical();

        foreach (var item in qTable) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("X: " + item.Key.x, GUILayout.MaxWidth(100));
            EditorGUILayout.LabelField("Y: " + item.Key.y, GUILayout.MaxWidth(100));
            foreach (var action in item.Value) {
                EditorGUILayout.LabelField(action.ToString(), GUILayout.MaxWidth(100));
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndHorizontal();

        }

        EditorGUILayout.EndVertical();


        EditorGUILayout.EndScrollView();

    }
}
