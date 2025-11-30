using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GM : MonoBehaviour
{
    [Header("GM")]
    // Track the current state of the game.
    public int gameState = 0;

    [Header("Turns")]
    // Which faction's turn is it?
    public Faction activeFaction = Faction.Swarm;

    // What round are we on?
    // A round is complete when each faction takes their turn.
    public int round = 1;

    // The order factions take turns in.
    // TBD: Add neutrals!
    public Faction[] turnOrder = new Faction[] { Faction.Swarm, Faction.Coven, Faction.Syndicate };

    // Index in the turn order array.
    private int turnIndex = 0;

    
    [Header("Heroes")]
    // The hero leading the Swarm in this adventure.
    public Ship swarmHero;

    // The hero leading the Coven in this adventure.
    public Ship covenHero;

    // The hero leading the Syndicate in this adventure.
    public Ship syndicateHero;

    [Header("Ships")]
    public List<Ship> swarmShips = new List<Ship>();
    public List<Ship> covenShips = new List<Ship>();
    public List<Ship> syndicateShips = new List<Ship>();
    public List<Ship> neutralShips = new List<Ship>();

    [Header("Grid")]
    // The grid of all tiles for our current adventure.
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

        // - Initialize ship lists.

        // Swarm
        swarmShips = new List<Ship>();
        swarmShips.Add(swarmHero);

        // Coven
        covenShips = new List<Ship>();
        covenShips.Add(covenHero);

        // Syndicate
        syndicateShips = new List<Ship>();
        syndicateShips.Add(syndicateHero);

        // Neutral
        neutralShips = new List<Ship>();
    }

    // End the current turn and go to the next one.
    public void EndTurn()
    {
        Debug.Log("Ending the turn for faction: " + activeFaction);

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

        // Start a new turn for the next faction.
        Faction nextFaction = turnOrder[turnIndex];
        NewTurn(nextFaction);        
    }

    // Start a new turn for the given faction.
    private void NewTurn(Faction faction)
    {
        Debug.Log("Starting a new turn for faction: " + faction);
        
        // Set new active faction.
        activeFaction = faction;

        // Display new faction.
        UI.I.activeFaction.text = faction.ToString();

        // Peform upkeep for each of that faction's ships & tiles.
        Upkeep(faction);
    }

    // Handle upkeep for each of a faction's ships & tiles:
    // - Refresh each ship's movement and attacks.
    // - TBD: Gain 1 mana per tile.
    private void Upkeep(Faction faction)
    {
        Debug.Log("Performing upkeep for faction: " + faction);
        
        // Initialize a list of all ships for this faction.
        List<Ship> ships = null;

        // Assign list
        if (faction == Faction.Swarm) ships = swarmShips;
        if (faction == Faction.Coven) ships = covenShips;
        if (faction == Faction.Syndicate) ships = syndicateShips;
        if (faction == Faction.Neutral) ships = neutralShips;

        // Iterate through each ship.
        foreach (Ship ship in ships)
        {
            // Refresh ship.
            ship.Refresh();
        }
    }
}
