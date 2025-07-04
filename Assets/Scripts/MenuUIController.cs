using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUIController : MonoBehaviour {

    public GameObject mainMenuHolder;
    public GameObject optionsMenuHolder;

    public void PlayGame() {
        SceneManager.LoadScene("Game");
    }

    public void QuitGame() {
        Application.Quit();
    }

    public void OpenOptions() {
        mainMenuHolder.SetActive(false);
        optionsMenuHolder.SetActive(true);
    }

    public void BackToMainMenu() {
        mainMenuHolder.SetActive(true);
        optionsMenuHolder.SetActive(false);
    }
}
