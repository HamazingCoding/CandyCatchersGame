using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Stats")]
    public int lives = 3;
    public int score = 0;
    public int missedAllowed = 5;

    private int missedCount = 0;

    [Header("Difficulty")]
    public float candyFallSpeedBase = 3f;
    public float difficultyIncrease = 1.01f;
    public CandySpawner spawner;
    public AudioManager audioManager;


    [Header("UI")]
    public UIManager uiManager;

    private bool gameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        uiManager.UpdateScore(score);
        uiManager.UpdateLives(lives);
    }

    public void HandleCandyCollected(Collectible c)
    {
        int value = c.scoreValue;
        if (c.isBonus) value *= 5;

        score += value;
        uiManager.UpdateScore(score);

        audioManager.PlayTreat();   

        candyFallSpeedBase *= difficultyIncrease;
    }

    public void HandleTrickCollected(Collectible c)
    {
        lives--;

        uiManager.UpdateLives(lives);

        audioManager.PlayTrick();

        if (lives <= 0)
            GameOver();
    }

    public void HandleCandyMissed(Collectible c)
    {
        missedCount++;

        audioManager.PlayMiss();

        if (missedCount >= missedAllowed)
            GameOver();
    }

    void GameOver()
    {
        if (gameOver) return;

        gameOver = true;
        spawner.StopSpawning();
        uiManager.ShowGameOver(score);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Pause()
    {
        if (gameOver) return;                // don't pause after game over
        Time.timeScale = 0f;                 // stop time
        uiManager.ShowPause(true);           // show pause panel
    }

    // Resume the game from pause
    public void Resume()
    {
        Time.timeScale = 1f;                 // resume time
        uiManager.ShowPause(false);          // hide pause panel
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }


    // Optional helper to toggle pause state
    public void TogglePause()
    {
        if (Time.timeScale == 0f)
            Resume();
        else
            Pause();
    }

}
