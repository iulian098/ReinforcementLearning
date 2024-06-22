using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance;


    private void Awake() {
        Instance = FindFirstObjectByType<T>();
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        }
        else if (Instance == null) {
            GameObject go = new GameObject(typeof(T).Name + "_SingletonInstanced");
            Instance = go.AddComponent<T>();
        }
        else {
            Instance = this as T;
            gameObject.name = typeof(T).Name + "_Singleton";
        }

        OnAwake();
    }

    protected virtual void OnAwake() { }

}
