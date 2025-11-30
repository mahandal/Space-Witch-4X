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

    // Attempt to move the ship to the new coordinates.
    // Fails if
    // - the tile is too far away.
    // - there is already a ship there.
    public bool AttemptMove(int newX, int newY)
    {
        // Get new tile.
        Tile newTile = GM.I.grid[newX, newY];

        // Check if tile is empty.
        if (newTile.ship != null)
            return false;

        // Check if tile is too far away.
        if (newTile.moveCostFromSelectedTile > speed || newTile.moveCostFromSelectedTile < 0)
            return false;

        // Delegate to Move!
        return Move(newX, newY);
    }

    // Move the ship to new coordinates.
    // Note: Does NOT error check!
    public bool Move(int newX, int newY)
    {
        // Get new tile.
        Tile newTile = GM.I.grid[newX, newY];

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
