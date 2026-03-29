using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Attach this to a GameObject in the Level Select scene.
/// Buttons are auto-wired by name in Awake() — no Inspector drag needed.
/// Expected button names: Level1Button … Level10Button, BackButton
/// Each level button should have 3 child Image objects named Star1, Star2, Star3
/// that are enabled/disabled to show the player's best star rating.
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
                case "Level1Button":  btn.onClick.AddListener(SelectLevel1);  break;
                case "Level2Button":  btn.onClick.AddListener(SelectLevel2);  break;
                case "Level3Button":  btn.onClick.AddListener(SelectLevel3);  break;
                case "Level4Button":  btn.onClick.AddListener(SelectLevel4);  break;
                case "Level5Button":  btn.onClick.AddListener(SelectLevel5);  break;
                case "Level6Button":  btn.onClick.AddListener(SelectLevel6);  break;
                case "Level7Button":  btn.onClick.AddListener(SelectLevel7);  break;
                case "Level8Button":  btn.onClick.AddListener(SelectLevel8);  break;
                case "Level9Button":  btn.onClick.AddListener(SelectLevel9);  break;
                case "Level10Button": btn.onClick.AddListener(SelectLevel10); break;
                case "BackButton":    btn.onClick.AddListener(BackToMenu);    break;
            }

            // Lock/unlock interactability based on save data
            for (int n = 1; n <= 10; n++)
            {
                if (btn.name == "Level" + n + "Button")
                {
                    btn.interactable = SaveManager.IsLevelUnlocked(n);
                    break;
                }
            }
        }

        RefreshStarDisplay(buttons);
    }

    // -------------------------------------------------------
    // Level Select methods
    // -------------------------------------------------------

    public void SelectLevel1()  { LevelSelection.Selected = LevelConfig.Level1();  LoadGame(); }
    public void SelectLevel2()  { LevelSelection.Selected = LevelConfig.Level2();  LoadGame(); }
    public void SelectLevel3()  { LevelSelection.Selected = LevelConfig.Level3();  LoadGame(); }
    public void SelectLevel4()  { LevelSelection.Selected = LevelConfig.Level4();  LoadGame(); }
    public void SelectLevel5()  { LevelSelection.Selected = LevelConfig.Level5();  LoadGame(); }
    public void SelectLevel6()  { LevelSelection.Selected = LevelConfig.Level6();  LoadGame(); }
    public void SelectLevel7()  { LevelSelection.Selected = LevelConfig.Level7();  LoadGame(); }
    public void SelectLevel8()  { LevelSelection.Selected = LevelConfig.Level8();  LoadGame(); }
    public void SelectLevel9()  { LevelSelection.Selected = LevelConfig.Level9();  LoadGame(); }
    public void SelectLevel10() { LevelSelection.Selected = LevelConfig.Level10(); LoadGame(); }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    private void LoadGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Candy Catcher");
    }

    /// <summary>
    /// For each level button, finds child Image objects named Star1/Star2/Star3
    /// and enables them based on the player's saved star count.
    /// </summary>
    private void RefreshStarDisplay(Button[] buttons)
    {
        for (int n = 1; n <= 10; n++)
        {
            int stars = SaveManager.GetStars(n);

            foreach (var btn in buttons)
            {
                if (btn.name != "Level" + n + "Button") continue;

                for (int s = 1; s <= 3; s++)
                {
                    Transform starChild = btn.transform.Find("Star" + s);
                    if (starChild != null)
                    {
                        Image img = starChild.GetComponent<Image>();
                        if (img != null)
                            img.enabled = s <= stars;
                    }
                }
                break;
            }
        }
    }
}
