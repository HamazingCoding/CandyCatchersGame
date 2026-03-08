using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to a GameObject in the Level Select scene.
/// Wire the 3 buttons: Level1() / Level2() / Level3()
/// </summary>
public class LevelSelectManager : MonoBehaviour
{
    public void SelectLevel1()
    {
        LevelSelection.Selected = LevelConfig.Level1();
        LoadGame();
    }

    public void SelectLevel2()
    {
        LevelSelection.Selected = LevelConfig.Level2();
        LoadGame();
    }

    public void SelectLevel3()
    {
        LevelSelection.Selected = LevelConfig.Level3();
        LoadGame();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    private void LoadGame()
    {
        Time.timeScale = 1f;               // safety reset in case coming from pause
        SceneManager.LoadScene("Candy Catcher");
    }
}
