using UnityEngine;

public class Ship : MonoBehaviour
{
    [Header("Meta")]
    public string myName;
    public string faction;
    public int x;
    public int y;
    public Tile currentTile;

    [Header("Ship")]
    public int speed;
    public int range;

    // Move the ship to new coordinates.
    // Note: Fails if there is already a ship in the given tile.
    // Note: Does NOT check speed!
    public bool Move(int newX, int newY)
    {
        // Get new tile.
        Tile newTile = GM.I.grid[newX, newY];

        // Check that new tile is empty!
        if (newTile.ship != null)
            return false;

        // Remove from old tile.
        if (currentTile != null)
            currentTile.ship = null;

        // Set coordinates.
        x = newX;
        y = newY;

        // Set new tile
        currentTile = newTile;
        currentTile.ship = this;

        // Move physically.
        transform.position = newTile.transform.position;

        // Return!
        return true;
    }
}
