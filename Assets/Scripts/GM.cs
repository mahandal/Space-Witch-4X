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

    
    [Header("Leaders")]
    // A dictionary mapping each faction to their leader.
    public Dictionary<Faction, Leader> leaders = new Dictionary<Faction, Leader>();

    // The hero leading the Swarm in this skirmish.
    public Leader swarmLeader;

    // The hero leading the Coven in this skirmish.
    public Leader covenLeader;

    // The hero leading the Syndicate in this skirmish.
    public Leader syndicateLeader;

    // The god worshipped by Neutral ships in this skirmish.
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
        if (swarmLeader != null) leaders[Faction.Swarm] = swarmLeader;
        if (covenLeader != null) leaders[Faction.Coven] = covenLeader;
        if (syndicateLeader != null) leaders[Faction.Syndicate] = syndicateLeader;
        if (neutralLeader != null) leaders[Faction.Neutral] = neutralLeader;
    }

    // End the current turn and go to the next one.
    public void EndTurn()
    {
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
        // Set new active faction.
        activeFaction = faction;

        // Get the faction's leader.
        Leader leader = leaders[faction];

        // Let the leader start their faction's turn.
        leader.StartTurn();

        // Set up UI.
        UI.I.NewTurn(faction);
    }
}
