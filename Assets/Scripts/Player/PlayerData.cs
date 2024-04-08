using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [SerializeField, JsonProperty] Dictionary<string, int> ints = new Dictionary<string, int>();
    [SerializeField, JsonProperty] Dictionary<string, string> strings = new Dictionary<string, string>();

    public void SetInt(string key, int value) {
        if(ints.ContainsKey(key)) ints[key] = value;
        else ints.Add(key, value);
    }

    public void SetString(string key, string value) {
        if(strings.ContainsKey(key)) strings[key] = value;
        else strings.Add(key, value);
    }

    public int GetInt(string key, int defaultValue = 0) {
        if(ints.TryGetValue(key, out int val))
            return val;
        else
            return defaultValue;
    }

    public string GetString(string key, string devaultValue = "") {
        if(strings.TryGetValue(key, out string val))
            return val;
        else
            return devaultValue;
    }
}
