using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Score & Gems HUD")]
    public TMP_Text scoreText;
    public TMP_Text gemsText;

    [Header("Lives / Hearts")]
    public Image[] heartImages;   // heart icon images
    public TMP_Text livesCountText; // optional "x 5" label next to hearts

    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject revivePanel;
    public GameObject levelCompletePanel;

    [Header("Game Over Elements")]
    public TMP_Text finalScoreText;

    [Header("Revive Panel Elements")]
    public Button freeReviveButton;
    public Button paidReviveButton;
    public TMP_Text paidReviveCostText;
    public TMP_Text reviveGemsBalanceText;

    [Header("Level Complete Panel Elements")]
    public Image[] levelCompleteStars; // 3 star images (filled = earned)
    public TMP_Text gemsAwardedText;

    void Start()
    {
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        revivePanel.SetActive(false);
        levelCompletePanel.SetActive(false);
    }

    // -------------------------------------------------------
    // SCORE
    // -------------------------------------------------------
    public void UpdateScore(int score)
    {
        scoreText.text = "Score: " + score.ToString();
    }

    // -------------------------------------------------------
    // GEMS
    // -------------------------------------------------------
    public void UpdateGems(int gems)
    {
        if (gemsText != null)
            gemsText.text = gems.ToString() + " Gems";
    }

    // -------------------------------------------------------
    // LIVES
    // -------------------------------------------------------
    public void UpdateLives(int lives)
    {
        // Show all hearts filled up to min(lives, heartImages.Length)
        for (int i = 0; i < heartImages.Length; i++)
            heartImages[i].enabled = i < lives;

        // If lives can exceed the number of heart images, show a count label
        if (livesCountText != null)
            livesCountText.text = lives > 0 ? "x" + lives : "";
    }

    // -------------------------------------------------------
    // PAUSE
    // -------------------------------------------------------
    public void ShowPause(bool show)
    {
        pausePanel.SetActive(show);
    }

    // -------------------------------------------------------
    // GAME OVER
    // -------------------------------------------------------
    public void ShowGameOver(int finalScore)
    {
        gameOverPanel.SetActive(true);
        if (finalScoreText != null)
            finalScoreText.text = "Score: " + finalScore.ToString();
    }

    // -------------------------------------------------------
    // REVIVE PANEL
    // -------------------------------------------------------

    /// <summary>
    /// Show the revive panel. hasFreeRevive=true shows the free button only;
    /// false shows the paid button only (already confirmed player can afford it).
    /// </summary>
    public void ShowRevivePanel(bool hasFreeRevive, int playerGems)
    {
        revivePanel.SetActive(true);

        if (reviveGemsBalanceText != null)
            reviveGemsBalanceText.text = playerGems.ToString() + " Gems";

        if (freeReviveButton != null)
            freeReviveButton.gameObject.SetActive(hasFreeRevive);

        if (paidReviveButton != null)
        {
            paidReviveButton.gameObject.SetActive(!hasFreeRevive);
            if (paidReviveCostText != null)
                paidReviveCostText.text = "Revive (100 Gems)";
        }
    }

    public void HideRevivePanel()
    {
        revivePanel.SetActive(false);
    }

    // -------------------------------------------------------
    // LEVEL COMPLETE PANEL
    // -------------------------------------------------------
    public void ShowLevelComplete(int stars, int gemsAwarded)
    {
        levelCompletePanel.SetActive(true);

        // Fill star images based on stars earned (1–3)
        if (levelCompleteStars != null)
        {
            for (int i = 0; i < levelCompleteStars.Length; i++)
            {
                if (levelCompleteStars[i] != null)
                    levelCompleteStars[i].enabled = i < stars;
            }
        }

        if (gemsAwardedText != null)
            gemsAwardedText.text = "+" + gemsAwarded + " Gems";
    }
}
