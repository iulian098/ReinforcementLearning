using TMPro;
using UnityEngine;

public class EmailLoginUI : MonoBehaviour
{
    [SerializeField] TMP_InputField emailInput;
    [SerializeField] TMP_InputField passInput;

    public void Show() {
        PanelManager.Instance.ShowPanel(name, OnShow, OnHide);
    }

    public void Hide() {
        PanelManager.Instance.HidePanel(name);
    }

    void OnShow() {
        gameObject.SetActive(true);
    }

    void OnHide() {
        gameObject.SetActive(false);
    }

    public void Login() {
        AuthenticationManager.Instance.Login((int)AuthenticationManager.LoginType.EmailLogin, emailInput.text, passInput.text);
    }
}
