using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn == null) return;

        btn.onClick.AddListener(PlaySound);
    }

    private void PlaySound() {
        AudioManager.Instance.PlayCommonAudio(AudioManager.Instance.AudioDataContainer.ButtonClickClip, false, 0.9f, 1.1f);
    }
}
