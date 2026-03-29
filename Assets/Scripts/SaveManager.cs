using UnityEngine;

/// <summary>
/// Static helper for persisting player progress via PlayerPrefs.
/// No MonoBehaviour — call from anywhere.
/// Keys: "TotalGems", "Level_N_Stars", "Level_N_Unlocked"
/// </summary>
public static class SaveManager
{
    private const string GemsKey = "TotalGems";

    // -------------------------------------------------------
    // Gems
    // -------------------------------------------------------

    public static int GetGems()
    {
        return PlayerPrefs.GetInt(GemsKey, 0);
    }

    public static void AddGems(int amount)
    {
        int current = GetGems();
        PlayerPrefs.SetInt(GemsKey, current + amount);
        PlayerPrefs.Save();
    }

    public static bool CanAfford(int cost)
    {
        return GetGems() >= cost;
    }

    public static void SpendGems(int cost)
    {
        int current = GetGems();
        int newTotal = Mathf.Max(0, current - cost);
        PlayerPrefs.SetInt(GemsKey, newTotal);
        PlayerPrefs.Save();
    }

    // -------------------------------------------------------
    // Stars — only saves if the new value is higher
    // -------------------------------------------------------

    public static int GetStars(int levelNumber)
    {
        return PlayerPrefs.GetInt(StarKey(levelNumber), 0);
    }

    public static void SaveStars(int levelNumber, int stars)
    {
        int existing = GetStars(levelNumber);
        if (stars > existing)
        {
            PlayerPrefs.SetInt(StarKey(levelNumber), stars);
            PlayerPrefs.Save();
        }
    }

    // -------------------------------------------------------
    // Level unlocking — Level 1 is always unlocked
    // -------------------------------------------------------

    public static bool IsLevelUnlocked(int levelNumber)
    {
        if (levelNumber <= 1) return true;
        return PlayerPrefs.GetInt(UnlockKey(levelNumber), 0) == 1;
    }

    public static void UnlockLevel(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > 10) return;
        PlayerPrefs.SetInt(UnlockKey(levelNumber), 1);
        PlayerPrefs.Save();
    }

    // -------------------------------------------------------
    // Private key helpers
    // -------------------------------------------------------

    private static string StarKey(int levelNumber)   => "Level_" + levelNumber + "_Stars";
    private static string UnlockKey(int levelNumber) => "Level_" + levelNumber + "_Unlocked";
}
