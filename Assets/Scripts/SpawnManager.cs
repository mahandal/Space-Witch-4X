using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("Parents")]
    // A parent object of all tiles for this map.
    public Transform tileParent;

    // A parent object for all Pack ships on this map.
    public Transform packShipParent;

    // A parent object for all Coven ships on this map.
    public Transform covenShipParent;

    // A parent object for all Syndicate ships on this map.
    public Transform syndicateShipParent;

    // A parent object for all Neutral ships on this map.
    public Transform neutralShipParent;

    [Header("Progenitors - Tiles")]
    public Tile p_Void;
    public Tile p_Air;
    public Tile p_Planet;
    public Tile p_Water;
    public Tile p_Fire;
    public Tile p_Asteroids;

    [Header("Progenitors - Pack Ships")]
    public Ship p_Tarodactyl;

    [Header("Progenitors - Coven Ships")]
    public Ship p_SpaceWitch;
    public Ship p_Fairy;
    public Ship p_Treant;
    public Ship p_Squirrel;

    [Header("Progenitors - Syndicate Ships")]
    public Ship p_Flybot;

    [Header("Progenitors - Neutral Ships")]
    public Ship p_SkyPirate;

    public static SpawnManager I;

    void Awake()
    {
        // Enforce singleton pattern.
        if (I == null)
            I = this;
        else
            Destroy(this);

        // - Make sure progenitors are hidden!

        // Tiles
        p_Void.gameObject.SetActive(false);
        p_Air.gameObject.SetActive(false);
        p_Planet.gameObject.SetActive(false);
        p_Water.gameObject.SetActive(false);
        p_Fire.gameObject.SetActive(false);
        p_Asteroids.gameObject.SetActive(false);

        // - Ships

        // Pack
        p_Tarodactyl.gameObject.SetActive(false);

        // Coven
        p_SpaceWitch.gameObject.SetActive(false);
        p_Fairy.gameObject.SetActive(false);
        p_Treant.gameObject.SetActive(false);
        p_Squirrel.gameObject.SetActive(false);

        // Syndicate
        p_Flybot.gameObject.SetActive(false);

        // Neutral
        p_SkyPirate.gameObject.SetActive(false);
    }

    // Start your engines!
    void Start()
    {
        // Spawn a new map!
        int width = Random.Range(10, 30);
        int height = Random.Range(5, 20);
        GenerateNewMap(width, height);
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
        // Tile.ClearAllHighlights();


        // - Spawn in heroes
        foreach (Leader leader in GM.I.leaders.Values)
        {
            // Initialize each leader.
            // Note: Also moves them into a random position.
            leader.Init();

            // Terraform spawn position to be a planet.
            leader.currentTile.Terraform(TileType.Planet);
        }

        foreach (Leader leader in GM.I.leaders.Values)
        {
            // Clear a path from each leader to each other.
            foreach (Leader otherLeader in GM.I.leaders.Values)
            {
                TerraformPath(leader, otherLeader);
            }
        }

        // - Begin the game by starting a new turn for the pack!
        GM.I.NewTurn(Faction.Pack);
    }

    // Spawn a random tile at the given coordinates.
    public Tile SpawnRandomTile(int x, int y)
    {
        // New tile to be created...
        Tile newTile;

        // Roll to decide which tile we spawn.
        float roll = Random.Range(0f, 100f);

        // 40% - Void
        if (roll < 40)
        {
            newTile = Object.Instantiate(p_Void, tileParent);
        }
        // 30% - Air
        else if (roll < 70)
        {
            newTile = Object.Instantiate(p_Air, tileParent);
        }
        // 15% - Asteroids
        else if (roll < 85)
        {
            newTile = Object.Instantiate(p_Asteroids, tileParent);
        }
        // 10% - Water
        else if (roll < 95)
        {
            newTile = Object.Instantiate(p_Water, tileParent);
        }
        // 3% - Fire
        else if (roll < 98)
        {
            newTile = Object.Instantiate(p_Fire, tileParent);
        }
        // 2% - Planet
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

        // Reset tile lighting.
        newTile.ClearHighlight();

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

        // Get our progenitor.
        Ship progenitor = GetProgenitor(shipType);

        // Instantiate our new ship as a copy of our progenitor.
        Ship newShip = Object.Instantiate(progenitor);

        Debug.Log("Spawning " + newShip.myName + " for faction: " + newShip.faction);

        // Update health bar.
        newShip.healthBar.fillAmount = newShip.currentHealth / newShip.maxHealth;
        
        // Add to leader's fleet.
        GM.I.leaders[newShip.faction].fleet.Add(newShip);

        // - Assign parent.
        // (Doesn't really do much, just for organizational purposes)

        // Neutral
        if (newShip.faction == Faction.Neutral)
            newShip.transform.SetParent(neutralShipParent);

        // Pack
        else if (newShip.faction == Faction.Pack)
            newShip.transform.SetParent(packShipParent);

        // Coven
        else if (newShip.faction == Faction.Coven)
            newShip.transform.SetParent(covenShipParent);

        // Syndicate
        else if (newShip.faction == Faction.Syndicate)
            newShip.transform.SetParent(syndicateShipParent);

        // Set new position.
        newShip.Move(x, y, false);

        // Activate!
        newShip.gameObject.SetActive(true);

        // Return!
        return newShip;
    }

    // Return the progenitor ship of a given ship type.
    public Ship GetProgenitor(string shipName)
    {
        // Pack
        if (shipName == "Tarodactyl")
            return p_Tarodactyl;
        // Coven
        else if (shipName == "Space Witch")
            return p_SpaceWitch;
        else if (shipName == "Fairy")
            return p_Fairy;
        else if (shipName == "Treant")
            return p_Treant;
        else if (shipName == "Squirrel")
            return p_Squirrel;
        // Syndicate
        else if (shipName == "Flybot")
            return p_Flybot;
        // Neutral
        else if (shipName == "Sky Pirate")
            return p_SkyPirate;
        // Unknown
        else
        {
            Debug.LogError("Failed to find progenitor for unknown ship type: " + shipName);
            return null;
        }
    }

    // Terraform a path between the two ships.
    public void TerraformPath(Ship incomingShip, Ship targetShip)
    {
        // Find the shortest path between them.
        List<Tile> shortestPath = incomingShip.FindPathTo(targetShip.currentTile);

        // Find any void tiles in the path and terraform them to air.
        foreach (Tile tile in shortestPath)
        {
            if (tile.myType == TileType.Void)
                tile.Terraform(TileType.Air);
        }
    }
}
