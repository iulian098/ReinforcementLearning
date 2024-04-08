using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.SceneManagement;

public class InGameMenu : MonoBehaviour
{
    [SerializeField] GameObject inGameMenuObj;

    AudioSource[] playingAudioSources;

    void Start()
    {
        PanelManager.Instance.OnExitPopupShow += Show;
    }

    private void OnDestroy() {
        PanelManager.Instance.OnExitPopupShow -= Show;
    }

    void Show() {
        PanelManager.Instance.ShowPanel("InGameMenu", OnShow, OnHide);
    }

    public void Hide() {
        PanelManager.Instance.HidePanel();
    }

    void OnShow() {
        playingAudioSources = System.Array.FindAll(FindObjectsByType<AudioSource>(FindObjectsSortMode.None), x => x.isPlaying);

        foreach (var item in playingAudioSources) {
            item.Pause();
        }
        Time.timeScale = 0;
        inGameMenuObj.SetActive(true);
    }

    void OnHide() {
        foreach (var item in playingAudioSources)
            item.UnPause();


        Time.timeScale = 1;
        inGameMenuObj.SetActive(false);
    }

    public void Exit() {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
}
