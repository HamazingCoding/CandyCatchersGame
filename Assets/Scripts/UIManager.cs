using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Score")]
    public TMP_Text scoreText;

    [Header("Lives / Hearts")]
    public Image[] heartImages; // assign 3 heart images

    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    [Header("Game Over Elements")]
    public TMP_Text finalScoreText;

    void Start()
    {
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    // ------------------------------
    // SCORE
    // ------------------------------
    public void UpdateScore(int score)
    {
        scoreText.text = "Score: " + score.ToString();
    }

    // ------------------------------
    // LIVES
    // ------------------------------
    public void UpdateLives(int lives)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            heartImages[i].enabled = i < lives;
        }
    }

    // ------------------------------
    // PAUSE
    // ------------------------------
    public void ShowPause(bool show)
    {
        pausePanel.SetActive(show);
    }

    // ------------------------------
    // GAME OVER
    // ------------------------------
    public void ShowGameOver(int finalScore)
    {
        
        gameOverPanel.SetActive(true);
    }
}
