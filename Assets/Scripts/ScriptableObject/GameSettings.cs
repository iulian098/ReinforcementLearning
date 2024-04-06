using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings")]
public class GameSettings : ScriptableObject
{
    [SerializeField] int qualityLevel;
    [SerializeField] int antiAliasing = 0;
    [SerializeField] ShadowResolution shadowResolution;

    private void Setup() {
        QualitySettings.SetQualityLevel(qualityLevel, true);
        QualitySettings.antiAliasing = antiAliasing;
        QualitySettings.shadowResolution = shadowResolution;
    }

    public void UpdateValues() {

    }
}
