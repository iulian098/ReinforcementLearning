using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.RemoteConfig;
using System.Threading.Tasks;
using System;
using System.ComponentModel;

public class RemoteConfigManager : MonoSingleton<RemoteConfigManager>
{
    Dictionary<string, string> remoteConfigData = new Dictionary<string, string>();
    FirebaseRemoteConfig firebaseRemoteConfig;

    public Dictionary<string, string> RemoteConfigData => remoteConfigData;

    bool dataUpdated = false;

    public async Task FetchData() {
        if (dataUpdated) return;
        
        if (firebaseRemoteConfig == null)
            firebaseRemoteConfig = FirebaseRemoteConfig.DefaultInstance;

        await firebaseRemoteConfig.FetchAsync(TimeSpan.Zero);

        var info = firebaseRemoteConfig.Info;

        if(info.LastFetchStatus != LastFetchStatus.Success) {
            Debug.LogError("Error Fetching");
            return;
        }

        await firebaseRemoteConfig.ActivateAsync();

        foreach (var key in firebaseRemoteConfig.Keys) {
            remoteConfigData.Add(key, firebaseRemoteConfig.GetValue(key).StringValue);
        }

        UpdateData();
    }

    public void UpdateData() {
        foreach (var variable in typeof(GlobalData).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)){
            if(remoteConfigData.TryGetValue(variable.Name, out string val)) {
                variable.SetValue(null, StringToValue(val, variable.FieldType));
            }
        }

        PrintValues();

        dataUpdated = true;
    }

    public void PrintValues() {
        foreach (var variable in typeof(GlobalData).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)) {
            Debug.Log(variable.GetValue(null));
        }

    }

    object StringToValue(string value, Type toType) {
        if (toType == typeof(int)) {
            return int.Parse(value);
        }
        else if (toType == typeof(float)) {
            return float.Parse(value);
        }
        else if (toType == typeof(double)) {
            return double.Parse(value);
        }
        else if (toType == typeof(bool)) {
            return bool.Parse(value);
        }

        return TypeDescriptor.GetConverter(toType).ConvertFromString(value);
    }
}
