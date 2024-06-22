using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITrackInfo : MonoBehaviour
{
    [SerializeField] TracksContainer tracksContainer;
    [SerializeField] Image trackIcon;
    [SerializeField] UITrackRewardInfo buyInInfo;
    [SerializeField] Transform infoContainer;
    [SerializeField] UITrackRewardInfo infoPrefab;
    [SerializeField] MainMenu mainMenu;

    List<UITrackRewardInfo> rewardInfos = new List<UITrackRewardInfo>();

    RaceData raceData;
    int raceIndex = 0;

    public void SetRaceData(RaceData raceData) {
        this.raceData = raceData;
        raceIndex = Array.IndexOf(tracksContainer.Tracks, raceData);
        ClearInfos();

        if(raceData.BuyIn > 0) {
            buyInInfo.Init("Buy-in:", raceData.BuyIn.ToString() + "$");
        }
        else {
            buyInInfo.Init("Buy-in:", "FREE");
        }

        for (int i = 0; i < raceData.CoinsRewards.Length; i++) {
            UITrackRewardInfo info = Instantiate(infoPrefab, infoContainer);
            rewardInfos.Add(info);
            
            string placeText = $"{i+1}th:";

            switch (i) {
                case 0:
                    placeText = $"{i + 1}st:";
                    break;
                case 1:
                    placeText = $"{i + 1}nd:";
                    break;
                case 2:
                    placeText = $"{i + 1}rd:";
                    break;
            }

            info.Init(placeText, raceData.CoinsRewards[i].ToString() + "$");
        }
    }

    public void StartRace() {
        if(UserManager.playerData.GetInt(PlayerPrefsStrings.CASH) < raceData.BuyIn) {
            PopupPanel.Instance.Show("", "Not enough money.", null);
            return;
        }
        UserManager.playerData.SubtractInt(PlayerPrefsStrings.CASH, raceData.BuyIn);
        GlobalData.lastPlayedTrack = raceIndex;
        GlobalData.selectedRaceData = raceData;
        mainMenu.OnPlay(raceData);
    }

    void ClearInfos() {
        foreach (var item in rewardInfos) {
            Destroy(item.gameObject);
        }

        rewardInfos.Clear();
    }

}
