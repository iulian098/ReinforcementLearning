using UnityEngine;

public class GlobalData
{
    public static int lastPlayedTrack = -1;
    public static int vehicleRespawnTime = 3;
    public static float specialEventBonus = 1.2f;
    public static bool enableSpecialEventBonus = false;
    public static RaceData selectedRaceData = null;
    public static bool IsGamePaused => Time.timeScale == 0;
}
