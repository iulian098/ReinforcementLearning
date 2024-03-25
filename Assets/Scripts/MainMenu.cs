using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string playScene;

    [SerializeField] Image imgTest;
    [SerializeField] GameObject prefabTest;

    public void OnPlay() {
        SceneManager.LoadScene(playScene);
    }


    public void OnTest() {
        imgTest.sprite = IconCreator.CreateSprite(prefabTest, new Vector3(0, 0, -5), new Vector3(0, 90, 0), new Rect(0, 0, 512, 512));
    }
}
