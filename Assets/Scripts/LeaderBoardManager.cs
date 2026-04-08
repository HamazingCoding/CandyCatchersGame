using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Static manager for local leaderboard persistence.
/// Stores top 10 scores with player names using PlayerPrefs (JSON serialized).
/// No MonoBehaviour — call from anywhere.
/// </summary>
public static class LeaderboardManager
{
    private const string LeaderboardKey = "Leaderboard_JSON";
    private const int MaxEntries = 10;

    // -------------------------------------------------------
    // Data Model
    // -------------------------------------------------------

    [System.Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public int score;
        public string date; // ISO date string for display

        public LeaderboardEntry(string name, int score)
        {
            this.playerName = name;
            this.score = score;
            this.date = System.DateTime.Now.ToString("yyyy-MM-dd");
        }
    }

    [System.Serializable]
    private class LeaderboardData
    {
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary>Returns the current top-10 list, sorted highest-first.</summary>
    public static List<LeaderboardEntry> GetEntries()
    {
        return LoadData().entries;
    }

    /// <summary>Returns true if the given score would qualify for the top 10.</summary>
    public static bool IsHighScore(int score)
    {
        if (score <= 0) return false;

        var data = LoadData();
        if (data.entries.Count < MaxEntries) return true;
        return score > data.entries[data.entries.Count - 1].score;
    }

    /// <summary>
    /// Adds a new entry if it qualifies. Returns the 1-based rank (or -1 if it didn't qualify).
    /// </summary>
    public static int AddEntry(string playerName, int score)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "Player";

        // Clamp name length
        if (playerName.Length > 15)
            playerName = playerName.Substring(0, 15);

        var data = LoadData();
        var entry = new LeaderboardEntry(playerName, score);

        data.entries.Add(entry);
        data.entries = data.entries
            .OrderByDescending(e => e.score)
            .ThenBy(e => e.date) // older dates first for tie-breaking
            .Take(MaxEntries)
            .ToList();

        SaveData(data);

        // Find the rank of the newly added entry
        int rank = data.entries.IndexOf(entry);
        return rank >= 0 ? rank + 1 : -1;
    }

    /// <summary>Clears all leaderboard data.</summary>
    public static void ClearAll()
    {
        PlayerPrefs.DeleteKey(LeaderboardKey);
        PlayerPrefs.Save();
    }

    /// <summary>Returns the number of entries currently stored.</summary>
    public static int EntryCount()
    {
        return LoadData().entries.Count;
    }

    /// <summary>
    /// Seeds the leaderboard with sample test data (8 entries).
    /// Only adds data if the leaderboard is currently empty.
    /// Returns true if data was seeded, false if leaderboard already had entries.
    /// </summary>
    public static bool SeedTestData()
    {
        var data = LoadData();
        if (data.entries.Count > 0)
        {
            Debug.Log("[LeaderboardManager] SeedTestData skipped — leaderboard already has " + data.entries.Count + " entries.");
            return false;
        }

        Debug.Log("[LeaderboardManager] Seeding leaderboard with test data...");

        // Add 8 sample high scores
        var testEntries = new (string name, int score)[]
        {
            ("Alice",    4200),
            ("Bob",      3750),
            ("Charlie",  3500),
            ("Diana",    3100),
            ("Player1",  2800),
            ("Eve",      2400),
            ("Frank",    1950),
            ("Grace",    1500),
        };

        foreach (var (name, score) in testEntries)
        {
            var entry = new LeaderboardEntry(name, score);
            data.entries.Add(entry);
        }

        // Sort and trim
        data.entries = data.entries
            .OrderByDescending(e => e.score)
            .ThenBy(e => e.date)
            .Take(MaxEntries)
            .ToList();

        SaveData(data);
        Debug.Log("[LeaderboardManager] Seeded " + data.entries.Count + " test entries successfully.");
        return true;
    }

    // -------------------------------------------------------
    // Persistence Helpers
    // -------------------------------------------------------

    private static LeaderboardData LoadData()
    {
        string json = PlayerPrefs.GetString(LeaderboardKey, "");
        if (string.IsNullOrEmpty(json))
            return new LeaderboardData();

        try
        {
            return JsonUtility.FromJson<LeaderboardData>(json);
        }
        catch
        {
            Debug.LogWarning("[LeaderboardManager] Corrupted data — resetting.");
            return new LeaderboardData();
        }
    }

    private static void SaveData(LeaderboardData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(LeaderboardKey, json);
        PlayerPrefs.Save();
    }
}

