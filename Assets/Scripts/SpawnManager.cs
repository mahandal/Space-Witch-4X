using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("Parents")]
    // A parent object of all tiles for this map.
    public Transform tileParent;

    // A parent object for all Swarm ships on this map.
    public Transform swarmShipParent;

    // A parent object for all Coven ships on this map.
    public Transform covenShipParent;

    // A parent object for all Syndicate ships on this map.
    public Transform syndicateShipParent;

    // A parent object for all Neutral ships on this map.
    public Transform neutralShipParent;

    [Header("Progenitors - Tiles")]
    public Tile p_Air;
    public Tile p_Planet;
    public Tile p_Water;
    public Tile p_Fire;
    public Tile p_Asteroids;

    [Header("Progenitors - Swarm Ships")]
    public Ship p_Tarodactyl;

    [Header("Progenitors - Coven Ships")]
    public Ship p_SpaceWitch;

    [Header("Progenitors - Syndicate Ships")]
    public Ship p_Flybot;

    void Awake()
    {
        // - Make sure progenitors are hidden!

        // Tiles
        p_Air.gameObject.SetActive(false);
        p_Planet.gameObject.SetActive(false);
        p_Water.gameObject.SetActive(false);
        p_Fire.gameObject.SetActive(false);
        p_Asteroids.gameObject.SetActive(false);

        // Ships
        p_SpaceWitch.gameObject.SetActive(false);
        p_Tarodactyl.gameObject.SetActive(false);
        p_Flybot.gameObject.SetActive(false);
    }

    // Start your engines!
    void Start()
    {
        // Spawn a new map 15 tiles wide and 10 tiles tall.
        GenerateNewMap(15, 10);
    }

    // Procedurally generate a new map.
    public void GenerateNewMap(int width, int height)
    {
        // Create new grid.
        GM.I.grid = new Tile[width, height];

        // Remember grid dimensions.
        GM.I.gridWidth = width;
        GM.I.gridHeight = height;

        // Loop through each column.
        for (int x = 0; x < width; x++)
        {
            // Loop through each row.
            for (int y = 0; y < height; y++)
            {
                // Create new tile
                Tile newTile = SpawnRandomTile(x, y);
            }
        }

        // Clear highlighting!
        Tile.ClearAllHighlights();


        // - Spawn in heroes
        foreach (Leader leader in GM.I.leaders.Values)
        {
            leader.Init();
        }

        // - Begin the game by starting a new turn for the swarm!
        GM.I.NewTurn(Faction.Swarm);
    }

    // Spawn a random tile at the given coordinates.
    public Tile SpawnRandomTile(int x, int y)
    {
        // New tile to be created...
        Tile newTile;

        // Roll to decide which tile we spawn.
        float roll = Random.Range(0f, 100f);

        // 50% - Air
        if (roll < 50)
        {
            newTile = Object.Instantiate(p_Air, tileParent);
        }
        // 20% - Water
        else if (roll < 70)
        {
            newTile = Object.Instantiate(p_Water, tileParent);
        }
        // 15% - Asteroids
        else if (roll < 85)
        {
            newTile = Object.Instantiate(p_Asteroids, tileParent);
        }
        // 10% - Fire
        else if (roll < 95)
        {
            newTile = Object.Instantiate(p_Fire, tileParent);
        }
        // 5% - Planet
        else
        {
            newTile = Object.Instantiate(p_Planet, tileParent);
        }

        // Assign in grid.
        GM.I.grid[x, y] = newTile;

        // Remember coordinates.
        newTile.x = x;
        newTile.y = y;

        // Place in space.
        newTile.transform.position = Utility.GridToWorld(x, y);

        // Activate!
        newTile.gameObject.SetActive(true);

        // Return!
        return newTile;
    }

    // Spawn a new ship.
    public Ship SpawnShip(string shipType, int x, int y)
    {
        // Respect boundaries.
        if (x < 0) return null;
        if (y < 0) return null;
        if (x >= GM.I.gridWidth) return null;
        if (y >= GM.I.gridHeight) return null;

        // Avoid spawning a ship in a tile with a ship already in it.
        Tile tile = GM.I.grid[x, y];
        if (tile.ship != null) return null;

        // Init new ship.
        Ship newShip = null;

        // Check ship type to instantiate new ship.
        if (shipType == "Space Witch")
        {
            newShip = Object.Instantiate(p_SpaceWitch);
        }
        else if (shipType == "Tarodactyl")
        {
            newShip = Object.Instantiate(p_Tarodactyl);
        }
        else if (shipType == "Flybot")
        {
            newShip = Object.Instantiate(p_Flybot);
        }
        else
        {
            Debug.LogError("ERROR! Failed to spawn new ship of unknown type: " + shipType);
            return null;
        }

        Debug.Log("Spawning " + newShip.myName + " for faction: " + newShip.faction);
        
        // Add to leader's fleet.
        GM.I.leaders[newShip.faction].fleet.Add(newShip);

        // - Assign parent.
        // (Doesn't really do anything, just for organizational purposes)

        // Neutral
        if (newShip.faction == Faction.Neutral)
            newShip.transform.SetParent(neutralShipParent);

        // Swarm
        else if (newShip.faction == Faction.Swarm)
            newShip.transform.SetParent(swarmShipParent);

        // Coven
        else if (newShip.faction == Faction.Coven)
            newShip.transform.SetParent(covenShipParent);

        // Syndicate
        else if (newShip.faction == Faction.Syndicate)
            newShip.transform.SetParent(syndicateShipParent);

        // Set new position.
        newShip.Move(x, y);

        // Activate!
        newShip.gameObject.SetActive(true);

        // Return!
        return newShip;
    }
}
