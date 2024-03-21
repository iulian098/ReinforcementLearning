using TMPro;
using UnityEngine;

public class UILeaderboardItem : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    
    public void SetText(string val) {
        text.text = val;
    }
}
