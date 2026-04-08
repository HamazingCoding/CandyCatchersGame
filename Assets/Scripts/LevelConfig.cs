using UnityEngine;

/// <summary>
/// Holds all difficulty variables and star thresholds for a single level.
/// Passed from Level Select → GameManager via static storage.
/// </summary>
[System.Serializable]
public class LevelConfig
{
    public int levelNumber;
    public string levelName;

    [Header("Lives & Misses")]
    public int lives = 3;
    public int missedAllowed = 5;

    [Header("Fall Speed")]
    public float candyFallSpeedBase = 3f;
    public float difficultyIncrease = 1.01f;

    [Header("Spawner")]
    public float spawnInterval = 1.0f;
    public float minSpawnInterval = 0.4f;
    public float spawnAcceleration = 0.98f;
    public float trickChance = 0.15f;

    [Header("Level Length")]
    public int totalCandiesToSpawn = 30;

    [Header("Mode")]
    public bool isEndless = false;

    [Header("Star Thresholds (candies caught)")]
    public int targetFor1Star;
    public int targetFor2Stars;
    public int targetFor3Stars;

    // -------------------------------------------------------
    // Preset factory methods — one per level
    // -------------------------------------------------------

    public static LevelConfig Level1() => new LevelConfig
    {
        levelNumber         = 1,
        levelName           = "Level 1",
        lives               = 5,
        missedAllowed       = 5,
        candyFallSpeedBase  = 2.5f,
        difficultyIncrease  = 1.005f,
        spawnInterval       = 1.2f,
        minSpawnInterval    = 0.6f,
        spawnAcceleration   = 0.99f,
        trickChance         = 0.10f,
        totalCandiesToSpawn = 20,
        targetFor1Star      = 8,
        targetFor2Stars     = 14,
        targetFor3Stars     = 18
    };

    public static LevelConfig Level2() => new LevelConfig
    {
        levelNumber         = 2,
        levelName           = "Level 2",
        lives               = 5,
        missedAllowed       = 5,
        candyFallSpeedBase  = 2.8f,
        difficultyIncrease  = 1.006f,
        spawnInterval       = 1.1f,
        minSpawnInterval    = 0.55f,
        spawnAcceleration   = 0.99f,
        trickChance         = 0.12f,
        totalCandiesToSpawn = 25,
        targetFor1Star      = 10,
        targetFor2Stars     = 17,
        targetFor3Stars     = 22
    };

    public static LevelConfig Level3() => new LevelConfig
    {
        levelNumber         = 3,
        levelName           = "Level 3",
        lives               = 4,
        missedAllowed       = 4,
        candyFallSpeedBase  = 3.2f,
        difficultyIncrease  = 1.008f,
        spawnInterval       = 1.0f,
        minSpawnInterval    = 0.5f,
        spawnAcceleration   = 0.985f,
        trickChance         = 0.15f,
        totalCandiesToSpawn = 30,
        targetFor1Star      = 12,
        targetFor2Stars     = 20,
        targetFor3Stars     = 27
    };

    public static LevelConfig Level4() => new LevelConfig
    {
        levelNumber         = 4,
        levelName           = "Level 4",
        lives               = 4,
        missedAllowed       = 4,
        candyFallSpeedBase  = 3.6f,
        difficultyIncrease  = 1.009f,
        spawnInterval       = 0.95f,
        minSpawnInterval    = 0.45f,
        spawnAcceleration   = 0.984f,
        trickChance         = 0.18f,
        totalCandiesToSpawn = 35,
        targetFor1Star      = 14,
        targetFor2Stars     = 24,
        targetFor3Stars     = 31
    };

    public static LevelConfig Level5() => new LevelConfig
    {
        levelNumber         = 5,
        levelName           = "Level 5",
        lives               = 4,
        missedAllowed       = 4,
        candyFallSpeedBase  = 4.0f,
        difficultyIncrease  = 1.010f,
        spawnInterval       = 0.9f,
        minSpawnInterval    = 0.42f,
        spawnAcceleration   = 0.983f,
        trickChance         = 0.20f,
        totalCandiesToSpawn = 40,
        targetFor1Star      = 16,
        targetFor2Stars     = 28,
        targetFor3Stars     = 36
    };

