using System.Collections;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

[DefaultExecutionOrder(-50)]
public class SaveSystem : MonoBehaviour
{
    const string FILE_NAME = "saveFile.data";

    [SerializeField] VehiclesContainer vehiclesContainer;

    string filePath;
    SaveFile saveFile;

    void Awake()
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
            Debug.LogError("[SaveSystem] No save file found, creating new one");
            saveFile = new SaveFile();
        }

        UserManager.playerData = saveFile.playerData;
        //vehiclesContainer.vehicleSaveDatas = saveFile.vehicleSaveData;
        vehiclesContainer.SetSaveData(saveFile.vehicleSaveData);
    }

    #region Save/Load

    SaveFile LoadFile() {
        SaveFile file;

        string data;
        
        if (File.Exists(filePath)) {
            try {
                using (var reader = new StreamReader(filePath)) {
                    data = reader.ReadToEnd();
                    file = JsonConvert.DeserializeObject(data, typeof(SaveFile)) as SaveFile;
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
        try {
            using (StreamWriter sw = new StreamWriter(filePath))
                sw.Write(JsonConvert.SerializeObject(saveFile));
        }catch(IOException ex) {
            Debug.LogError(ex.Message);
        }

        Debug.Log("[SaveSystem] Saved");
    }

    #endregion

}
