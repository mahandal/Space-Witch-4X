using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Class for handling the leaderboard,
// the ranking of each leader's current score in the top left of the game.
public class Leaderboard : MonoBehaviour
{
    [Header("Leaderboard")]
    public List<LeaderboardEntry> leaderboardEntries;

    // Singleton
    public static Leaderboard I;

    // Awaken!
    void Awake()
    {
        // Enforce singleton pattern.
        if (I == null)
            I = this;
        else
            Destroy(this);
    }

    // Update the leaderboard with the current scores of each faction.
    public void UpdateLeaderboard()
    {
        // Sort factions.
        List<Faction> sortedFactions = GM.I.leaders.Keys.OrderByDescending(faction => GetScore(faction)).ToList();

        // Call leaderboardEntries[x].UpdateEntry() appropriately for each faction.
        for (int i = 0; i < leaderboardEntries.Count; i++)
        {
            leaderboardEntries[i].UpdateEntry(sortedFactions[i]);
        }
    }

    // Get the score for the given faction.
    // For now at least,
    // score is equal to the number of tiles you own.
    public static int GetScore(Faction faction)
    {
        // Initialize to 0.
        int score = 0;

        // Loop through each tile.
        foreach (Tile tile in Tile.GetTiles())
        {
            // Check faction.
            if (tile.faction == faction)
            {
                // Increment score.
                score++;
            }
        }

        // Return!
        return score;
    }
}
