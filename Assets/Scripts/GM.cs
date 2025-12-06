using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class GM : MonoBehaviour
{
    [Header("GM")]
    // Track the current state of the game.
    public int gameState = 0;

    // How much time each AI gets to think.
    public float aiTurnTime = 1f;

    [Header("Turns")]
    // Which faction's turn is it?
    public Faction activeFaction = Faction.Pack;

    // Which faction is the player currently playing as?
    public Faction playerFaction = Faction.Coven;

    // What round are we on?
    // A round is complete when each faction takes their turn.
    public int round = 1;

    // The order factions take turns in.
    public Faction[] turnOrder = new Faction[] {};

    // Index in the turn order array.
    private int turnIndex = 0;

    
    [Header("Leaders")]
    // A dictionary mapping each faction to their leader.
    public Dictionary<Faction, Leader> leaders = new Dictionary<Faction, Leader>();

    // The hero leading the Pack in this skirmish.
    public Leader packLeader;

    // The hero leading the Coven in this skirmish.
    public Leader covenLeader;

    // The hero leading the Syndicate in this skirmish.
    public Leader syndicateLeader;

    // The hero leading Neutral ships in this skirmish.
    public Leader neutralLeader;

    [Header("Grid")]
    // The grid of all tiles for our current skirmish.
    public Tile[,] grid;

    // How many tiles wide our grid is.
    public int gridWidth = 15;

    // How many tiles high our grid is.
    public int gridHeight = 10;

    // The currently selected tile.
    public Tile selectedTile;

    // The tile we are currently hovering over.
    // (set in InputManager)
    public Tile hoveredTile;


    [Header("Machinery")]
    // Singleton
    public static GM I;

    // Awaken!
    void Awake()
    {
        // Enforce singleton pattern.
        if (I == null)
            I = this;
        else
            Destroy(this);

        // Initialize dictionary of leaders.
        if (packLeader != null) leaders[Faction.Pack] = packLeader;
        if (covenLeader != null) leaders[Faction.Coven] = covenLeader;
        if (syndicateLeader != null) leaders[Faction.Syndicate] = syndicateLeader;
        if (neutralLeader != null) leaders[Faction.Neutral] = neutralLeader;
    }

    // Called when the player clicks the end turn button.
    // Error checks then delegates to EndTurn().
    public void Button_EndTurn()
    {
        // Hide end turn button.
        UI.I.endTurnButton.gameObject.SetActive(false);

        // End the current turn.
        EndTurn();
    }

    // End the current turn and go to the next one.
    // Delegates to a coroutine so we can let AI have some time to think.
    public void EndTurn()
    {
        // Stop once the game is over.
        if (gameState > 1) return;

        // Start the coroutine!
        StartCoroutine(EndTurnCoroutine());
    }

    // Handle ending a turn.
    // Note: Waits a second first to give AI time to think!
    private IEnumerator EndTurnCoroutine()
    {
        // Fade out overlay.
        UI.I.EndTurn(activeFaction);
        
        // Wait for fade to complete.
        // (also give each AI a second for their turn!)
        yield return new WaitForSeconds(aiTurnTime);
        
        // Let the current leader end the turn for their faction.
        leaders[activeFaction].EndTurn();

        // Increment turn index.
        turnIndex++;

        // Check if each faction has had a turn.
        if (turnIndex >= turnOrder.Length)
        {
            // Reset to beginning.
            turnIndex = 0;

            // Start a new round!
            round++;
        }

        // Get the next faction.
        Faction nextFaction = turnOrder[turnIndex];

        // Get the leader of the next faction.
        Leader nextLeader = leaders[nextFaction];

        // If the next faction's leader is gone, skip them!
        if (nextLeader == null ||
            nextLeader.currentHealth <= 0 ||
            nextLeader.faction != nextFaction)
        {
            EndTurn();
        } else {
            // Start a new turn for the next faction.
            NewTurn(nextFaction);   
        }
    }

    // Start a new turn for the given faction.
    public void NewTurn(Faction faction)
    {
        // Fade in if it is the player's turn.
        if (faction == playerFaction)
            UI.I.FadeOverlay(false, 1f);

        // Set new active faction.
        activeFaction = faction;

        // Get the faction's leader.
        Leader leader = leaders[faction];

        // Let the leader start their faction's turn.
        leader.StartTurn();

        // Set up UI.
        UI.I.NewTurn(faction);

        // Set up fog of war?
        UpdateFogOfWar();

        // AI?
        if (activeFaction != playerFaction)
            leader.AITurn();
    }

    // Return the tile in our grid located at position (x, y)
    // Returns null if position is out of bounds.
    public Tile GetTile(int x, int y)
    {
        // Respect boundaries
        if (x < 0) return null;
        if (y < 0) return null;
        if (x >= gridWidth) return null;
        if (y >= gridHeight) return null;

        // Return!
        return grid[x, y];
    }

    // Update fog of war based on player's units' vision
    public void UpdateFogOfWar()
    {
        // First, reveal your territory.
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                // Get tile.
                Tile tile = grid[i, j];

                // Check if tile is in your territory.
                if (tile.faction == playerFaction)
                {
                    tile.RevealFromFog();
                } else {
                    tile.HideInFog();
                }
            }
        }

        // Get player's leader
        Leader playerLeader = leaders[playerFaction];
        if (playerLeader == null) return;

        // Reveal tiles your ships can see.
        foreach (Ship ship in playerLeader.fleet)
        {
            // Skip dead ships
            if (ship == null || ship.currentHealth <= 0) continue;

            // Get all visible tiles from this ship
            HashSet<Tile> visibleTiles = ship.currentTile.GetTilesInVisionRange();
            
            foreach (Tile tile in visibleTiles)
            {
                tile.RevealFromFog();
            }
        }
    }

    // - Game Over

    // Victory!
    public void Victory()
    {
        // UI
        UI.I.Victory();

        // Shared game over.
        GameOver();
    }

    // Defeat :(
    public void Defeat()
    {
        // UI
        UI.I.Defeat();

        // Shared game over.
        GameOver();
    }

    // Shared game over.
    public void GameOver()
    {
        // Set game state.
        gameState = 2;
    }

    // Check if we won!
    // Note: Also begins the celebration!
    public bool CheckVictory()
    {
        // Init to true.
        bool weJustWon = true;

        // Check if we're the last leader standing
        foreach (Leader leader in leaders.Values)
        {
            // Can't claim victory until all other leaders are gone or on our side.
            if (leader != null && leader.faction != playerFaction)
                weJustWon = false;
        }

        // If we have no opposition, claim victory!
        if (weJustWon)
            Victory();

        // Return!
        return weJustWon;
    }

    // Check if we just lost!
    public bool CheckDefeat()
    {
        // Try and get our leader.
        Leader leader = leaders[playerFaction];

        // Leader is dead!
        if (leader == null || leader.currentHealth <= 0)
        {
            // Start mourning.
            Defeat();

            // Return!
            return true;
        }

        // Otherwise, we're still in it!
        return false;
    }
}
