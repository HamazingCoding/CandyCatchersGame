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
    private bool _isEndless = false;
    private float _timeAlive = 0f;

    private PowerupHUD _powerupHUD;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1f;

        _currentConfig = LevelSelection.Selected;
        _isEndless = _currentConfig.isEndless;

        lives = _currentConfig.lives;
        candyFallSpeedBase = _currentConfig.candyFallSpeedBase;
        difficultyIncrease = _currentConfig.difficultyIncrease;

        ApplyLevelBackground(_currentConfig.levelNumber);
        ApplyLevelMusic(_currentConfig.levelNumber);

        if (spawner != null)
            spawner.ApplyConfig(_currentConfig);

        uiManager.UpdateScore(score);
        uiManager.UpdateLives(lives);
        uiManager.UpdateGems(SaveManager.GetGems());

        SetupPowerupHUD();

        if (_isEndless)
            uiManager.ShowEndlessStats();
    }

    void Update()
    {
        if (!gameOver)
        {
            _timeAlive += Time.deltaTime;

            if (_isEndless)
                uiManager.UpdateEndlessStats(candyFallSpeedBase, spawner.trickChance, _timeAlive, _candiesCaught);
        }
    }

    void SetupPowerupHUD()
    {
        _powerupHUD = FindFirstObjectByType<PowerupHUD>();
        if (_powerupHUD == null)
        {
            var hudGO = new GameObject("PowerupHUD_Runtime");
            _powerupHUD = hudGO.AddComponent<PowerupHUD>();
            Debug.Log("[GameManager] Created PowerupHUD at runtime");
        }
    }

    void ApplyLevelBackground(int level)
    {
        var tex = Resources.Load<Texture2D>("Backgrounds/Level" + level + "BG");
        if (tex == null) return;

        var bgObj = GameObject.Find("BG_sample_0");
        if (bgObj == null) return;

        var sr = bgObj.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        const float ppu = 100f;
        sr.sprite = Sprite.Create(tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), ppu);

        // Scale to cover the full camera viewport (no gaps)
        var cam = Camera.main;
        if (cam == null) return;

        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        float spriteW = tex.width / ppu;
        float spriteH = tex.height / ppu;

        float scale = Mathf.Max(camWidth / spriteW, camHeight / spriteH);
        bgObj.transform.localScale = new Vector3(scale, scale, 1f);
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

        // Apply Booster powerup multiplier
        if (_powerupHUD != null)
            value = Mathf.RoundToInt(value * _powerupHUD.ScoreMultiplier);

        score += value;
        _candiesCaught++;

        uiManager.UpdateScore(score);
        audioManager.PlayTreat();

        candyFallSpeedBase *= difficultyIncrease;
    }

    public void HandleTrickCollected(Collectible c)
    {
        // Shield powerup: ignore tricks
        if (_powerupHUD != null && _powerupHUD.IsShieldActive)
        {
            Debug.Log("[GameManager] Shield blocked a trick!");
            // Play a different sound or just skip damage
            return;
        }

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
        if (_isEndless) return;
        if (!gameOver && !_levelOver)
            StartCoroutine(WaitForScreenClear());
    }

    IEnumerator WaitForScreenClear()
    {
        // Poll every 0.25 s until the spawn container is empty
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
        if (_candiesCaught >= _currentConfig.targetFor3Stars) stars = 3;
        else if (_candiesCaught >= _currentConfig.targetFor2Stars) stars = 2;
        else if (_candiesCaught >= _currentConfig.targetFor1Star) stars = 1;

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

        // Try Heart auto-revive FIRST
        if (_powerupHUD != null && _powerupHUD.TryAutoReviveWithHeart())
        {
            // Heart consumed — restore lives and continue
            lives = _currentConfig.lives;
            uiManager.UpdateLives(lives);
            Debug.Log("[GameManager] Heart powerup auto-revived player!");
            return; // Don't trigger game over
        }

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
            ShowGameOverWithLeaderboard();
        }
    }

    /// <summary>
    /// Awards scaled gems for endless mode. Returns the amount awarded (0 if not endless).
    /// Called once at the true end of the run (not on intermediate deaths before revive).
    /// </summary>
    int AwardEndlessGems()
    {
        if (!_isEndless) return 0;

        int gems = Mathf.Max(5, 5 + _candiesCaught / 5 + Mathf.FloorToInt(_timeAlive / 30f));
        SaveManager.AddGems(gems);
        uiManager.UpdateGems(SaveManager.GetGems());
        Debug.Log("[GameManager] Endless gems awarded: " + gems
            + " (caught=" + _candiesCaught + ", time=" + _timeAlive.ToString("F1") + "s)");
        return gems;
    }

    /// <summary>
    /// Shows game-over screen. Awards endless gems first (once), then
    /// prompts for leaderboard name entry if the score qualifies.
    /// </summary>
    void ShowGameOverWithLeaderboard()
    {
        int gemsEarned = AwardEndlessGems();

        if (LeaderboardManager.IsHighScore(score))
        {
            Time.timeScale = 0f;
            int capturedGems = gemsEarned;
            LeaderboardNameEntry.Show(score, () =>
            {
                uiManager.ShowGameOver(score, capturedGems);
            });
        }
        else
        {
            uiManager.ShowGameOver(score, gemsEarned);
        }
    }

    public void ReviveFree() => Revive(true);
    public void RevivePaid() => Revive(false);

    public void Revive(bool free)
    {
        if (!free) SaveManager.SpendGems(100);

        _revivesUsed++;
        lives = _currentConfig.lives;
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
        ShowGameOverWithLeaderboard();
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
        SceneManager.LoadScene("Main Menu");
    }

    public void GoToLevelSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level Select");
    }

    public void GoToStore()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StoreScene");
    }
}

