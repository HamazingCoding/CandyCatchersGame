using UnityEngine;

/// <summary>
/// Static manager for powerup inventory. Persists via PlayerPrefs.
/// Powerup types: Booster, Lightning, Heart, Shield
/// </summary>
public static class PowerupManager
{
    public enum PowerupType
    {
        Booster,    // 2x score multiplier
        Lightning,  // Speed boost for 15s
        Heart,      // Extra life on death
        Shield      // Immunity to tricks for 10s
    }

    // Store prices
    public static int GetPrice(PowerupType type)
    {
        switch (type)
        {
            case PowerupType.Booster: return 50;
            case PowerupType.Lightning: return 75;
            case PowerupType.Heart: return 100;
            case PowerupType.Shield: return 55;
            default: return 999;
        }
    }

    public static string GetDisplayName(PowerupType type)
    {
        switch (type)
        {
            case PowerupType.Booster: return "Booster Pack";
            case PowerupType.Lightning: return "Lightning";
            case PowerupType.Heart: return "Shiny Heart";
            case PowerupType.Shield: return "Shield";
            default: return "Unknown";
        }
    }

    public static string GetDescription(PowerupType type)
    {
        switch (type)
        {
            case PowerupType.Booster: return "Double all points scored for 20 seconds";
            case PowerupType.Lightning: return "2x catcher speed for 15 seconds";
            case PowerupType.Heart: return "Auto-revive with full lives on death";
            case PowerupType.Shield: return "Immune to tricks for 10 seconds";
            default: return "";
        }
    }

    public static string GetImageName(PowerupType type)
    {
        switch (type)
        {
            case PowerupType.Booster: return "Booster";
            case PowerupType.Lightning: return "Lightning";
            case PowerupType.Heart: return "Heart";
            case PowerupType.Shield: return "Shield";
            default: return "";
        }
    }

    // -------------------------------------------------------
    // Inventory
    // -------------------------------------------------------

    private static string Key(PowerupType type) => "Powerup_" + type.ToString();

    public static int GetQuantity(PowerupType type)
    {
        return PlayerPrefs.GetInt(Key(type), 0);
    }

    public static void AddQuantity(PowerupType type, int amount = 1)
    {
        int current = GetQuantity(type);
        PlayerPrefs.SetInt(Key(type), current + amount);
        PlayerPrefs.Save();
    }

    public static bool UseOne(PowerupType type)
    {
        int current = GetQuantity(type);
        if (current <= 0) return false;
        PlayerPrefs.SetInt(Key(type), current - 1);
        PlayerPrefs.Save();
        return true;
    }

    public static bool HasAny(PowerupType type)
    {
        return GetQuantity(type) > 0;
    }

    /// <summary>
    /// Attempt to purchase a powerup. Returns true if successful.
    /// </summary>
    public static bool Purchase(PowerupType type)
    {
        int price = GetPrice(type);
        if (!SaveManager.CanAfford(price)) return false;

        SaveManager.SpendGems(price);
        AddQuantity(type);
        return true;
    }
}
