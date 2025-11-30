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
    // A parent object of all tiles and ships for this map.
    public Transform universe;

    // A parent object of all tiles for this map.
    public Transform tileParent;

    // A parent object for all ships for this map.
    public Transform shipParent;

    // Singleton
    public static GM I;

    void Awake()
    {
        // Enforce singleton pattern.
        if (I == null)
            I = this;
        else
            Destroy(this);
    }

    // End the current turn and go to the next one.
    public void EndTurn()
    {
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

        // Set new active faction.
        activeFaction = turnOrder[turnIndex];

        // Display new faction.
        UI.I.activeFaction.text = activeFaction.ToString();
    }
}
