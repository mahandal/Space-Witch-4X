using UnityEngine;

// Constance handles my constants!
// She's a friendly old innkeeper, proprietor of The Dragon's Roost.
// You can trust her, don't worry!
public static class Constance
{
    // How big our tiles are.
    public static int tileSize = 1;

    // - Conversions!

    // Convert grid coordinates to world coordinates.
    public static Vector3 GridToWorld(int gridX, int gridY)
    {
        return new Vector3(gridX * tileSize, gridY * tileSize, 0);
    }

    // Convert world coordinates to grid coordinates and return the closest tile.
    public static Tile WorldToGrid(Vector3 worldPos)
    {
        // Get x.
        int x = Mathf.RoundToInt(worldPos.x / tileSize);

        // Get y.
        int y = Mathf.RoundToInt(worldPos.y / tileSize);

        // Return tile!
        return GM.I.grid[x, y];
    }
}
