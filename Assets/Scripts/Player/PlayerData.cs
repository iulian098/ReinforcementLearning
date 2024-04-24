using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [SerializeField, JsonProperty] Dictionary<string, int> ints = new Dictionary<string, int>();
    [SerializeField, JsonProperty] Dictionary<string, float> floats = new Dictionary<string, float>();
    [SerializeField, JsonProperty] Dictionary<string, string> strings = new Dictionary<string, string>();
    [SerializeField, JsonProperty] Dictionary<string, bool> bools = new Dictionary<string, bool>();

    [JsonIgnore] public Action<string> OnValueChanged;

    #region String

    public void SetString(string key, string value) {
        if(strings.ContainsKey(key)) strings[key] = value;
        else strings.Add(key, value);
        OnValueChanged?.Invoke(key);
    }

    public string GetString(string key, string devaultValue = "") {
        if(strings.TryGetValue(key, out string val))
            return val;
        else
            return devaultValue;
    }

    #endregion

    #region Int

    public void SetInt(string key, int value) {
        if (ints.ContainsKey(key)) ints[key] = value;
        else ints.Add(key, value);
        OnValueChanged?.Invoke(key);
    }

    public int GetInt(string key, int defaultValue = 0) {
        if (ints.TryGetValue(key, out int val))
            return val;
        else
            return defaultValue;
    }

    public void AddInt(string key, int value) {
        if (ints.ContainsKey(key))
            ints[key] += value;
        else
            ints.Add(key, value);
        OnValueChanged?.Invoke(key);
    }

    public void SubtractInt(string key, int value) {
        if (ints.ContainsKey(key))
            ints[key] -= value;
        else
            ints.Add(key, value);
        OnValueChanged?.Invoke(key);
    }

    #endregion

    #region Float

    public void SetFloat(string key, float value) {
        if (floats.ContainsKey(key)) floats[key] = value;
        else floats.Add(key, value);
        OnValueChanged?.Invoke(key);
    }

    public float GetFloat(string key, float defaultValue = 0) {
        if (floats.TryGetValue(key, out float val))
            return val;
        else
            return defaultValue;
    }

    public void AddFloat(string key, float value) {
        if (floats.ContainsKey(key))
            floats[key] += value;
        else
            floats.Add(key, value);
        OnValueChanged?.Invoke(key);
    }

    public void SubtractFloat(string key, float value) {
        if (floats.ContainsKey(key))
            floats[key] -= value;
        else
            floats.Add(key, value);
                OnValueChanged?.Invoke(key);
    }

    #endregion

    #region Bool

    public void SetBool(string key, bool value) {
        if (bools.ContainsKey(key)) bools[key] = value;
        else bools.Add(key, value);
        OnValueChanged?.Invoke(key);
    }

    public bool GetBool(string key, bool defaultValue = false) {
        if (bools.TryGetValue(key, out bool val))
            return val;
        else
            return defaultValue;
    }

    #endregion

}
