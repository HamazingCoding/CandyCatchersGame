using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Stats")]
    public int lives = 3;
    public int score = 0;

    private int _candiesCaught = 0;
    private int _revivesUsed = 0;
    private bool _levelOver = false;

    [Header("Difficulty")]
    public float candyFallSpeedBase = 3f;
    public float difficultyIncrease = 1.01f;

    [Header("References")]
    public CandySpawner spawner;
    public AudioManager audioManager;
    public UIManager uiManager;

    private LevelConfig _currentConfig;
    private bool gameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _currentConfig = LevelSelection.Selected;

        lives               = _currentConfig.lives;
        candyFallSpeedBase  = _currentConfig.candyFallSpeedBase;
        difficultyIncrease  = _currentConfig.difficultyIncrease;

        ApplyLevelBackground(_currentConfig.levelNumber);
        ApplyLevelMusic(_currentConfig.levelNumber);

        if (spawner != null)
            spawner.ApplyConfig(_currentConfig);

        uiManager.UpdateScore(score);
        uiManager.UpdateLives(lives);
        uiManager.UpdateGems(SaveManager.GetGems());
    }

    void ApplyLevelBackground(int level)
    {
        var bg = Resources.Load<Sprite>("Backgrounds/Level" + level + "BG");
        if (bg == null) return;

        var bgObj = GameObject.Find("BG_sample_0");
        if (bgObj != null)
            bgObj.GetComponent<SpriteRenderer>().sprite = bg;
    }

    void ApplyLevelMusic(int level)
    {
        // Levels 4-7 have dedicated BGM; all others fall back to the default OST
        string clipName = (level >= 4 && level <= 7) ? "BGM/Level" + level + "BGM" : "BGM/DefaultBGM";
        var clip = Resources.Load<AudioClip>(clipName);
        audioManager.PlayLevelMusic(clip);
    }

    // -------------------------------------------------------
    // Candy / Trick event handlers (called by Collectible)
    // -------------------------------------------------------

    public void HandleCandyCollected(Collectible c)
    {
        int value = c.scoreValue;
        if (c.isBonus) value *= 5;

        score += value;
        _candiesCaught++;

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
        lives--;
        uiManager.UpdateLives(lives);
        audioManager.PlayMiss();

        if (lives <= 0)
            GameOver();
    }

    // -------------------------------------------------------
    // Level completion — triggered by CandySpawner via OnSpawningComplete
    // -------------------------------------------------------

    /// <summary>Called by CandySpawner once the last candy has been spawned.</summary>
    public void OnSpawningComplete()
    {
        if (!gameOver && !_levelOver)
            StartCoroutine(WaitForScreenClear());
    }

    IEnumerator WaitForScreenClear()
    {
        // Poll every 0.25 s until the spawn container is empty
        // (all candies/tricks have been caught, missed, or fallen off screen)
        while (spawner.spawnParent != null && spawner.spawnParent.childCount > 0)
        {
            yield return new WaitForSeconds(0.25f);
        }

        if (!gameOver)
        {
            _levelOver = true;
            EvaluateLevelComplete();
        }
    }

    void EvaluateLevelComplete()
    {
        spawner.StopSpawning();

        int stars = 0;
        if      (_candiesCaught >= _currentConfig.targetFor3Stars) stars = 3;
        else if (_candiesCaught >= _currentConfig.targetFor2Stars) stars = 2;
        else if (_candiesCaught >= _currentConfig.targetFor1Star)  stars = 1;

        if (stars == 0)
        {
            // Didn't reach even 1-star — treat as a failure
            GameOver();
            return;
        }

        // Award gems: 1★=5, 2★=15, 3★=30
        int[] gemRewards = { 0, 5, 15, 30 };
        int gemsAwarded = gemRewards[stars];

        SaveManager.SaveStars(_currentConfig.levelNumber, stars);
        SaveManager.AddGems(gemsAwarded);
        SaveManager.UnlockLevel(_currentConfig.levelNumber + 1);

        uiManager.UpdateGems(SaveManager.GetGems());
        uiManager.ShowLevelComplete(stars, gemsAwarded);
    }

    // -------------------------------------------------------
    // Game Over & Revive
    // -------------------------------------------------------

    void GameOver()
    {
        if (gameOver) return;

        gameOver = true;
        spawner.StopSpawning();

        if (_revivesUsed == 0)
        {
            Time.timeScale = 0f;
            uiManager.ShowRevivePanel(hasFreeRevive: true, playerGems: SaveManager.GetGems());
        }
        else if (SaveManager.CanAfford(100))
        {
            Time.timeScale = 0f;
            uiManager.ShowRevivePanel(hasFreeRevive: false, playerGems: SaveManager.GetGems());
        }
        else
        {
            uiManager.ShowGameOver(score);
        }
    }

    public void ReviveFree() => Revive(true);
    public void RevivePaid() => Revive(false);

    public void Revive(bool free)
    {
        if (!free) SaveManager.SpendGems(100);

        _revivesUsed++;
        lives    = _currentConfig.lives;
        gameOver = false;

        uiManager.UpdateLives(lives);
        uiManager.UpdateGems(SaveManager.GetGems());
        uiManager.HideRevivePanel();

        Time.timeScale = 1f;
        spawner.StartSpawning();
    }

    public void DeclineRevive()
    {
        Time.timeScale = 1f;
        uiManager.HideRevivePanel();
        uiManager.ShowGameOver(score);
    }

    // -------------------------------------------------------
    // Navigation
    // -------------------------------------------------------

    public void LoadNextLevel()
    {
        int next = _currentConfig.levelNumber + 1;
        if (next <= 10)
        {
            LevelSelection.Selected = LevelConfig.ForLevel(next);
            Time.timeScale = 1f;
            SceneManager.LoadScene("Candy Catcher");
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Level Select");
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Pause()
    {
        if (gameOver) return;
        Time.timeScale = 0f;
        uiManager.ShowPause(true);
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        uiManager.ShowPause(false);
    }

    public void TogglePause()
    {
        if (Time.timeScale == 0f) Resume();
        else Pause();
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToLevelSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level Select");
    }
}
