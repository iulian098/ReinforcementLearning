using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIProfile : MonoBehaviour
{
    [SerializeField] TMP_Text cashText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] Image levelFill;

    private void Start() {
        UserManager.playerData.OnValueChanged += OnProfileUpdated;
        UpdateProfile();
        //UserManager.playerData.AddInt(PlayerPrefsStrings.CASH, 100000);
    }

    private void OnDestroy() {
        UserManager.playerData.OnValueChanged -= OnProfileUpdated;
    }

    public void OnProfileUpdated(string key) {
        if (key == PlayerPrefsStrings.CASH)
            UpdateProfile();
    }

    public void UpdateProfile() {
        cashText.text = UserManager.playerData.GetInt(PlayerPrefsStrings.CASH).ToString();
        levelText.text = UserManager.playerData.GetInt(PlayerPrefsStrings.LEVEL).ToString();
    }
}
