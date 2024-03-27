using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SaveFileBase
{
    [SerializeField] Dictionary<string, bool> bools = new Dictionary<string, bool>();
    [SerializeField] Dictionary<string, int> ints = new Dictionary<string, int>();
    [SerializeField] Dictionary<string, float> floats = new Dictionary<string, float>();
    [SerializeField] Dictionary<string, string> strings = new Dictionary<string, string>();

    #region Get

    public bool GetBool(string key, bool defaultValue = false) {
        return bools.TryGetValue(key, out bool value) ? value : defaultValue;
    }

    public int GetInt(string key, int defaultValue = 0) {
        return ints.TryGetValue(key, out int value) ? value : defaultValue;
    }

    public float GetFloat(string key, float defaultValue = 0) {
        return floats.TryGetValue(key, out float value) ? value : defaultValue;
    }

    public string GetString(string key, string defaultValue = "") {
        return strings.TryGetValue(key, out string value) ? value : defaultValue;
    }

    #endregion

    #region Set

    public void SetBool(string key, bool value) {
        bools[key] = value;
    }

    public void SetInt(string key, int value) {
        ints[key] = value;
    }

    public void SetFloat(string key, float value) {
        floats[key] = value;
    }

    public void SetString(string key, string value) {
        strings[key] = value;
    }

    #endregion
}
