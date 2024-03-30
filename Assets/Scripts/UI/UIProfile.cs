using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIProfile : MonoBehaviour
{
    [SerializeField] TMP_Text cashText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] Image levelFill;

    private void Start() {
        cashText.text = UserManager.playerData.GetInt("Cash").ToString();
        levelText.text = UserManager.playerData.GetInt("Level").ToString();
    }
}
