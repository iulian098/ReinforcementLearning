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
        UpdateLevel();
        //UserManager.playerData.AddInt(PlayerPrefsStrings.CASH, 100000);
    }

    private void OnDestroy() {
        UserManager.playerData.OnValueChanged -= OnProfileUpdated;
    }

    public void OnProfileUpdated(string key) {
        if (key == PlayerPrefsStrings.CASH)
            UpdateProfile();
        else if (key == PlayerPrefsStrings.LEVEL || key == PlayerPrefsStrings.EXP)
            UpdateLevel();
    }

    void UpdateLevel() {
        levelText.text = (UserManager.playerData.GetInt(PlayerPrefsStrings.LEVEL) + 1).ToString();
        levelFill.fillAmount = LevelSystem.Instance.GetLevelProgress();
    }

    void UpdateProfile() {
        cashText.text = UserManager.playerData.GetInt(PlayerPrefsStrings.CASH).ToString();
    }
}
