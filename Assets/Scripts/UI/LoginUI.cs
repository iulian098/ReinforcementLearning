using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [SerializeField] Button btnGuest;

    private void Start() {
        btnGuest.onClick.AddListener(() => AuthenticationManager.Instance.Login(0));
    }
}