    public static LevelConfig Level6() => new LevelConfig
    {
        levelNumber         = 6,
        levelName           = "Level 6",
        lives               = 4,
        missedAllowed       = 4,
        candyFallSpeedBase  = 4.5f,
        difficultyIncrease  = 1.011f,
        spawnInterval       = 0.85f,
        minSpawnInterval    = 0.38f,
        spawnAcceleration   = 0.982f,
        trickChance         = 0.23f,
        totalCandiesToSpawn = 45,
        targetFor1Star      = 18,
        targetFor2Stars     = 31,
        targetFor3Stars     = 40
    };

    public static LevelConfig Level7() => new LevelConfig
    {
        levelNumber         = 7,
        levelName           = "Level 7",
        lives               = 3,
        missedAllowed       = 3,  
        candyFallSpeedBase  = 5.0f,
        difficultyIncrease  = 1.012f,
        spawnInterval       = 0.8f,
        minSpawnInterval    = 0.34f,
        spawnAcceleration   = 0.98f,
        trickChance         = 0.25f,
        totalCandiesToSpawn = 50,
        targetFor1Star      = 20,
        targetFor2Stars     = 35,
        targetFor3Stars     = 45
    };

    public static LevelConfig Level8() => new LevelConfig
    {
        levelNumber         = 8,
        levelName           = "Level 8",
        lives               = 3,
        missedAllowed       = 3,
        candyFallSpeedBase  = 5.5f,
        difficultyIncrease  = 1.013f,
        spawnInterval       = 0.7f,
        minSpawnInterval    = 0.30f,
        spawnAcceleration   = 0.978f,
        trickChance         = 0.28f,
        totalCandiesToSpawn = 55,
        targetFor1Star      = 22,
        targetFor2Stars     = 38,
        targetFor3Stars     = 49
    };

    public static LevelConfig Level9() => new LevelConfig
    {
        levelNumber         = 9,
        levelName           = "Level 9",
        lives               = 3,
        missedAllowed       = 3,
        candyFallSpeedBase  = 6.0f,
        difficultyIncrease  = 1.015f,
        spawnInterval       = 0.6f,
        minSpawnInterval    = 0.27f,
        spawnAcceleration   = 0.976f,
        trickChance         = 0.30f,
        totalCandiesToSpawn = 60,
        targetFor1Star      = 25,
        targetFor2Stars     = 42,
        targetFor3Stars     = 54
    };

    public static LevelConfig Level10() => new LevelConfig
    {
        levelNumber         = 10,
        levelName           = "Level 10",
        lives               = 3,
        missedAllowed       = 3,
        candyFallSpeedBase  = 7.0f,
        difficultyIncrease  = 1.02f,
        spawnInterval       = 0.5f,
        minSpawnInterval    = 0.25f,
        spawnAcceleration   = 0.97f,
        trickChance         = 0.35f,
        totalCandiesToSpawn = 70,
        targetFor1Star      = 28,
        targetFor2Stars     = 49,
        targetFor3Stars     = 63
    };

    public static LevelConfig Endless() => new LevelConfig
    {
        levelNumber         = 0,
        levelName           = "Endless",
        isEndless           = true,
        lives               = 3,
        missedAllowed       = 99,
        candyFallSpeedBase  = 3.0f,
        difficultyIncrease  = 1.008f,
        spawnInterval       = 1.0f,
        minSpawnInterval    = 0.3f,
        spawnAcceleration   = 0.985f,
        trickChance         = 0.10f,
        totalCandiesToSpawn = 0,
        targetFor1Star      = 0,
        targetFor2Stars     = 0,
        targetFor3Stars     = 0
    };

    /// <summary>Returns the config for level n (1–10). Falls back to Level1 for out-of-range values.</summary>
    public static LevelConfig ForLevel(int n)
    {
        switch (n)
        {
            case 1:  return Level1();
            case 2:  return Level2();
            case 3:  return Level3();
            case 4:  return Level4();
            case 5:  return Level5();
            case 6:  return Level6();
            case 7:  return Level7();
            case 8:  return Level8();
            case 9:  return Level9();
            case 10: return Level10();
            default: return Level1();
        }
    }
}

/// <summary>
/// Static bridge — Level Select writes here, GameManager reads on Start.
/// </summary>
public static class LevelSelection
{
    public static LevelConfig Selected = LevelConfig.Level1(); // default fallback
}
