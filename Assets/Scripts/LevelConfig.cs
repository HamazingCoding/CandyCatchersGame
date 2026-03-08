using UnityEngine;

/// <summary>
/// Holds all difficulty variables for a single level.
/// Passed from Level Select → GameManager via static storage.
/// </summary>
[System.Serializable]
public class LevelConfig
{
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

    // -------------------------------------------------------
    // Preset factory methods — one per level
    // -------------------------------------------------------

    public static LevelConfig Level1() => new LevelConfig
    {
        levelName       = "Level 1 - Easy",
        lives           = 3,
        missedAllowed   = 5,
        candyFallSpeedBase  = 2.5f,
        difficultyIncrease  = 1.005f,
        spawnInterval       = 1.2f,
        minSpawnInterval    = 0.6f,
        spawnAcceleration   = 0.99f,
        trickChance         = 0.10f
    };

    public static LevelConfig Level2() => new LevelConfig
    {
        levelName       = "Level 2 - Medium",
        lives           = 3,
        missedAllowed   = 4,
        candyFallSpeedBase  = 3.5f,
        difficultyIncrease  = 1.01f,
        spawnInterval       = 1.0f,
        minSpawnInterval    = 0.4f,
        spawnAcceleration   = 0.98f,
        trickChance         = 0.20f
    };

    public static LevelConfig Level3() => new LevelConfig
    {
        levelName       = "Level 3 - Hard",
        lives           = 2,
        missedAllowed   = 3,
        candyFallSpeedBase  = 5f,
        difficultyIncrease  = 1.02f,
        spawnInterval       = 0.8f,
        minSpawnInterval    = 0.25f,
        spawnAcceleration   = 0.97f,
        trickChance         = 0.30f
    };
}

/// <summary>
/// Static bridge — Level Select writes here, GameManager reads on Start.
/// </summary>
public static class LevelSelection
{
    public static LevelConfig Selected = LevelConfig.Level1(); // default fallback
}
