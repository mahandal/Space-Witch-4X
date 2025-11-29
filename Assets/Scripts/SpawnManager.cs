using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("Progenitors")]
    public Tile p_Air;
    public Tile p_Planet;
    public Tile p_Water;
    public Tile p_Fire;
    public Tile p_Asteroids;

    // Start your engines!
    void Start()
    {
        // Spawn a new map 15 tiles wide and 10 tiles tall.
        GenerateNewMap(15, 10);
    }

    // Procedurally generate a new map.
    public void GenerateNewMap(int width, int height)
    {
        // TBD: Clear old map first!
        ClearMap();

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

        // TBD: Spawn in heroes...

        int covenX = Random.Range(0, GM.I.gridWidth);
        int covenY = Random.Range(0, GM.I.gridHeight);
        GM.I.covenHero.Move(covenX, covenY);
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
            newTile = Object.Instantiate(p_Air, GM.I.universe);
        }
        // 20% - Water
        else if (roll < 70)
        {
            newTile = Object.Instantiate(p_Water, GM.I.universe);
        }
        // 15% - Asteroids
        else if (roll < 85)
        {
            newTile = Object.Instantiate(p_Asteroids, GM.I.universe);
        }
        // 10% - Fire
        else if (roll < 95)
        {
            newTile = Object.Instantiate(p_Fire, GM.I.universe);
        }
        // 5% - Planet
        else
        {
            newTile = Object.Instantiate(p_Planet, GM.I.universe);
        }

        // Assign in grid.
        GM.I.grid[x, y] = newTile;

        // Remember coordinates.
        newTile.x = x;
        newTile.y = y;

        // Place in space.
        newTile.transform.position = Constance.GridToWorld(x, y);

        // Activate!
        newTile.gameObject.SetActive(true);

        // Return!
        return newTile;
    }

    // Clear the old map of all tiles and ships!
    public void ClearMap()
    {

    }
}
