using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateAccountUI : MonoBehaviour
{
    [SerializeField] TMP_InputField emailInput;
    [SerializeField] TMP_InputField passwordInput;
    [SerializeField] TMP_InputField repeatPasswordInput;
    [SerializeField] TMP_Text passwordStat;
    [SerializeField] TMP_Text repeatPasswordStat;
    [SerializeField] Button submitButton;

    bool validEmail, validPassword;

    private void Start() {
        emailInput.onEndEdit.AddListener(CheckEmail);
        passwordInput.onEndEdit.AddListener(CheckPassword);
        repeatPasswordInput.onEndEdit.AddListener(CheckRepeatPassword);
    }

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

    private void CheckEmail(string text) {
        if(!text.Contains('@') || !text.Contains('.') || string.IsNullOrEmpty(text)) {
            passwordStat.text = "Invalid email.";
            passwordStat.gameObject.SetActive(true);
            validPassword = false;
        }
        else {
            passwordStat.gameObject.SetActive(false);
            validEmail = true;
        }
        CanSubmit();
    }

    private void CheckPassword(string text) {
        if (text.Length < 6 || string.IsNullOrEmpty(text)) {
            passwordStat.text = "Invalid password. Min. 6 characters.";
            passwordStat.gameObject.SetActive(true);
            validPassword = false;
        }
        else {
            passwordStat.gameObject.SetActive(false);
        }
        CanSubmit();
    }

    private void CheckRepeatPassword(string text) {
        if(text != passwordInput.text || string.IsNullOrEmpty(text)) {
            repeatPasswordStat.text = "Passwords does not match.";
            repeatPasswordStat.gameObject.SetActive(true);
            validPassword = false;
        }
        else {
            repeatPasswordStat.gameObject.SetActive(false);
            validPassword = true;
        }
        CanSubmit();
    }

    private void CanSubmit() {
        submitButton.interactable = validEmail && validPassword;
    }

    public void CreateEmail() {
        AuthenticationManager.Instance.Login((int)AuthenticationManager.LoginType.EmailCreate, emailInput.text, passwordInput.text);//AuthenticationManager.Instance.CreateAccount(emailInput.text, passwordInput.text);
    }
}
