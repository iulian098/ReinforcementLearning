using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    const string FILE_NAME = "saveFile.data";

    [SerializeField] VehiclesContainer vehiclesContainer;

    string path;
    string filePath;
    SaveFile saveFile;

    void Start()
    {
        DontDestroyOnLoad(this);
        filePath = Path.Combine(Application.persistentDataPath, FILE_NAME);

        saveFile = LoadFile();

        StartCoroutine(SaveGameCoroutine());

        LoadGameData();
    }

    IEnumerator SaveGameCoroutine() {
        yield return new WaitForSeconds(5);
        SaveFile();
        StartCoroutine(SaveGameCoroutine());
    }

    void LoadGameData() {
        if (saveFile == null) {
            Debug.LogError("No save file found, creating new one");
            saveFile = new SaveFile();
        }

        vehiclesContainer.vehicleSaveDatas = saveFile.vehicleSaveData;
    }

    #region Save/Load

    SaveFile LoadFile() {
        SaveFile file;

        string data;

        /*#if UNITY_WEBGL && !UNITY_EDITOR
                data = LoadData();
                file = JsonUtility.FromJson<SaveFile>(data);
                Debug.Log("LoadData = " + data);
        #else*/
        if (File.Exists(filePath)) {
            try {
                using (var reader = new StreamReader(filePath)) {
                    data = reader.ReadToEnd();
                    file = JsonUtility.FromJson<SaveFile>(data);
                }
                Debug.Log("[SaveSystem] Save file loaded");
            }
            catch (IOException ex) {
                Debug.LogError(ex.Message);
                file = new SaveFile();
                Debug.Log("[SaveSystem] Created new save file");
            }
        }
        else {
            file = new SaveFile();
            Debug.Log("[SaveSystem] Created new save file");
        }
        //#endif
        
        return file;
    }

    void SaveFile() {
//#if UNITY_WEBGL && !UNITY_EDITOR
        //string data = JsonUtility.ToJson(saveFile);
        //SaveData($"'{data}'");
        //Debug.Log("SaveData = " + data);

//#else
        try {
            using (StreamWriter sw = new StreamWriter(filePath))
                sw.Write(JsonUtility.ToJson(saveFile));
        }catch(IOException ex) {
            Debug.LogError(ex.Message);
        }
        Debug.Log("[SaveSystem] Saved");
        //#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern void SaveData(string data);

        [DllImport("__Internal")]
        public static extern string LoadData();
#endif


    #endregion

}
