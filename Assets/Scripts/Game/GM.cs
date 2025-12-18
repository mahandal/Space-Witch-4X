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

    // Remember the last ship we selected.
    public Ship lastSelectedShip;

    // Remember which ship we are trying to build.
    // Empty ("") unless we just clicked a Build Ship button
    public string currentlyBuilding = "";

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

    // Set up what we need for a battle.
    // Called once at the beginning of each battle (from the button pressed in GGG).
    public void BeginBattle()
    {
        // - Prepare

        // Set up new map.
        int width = Random.Range(10, 50);
        int height = Random.Range(10, 30);
        SpawnManager.I.GenerateNewMap(width, height);

        // Clear tile selection to begin with (?)
        Tile.ClearSelection();

        // Initialize UI.
        UI.I.SetUp();

        // Set game state.
        gameState = 1;

        // - Begin game!

        // Begin the game by starting a new turn for the pack!
        StartCoroutine(NewTurn(Faction.Pack));
    }

    // - Buttons

    public void Button_AutoPilot(string autoPilotMode)
    {
        // Null check.
        if (selectedTile == null || selectedTile.ship == null) return;

        AutoPilotMode apm = Constance.autoPilotModes[autoPilotMode];

        // Set the auto pilot mode for the currently selected ship.
        selectedTile.ship.SetAutoPilot(apm);
    }

    // Recycle your currently selected ship into mana.
    public void Button_RecycleShip()
    {
        // Null check.
        if (selectedTile == null || selectedTile.ship == null) return;

        // Recycle the currently selected ship.
        RecycleShip(selectedTile.ship);
    }

    // Called when the player clicks one of their ship blueprints in the top bar.
    public void Button_BuildShip(string shipName)
    {
        // Clear prior unit selection.
        Tile.ClearSelection();

        // Get mana cost.
        int manaCost = GetManaCost(shipName);

        // Get player's leader.
        Leader leader = leaders[playerFaction];

        // Check if player has enough mana to build this ship.
        if (leader.mana < manaCost)
        {
            // Return.
            return;
        }

        // Set currentlyBuilding
        currentlyBuilding = shipName;
    }

    // Called when the player clicks the end turn button.
    // Error checks then delegates to EndTurn().
    public void Button_EndTurn()
    {
        // Hide end turn button.
        UI.I.endTurnButton.gameObject.SetActive(false);

        // Clear your current selection
        Tile.ClearSelection();

        // End the current turn.
        StartCoroutine(EndTurn());
    }

    // - Functions

    // Recycle a ship.
    // Returns up to 90% of a ship's mana cost to its faction.
    // - Scales linearly with ship's health percentage.
    // - Scales linearly with # of friendly adjacent tiles.
    public void RecycleShip(Ship ship)
    {
        // Null check.
        if (ship == null) return;

        // Get mana returned.
        int manaReturned = ship.GetRecycleValue();

        // Get faction leader.
        Leader leader = leaders[ship.faction];

        // Gain mana.
        leader.GainMana(manaReturned);

        // Clean up ship.
        ship.Death(ship);
    }

    // Get the mana cost of a ship from its name.
    public int GetManaCost(string shipName)
    {
        // Get the progenitor ship.
        Ship progenitor = SpawnManager.I.GetProgenitor(shipName);

        // Check if ship name was invalid.
        if (progenitor == null)
        {
            // Return -1 for invalid ships.
            return -1;
        } else {
            // Return progenitor's mana cost.
            return progenitor.manaCost;
        }
    }

    // Build the given ship at the given tile.
    // Note: Automatically detects faction from ship name.
    public Ship BuildShip(string shipName, Tile home)
    {
        // Instantiate the new ship.
        Ship newShip = SpawnManager.I.SpawnShip(shipName, home.x, home.y);

        // Failed to build?
        if (newShip == null)
        {
            Debug.LogError("Failed to build ship of type: " + shipName +
                " at tile: " + home.myType + " (" + home.x + ", " + home.y + ")");
            return null;
        }

        // Update action indicators.
        newShip.SetMovementRemaining(newShip.movementRemaining);
        newShip.SetAttacksRemaining(newShip.attacksRemaining);

        // Get faction leader.
        Leader leader = leaders[newShip.faction];

        // Spend mana.
        leader.SpendMana(newShip.manaCost);

        // Reset build order.
        ResetBuildOrder();

        // Return the new ship.
        return newShip;
    }

    // Stop building.
    public void ResetBuildOrder()
    {
        currentlyBuilding = "";

        BuildButton.ClearAllBuildHighlights();
    }

    // End the current turn and go to the next one.
    // Delegates to a coroutine so we can let AI have some time to think.
    // public void EndTurn()
    // {
    //     // Stop once the game is over.
    //     if (gameState > 1) return;

    //     // Start the coroutine!
    //     StartCoroutine(EndTurnCoroutine());
    // }

    // Handle ending the player's turn.

    // Handle ending a turn.
    // Note: Waits a second first to give AI time to think!
    public IEnumerator EndTurn()
    {
        // Stop once the game is over.
        if (gameState > 1) yield break;

        // Get the current leader.
        Leader currentLeader = leaders[activeFaction];

        // Prevent ending your turn more than once per turn.
        if (currentLeader.hasEndedTurn) yield break;
        currentLeader.hasEndedTurn = true;

        // Handle auto-pilots.
        // foreach (Ship ship in currentLeader.fleet)
        foreach (Ship ship in currentLeader.GetAvailableShips())
        {
            yield return ship.AutoPilot();
        }
        
        // Let the current leader end the turn for their faction.
        // yield return currentLeader.EndTurn();

        // Refresh the current leader's fleet so you can click on them and preview their movement.
        currentLeader.RefreshFleet();

        // Hide movement and attacks remaining for all ships in the fleet.
        currentLeader.HideFleetMovementAndAttacks();

        // Increment turn index.
        IncrementTurnIndex();

        // Get the next faction.
        Faction nextFaction = turnOrder[turnIndex];

        // Get the leader of the next faction.
        Leader nextLeader = leaders[nextFaction];

        // Check if the next faction is still active.
        while (nextLeader == null ||
            nextLeader.currentHealth <= 0 ||
            nextLeader.faction != nextFaction)
        {
            // Increment the turn index.
            IncrementTurnIndex();

            // Get the next faction.
            nextFaction = turnOrder[turnIndex];

            // Get the leader of the next faction.
            nextLeader = leaders[nextFaction];
        }

        // Start a new turn for the next faction.
        yield return NewTurn(nextFaction);
    }

    // Increment the turn index.
    // Starts a new round after each faction has had a turn.
    public void IncrementTurnIndex()
    {
        // Increment the turn index.
        turnIndex++;

        // Check if each faction has had a turn.
        if (turnIndex >= turnOrder.Length)
        {
            // Reset to beginning.
            turnIndex = 0;

            // Start a new round!
            round++;
        }
    }

    // Start a new turn for the given faction.
    public IEnumerator NewTurn(Faction faction)
    {
        // Set new active faction.
        activeFaction = faction;

        // Get the faction's leader.
        Leader leader = leaders[faction];

        // Let the leader start their faction's turn.
        yield return leader.StartTurn();

        // Set up UI.
        UI.I.NewTurn(faction);

        // Set up fog of war?
        UpdateFogOfWar();

        // AI?
        if (activeFaction != playerFaction)
            yield return leader.AITurn();
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

    // Select a random available ship in your fleet.
    // Note: Ignores ships on auto pilot.
    public void Button_SelectNextShip()
    {
        // Allow clicking the next button as a backup to re-check if the End Turn button should be revealed.
        UI.I.WhichButtonInTopRight();

        // Get player's leader
        Leader playerLeader = leaders[playerFaction];

        playerLeader.GetRandomAvailableShip(true);
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
            if (leader.currentHealth > 0 && leader.faction != playerFaction)
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
