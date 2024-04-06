using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Racing {

    public class SceneManager : MonoSingleton<SceneManager> {
        [SerializeField] GameObject loadingPanel;

        public Action OnSceneLoaded;

        bool loadingInProgress;

        protected override void OnAwake() {
            DontDestroyOnLoad(this);
        }

        public void LoadScenes(params object[] scenes) {
            if (loadingInProgress) {
                Debug.LogError("Scenes loading is already in progress");
                return;
            }
            _ = ShowLoadingScreen(scenes);
        }

        async Task ShowLoadingScreen(params object[] scenes) {
            loadingPanel.SetActive(true);

            Debug.Log("StartLoadingScenes");
            for (int i = 0; i < scenes.Length; i++) {
                Debug.Log(scenes[i]);
                AsyncOperationHandle<SceneInstance> sceneLoader;
                sceneLoader = Addressables.LoadSceneAsync(scenes[i], i == 0 ? LoadSceneMode.Single : LoadSceneMode.Additive);
                await sceneLoader.Task;
            }

            OnSceneLoaded?.Invoke();

            loadingPanel.SetActive(false);

            OnSceneLoaded = null;
        }
    }

}
