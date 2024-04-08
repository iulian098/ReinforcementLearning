using System.Collections;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Reflection;

[DefaultExecutionOrder(-50)]
public class SaveSystem : MonoBehaviour
{
    static SaveSystem instance;
    public static bool stopSaving;

    const string FILE_NAME = "saveFile.data";

    [SerializeField] VehiclesContainer vehiclesContainer;
    [SerializeField] TracksContainer tracksContainer;

    JsonSerializerSettings jsonSettings = new JsonSerializerSettings() {
        MaxDepth = null,
        CheckAdditionalContent = true
    };
    string filePath;
    SaveFile saveFile;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != null && instance != this) {
            Destroy(this.gameObject);
            return;
        }

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
            Debug.LogError("[SaveSystem] No save file found, creating new one");
            saveFile = new SaveFile();
        }

        UserManager.playerData = saveFile.playerData;
        vehiclesContainer.SetSaveData(saveFile.vehicleSaveData);
        tracksContainer.SetSaveDatas(saveFile.tracksSaveData);

    }

    [ContextMenu("ClearSave")]
    void ClearSave() {
        filePath = Path.Combine(Application.persistentDataPath, FILE_NAME);

        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    #region Save/Load

    SaveFile LoadFile() {
        SaveFile file;

        string data;
        
        if (File.Exists(filePath)) {
            try {
                using (var reader = new StreamReader(filePath)) {
                    data = reader.ReadToEnd();
                    file = JsonConvert.DeserializeObject(data, typeof(SaveFile), jsonSettings) as SaveFile;
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
        
        return file;
    }

    void SaveFile() {
        if (stopSaving) {
            Debug.Log("[SaveSystem] Save disabled");
            return;
        }
        try {
            using (StreamWriter sw = new StreamWriter(filePath)) {
                sw.Write(JsonConvert.SerializeObject(saveFile, jsonSettings));
            }
        }catch(IOException ex) {
            Debug.LogError(ex.Message);
        }

        Debug.Log("[SaveSystem] Saved");
    }

    #endregion

}
