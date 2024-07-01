using System.Collections;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Reflection;
using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using System.Runtime.CompilerServices;

[DefaultExecutionOrder(-50)]
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;
    public static bool stopSaving;

    const string FILE_NAME = "saveFile.data";

    [SerializeField] VehiclesContainer vehiclesContainer;
    [SerializeField] AssetReferenceT<TracksContainer> tracksContainer;
    [SerializeField] TracksSaveDataContainer tracksSaveDataContainer;
    [SerializeField] GameSettings gameSettings;

    JsonSerializerSettings jsonSettings = new JsonSerializerSettings() {
        MaxDepth = null,
        CheckAdditionalContent = true
    };
    string filePath;
    SaveFile saveFile;
    SaveFile cloudSaveFile;
    FirebaseFirestore firestoreDatabase;
    Coroutine saveGameCoroutine;

    public Action OnSaveFileLoaded;

    async void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this);

    }

    private void Start() {
        gameSettings.Load(true);

        AuthenticationManager.Instance.OnUserLoggedOut += (() => {
            StopCoroutine(saveGameCoroutine);
            saveGameCoroutine = null;
        });

        AuthenticationManager.Instance.OnUserLoggedIn += (result) => {
            if(result)
                saveGameCoroutine = StartCoroutine(SaveGameCoroutine());
        };
    }

    public async Task Init() {
        filePath = Path.Combine(Application.persistentDataPath, FILE_NAME);

        saveFile = LoadFile();
        cloudSaveFile = await LoadCloudSave();

        if (cloudSaveFile != null && cloudSaveFile.saveDate > saveFile.saveDate) {
            PopupPanel.Instance.Show("", "Cloud save is newer than local save.\nUse cloud save?",
            () => {
                saveFile = cloudSaveFile;
                LoadGameData();
            }
            , true, LoadGameData);
        }
        else if (cloudSaveFile != null && cloudSaveFile.saveDate < saveFile.saveDate) {
            PopupPanel.Instance.Show("", "Cloud save is older than local save.\nUse local save?", LoadGameData, true,
            () => {
                saveFile = cloudSaveFile;
                LoadGameData();
            });
        }
        else
            LoadGameData();
    }

    IEnumerator SaveGameCoroutine() {
        yield return new WaitForSeconds(5);
        SaveFile();
        yield return SaveCloud();
        saveGameCoroutine = StartCoroutine(SaveGameCoroutine());
    }

     async void LoadGameData() {
        if (saveFile == null) {
            Debug.LogError("[SaveSystem] No save file found, creating new one");
            saveFile = new SaveFile();
        }

        TracksContainer tracksContainer = await AssetsManager<TracksContainer>.Load(this.tracksContainer);
        await tracksContainer.Init();
        UserManager.playerData = saveFile.playerData;
        vehiclesContainer.SetSaveData(saveFile.vehicleSaveData);
        tracksSaveDataContainer.SetSaveDatas(tracksContainer.Tracks, saveFile.tracksSaveData);

        saveGameCoroutine = StartCoroutine(SaveGameCoroutine());
        OnSaveFileLoaded?.Invoke();

        Debug.Log("[SaveSystem] Save File Loaded");
    } 

    [ContextMenu("ClearSave")]
    void ClearSave() {
        filePath = Path.Combine(Application.persistentDataPath, FILE_NAME);

        if (File.Exists(filePath))
            File.Delete(filePath);
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    #region Save/Load

    async Task<SaveFile> LoadCloudSave() {
        try {
            firestoreDatabase = FirebaseFirestore.DefaultInstance;
            DocumentReference docRef = firestoreDatabase.Collection("Players").Document(AuthenticationManager.Instance.UserId);
            DocumentSnapshot docSnapshot = await docRef.GetSnapshotAsync();
            string saveFileString = string.Empty;

            if (docSnapshot.Exists) {
                Dictionary<string, object> data = docSnapshot.ToDictionary();
                if (data.TryGetValue("SaveFile", out object cloudSaveFile)) {
                    saveFileString = cloudSaveFile.ToString();
                }
            }
            else {
                Debug.LogError($"Document {AuthenticationManager.Instance.UserId} does not exist!");
                return null;
            }
            SaveFile cloudSave;
            try {
                cloudSave = JsonConvert.DeserializeObject(saveFileString, typeof(SaveFile), jsonSettings) as SaveFile;
            }
            catch {
                Debug.LogError("Failed to deserialize the save file");
                return null;
            }
            Debug.Log("[SaveSystem] Cloud save loaded");
            return cloudSave;
        }catch (Exception ex) {
            Debug.LogError(ex.Message);
        }
        return null;

    }

    async Task SaveCloud() {

        DocumentReference docRef = firestoreDatabase.Collection("Players").Document(AuthenticationManager.Instance.UserId);
        DocumentSnapshot docSnapshot = await docRef.GetSnapshotAsync();
        string data = JsonConvert.SerializeObject(saveFile, jsonSettings);

        if (docSnapshot.Exists) {
            await docRef.UpdateAsync("SaveFile", data);
        }
        else {
            Dictionary<string, object> playerData = new Dictionary<string, object>() {
                { "SaveFile", data}
            };
            await docRef.SetAsync(playerData);
        }


        Debug.Log("Saved to cloud");
    }

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
        saveFile.saveDate = DateTime.UtcNow;

        if (stopSaving) {
            Debug.Log("[SaveSystem] Save disabled");
            return;
        }

        try {
            using (StreamWriter sw = new StreamWriter(filePath)) {
                string data = JsonConvert.SerializeObject(saveFile, jsonSettings);
                sw.Write(data);
            }
        }catch(IOException ex) {
            Debug.LogError(ex.Message);
        }

        Debug.Log("[SaveSystem] Saved");
    }

    #endregion

}
