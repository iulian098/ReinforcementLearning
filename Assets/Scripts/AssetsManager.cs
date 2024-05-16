using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public static class AssetsManager<T> where T : Object
{
    private static Dictionary<AssetReferenceT<T>, AsyncOperationHandle<T>> loadingAssets = new Dictionary<AssetReferenceT<T>, AsyncOperationHandle<T>>();
    private static Dictionary<AssetReferenceT<T>, AsyncOperationHandle<T>> loadedAssets = new Dictionary<AssetReferenceT<T>, AsyncOperationHandle<T>>();

    private static Dictionary<AssetReference, AsyncOperationHandle> loadedScenes = new Dictionary<AssetReference, AsyncOperationHandle>();
    public static async Task<T> Load(AssetReferenceT<T> reference) {
        if (loadedAssets.TryGetValue(reference, out var loadedAsset))
            return loadedAsset.Result;

        if(loadingAssets.TryGetValue(reference, out var loadingAsset)) {
            var task = loadingAsset.Task;
            await task;
            return task.Result;
        }

        var op = Addressables.LoadAssetAsync<T>(reference);
        loadingAssets.Add(reference, op);

        await op.Task;

        loadingAssets.Remove(reference);
        loadedAssets.Add(reference, op);
        return op.Result;
    }

    public static async Task Release(AssetReferenceT<T> reference) {
        if (!loadedAssets.TryGetValue(reference, out var asset))
            return;

        Addressables.Release(asset);

        loadedAssets.Remove(reference);
    }

    public static async Task LoadSceneAsync(AssetReferenceT<T> scene, LoadSceneMode loadSceneMode) {
        AsyncOperationHandle<SceneInstance> sceneLoader;
        sceneLoader = Addressables.LoadSceneAsync(scene, loadSceneMode);

        await sceneLoader.Task;

        loadedScenes.Add(scene, sceneLoader);
    }

    public static void ReleaseSceneaSync(AssetReferenceT<T> scene) {
        if (!loadedScenes.TryGetValue(scene, out var opHandle)) 
            return;

        Addressables.Release(opHandle);

        loadedScenes.Remove(scene);
    }
}
