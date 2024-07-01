using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrackItem : MonoBehaviour
{
    [SerializeField] Image thumbnail;
    [SerializeField] Image selectedImage;
    [SerializeField] TMP_Text trackNameText;
    [SerializeField] Sprite starFilled;
    [SerializeField] Image[] stars;
    [SerializeField] GameObject eventBadge;
    [SerializeField] GameObject lockedGameObject;
    [SerializeField] TMP_Text unlockLevelText;

    Action<TrackItem, RaceData> onClick;
    RaceData raceData;

    public void Init(RaceData data, Action<TrackItem, RaceData> onClick) {
        this.onClick = onClick;
        raceData = data;

        thumbnail.sprite = raceData.Thumbnail;
        trackNameText.text = raceData.TrackName;

        if (GlobalData.enableSpecialEventBonus && data.UseEvent)
            eventBadge.SetActive(true);

        if (UserManager.playerData.GetInt(PlayerPrefsStrings.LEVEL) < data.UnlockLevel) {
            lockedGameObject.SetActive(true);
            unlockLevelText.text = "Unlocks at level " + data.UnlockLevel;
        }
        else
            lockedGameObject.SetActive(false);

        if (data.saveData.placement == -1) return;

        for (int i = 0; i < stars.Length; i++) {
            if(i < 3 - data.saveData.placement)
                stars[i].overrideSprite = starFilled;
        }

    }

    public void OnClick() {
        onClick?.Invoke(this, raceData);
    }

    public void SetSelected(bool value) {
        selectedImage.gameObject.SetActive(value);
    }
}
