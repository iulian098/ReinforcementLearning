using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupPanel : MonoBehaviour
{
    readonly int InHash = Animator.StringToHash("In");
    readonly int OutHash = Animator.StringToHash("Out");

    [SerializeField] Animator anim;
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text bodyText;
    [SerializeField] Button okButton;
    [SerializeField] Button cancelButton;

    Coroutine closingCoroutine;

    public void OnIn() {
        anim.Play(InHash);
    }

    public void Show(string title, string body, UnityAction okAction, bool showCancelButton = false, UnityAction cancelAction = null) {
        titleText.text = title;
        bodyText.text = body;

        okButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        okButton.onClick.AddListener(okAction);

        if(cancelAction != null)
            cancelButton.onClick.AddListener(cancelAction);

        cancelButton.gameObject.SetActive(showCancelButton);

        OnIn();
    }

    public void Close() {
        if(closingCoroutine == null)
            closingCoroutine = StartCoroutine(CloseCoroutine());
    }

    public IEnumerator CloseCoroutine() {
        anim.Play(OutHash);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        gameObject.SetActive(false);
        closingCoroutine = null;
    }

}
