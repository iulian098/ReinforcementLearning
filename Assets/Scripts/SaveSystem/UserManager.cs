using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UserManager
{
    public static PlayerData playerData;

    public static void SetPlayerData(PlayerData data) {
        playerData = data;
    }
}
