using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadGame()
    {
        Time.timeScale = 1f; // safety reset (pause-proof)
        SceneManager.LoadScene("Candy Catcher");
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    public void LoadLevelSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level Select");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
