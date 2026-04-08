using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("Level Select");
    }

    public void SurvivalMode()
    {
        LevelSelection.Selected = LevelConfig.Endless();
        SceneManager.LoadScene("Candy Catcher");
    }

    public void Store()
    {
        SceneManager.LoadScene("StoreScene");
    }

    public void Settings()
    {
        SceneManager.LoadScene("SettingsScene");
    }

    public void Leaderboard()
    {
        SceneManager.LoadScene("LeaderboardScene");
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
