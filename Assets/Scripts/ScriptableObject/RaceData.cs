using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRaceData", menuName = "ScriptableObjects/New Race Data")]
public class RaceData : ScriptableObject
{
    public enum RaceType {
        Circuit,
        Sprint
    }

    [SerializeField] RaceType raceType;
    [SerializeField] int maxLoops;
    [SerializeField] int[] coinsReward;

}
