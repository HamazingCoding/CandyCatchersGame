using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Attach this to a GameObject in the Level Select scene.
/// Buttons are auto-wired by name in Awake() — no Inspector drag needed.
/// Expected button names: Level1Button, Level2Button, Level3Button, BackButton
/// </summary>
public class LevelSelectManager : MonoBehaviour
{
    void Awake()
    {
        var buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var btn in buttons)
        {
            switch (btn.name)
            {
                case "Level1Button": btn.onClick.AddListener(SelectLevel1); break;
                case "Level2Button": btn.onClick.AddListener(SelectLevel2); break;
                case "Level3Button": btn.onClick.AddListener(SelectLevel3); break;
                case "BackButton":   btn.onClick.AddListener(BackToMenu);   break;
            }
        }
    }

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
