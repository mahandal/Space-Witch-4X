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
    public int health;
    public int damage;
    public int armor;
    public int speed;
    public int range;

    // Attempt to move into range and attack the target ship.
    public bool AttemptAttackMove(Ship target)
    {
        // Make sure target exists.
        if (target == null) return false;

        // Make sure target is an enemy.
        if (target.faction == faction) return false;

        // Get the target's tile.
        Tile targetTile = target.currentTile;

        // Measure distance apart.
        int distance = Utility.Distance(currentTile, target.currentTile);

        // Check if we're in range to attack them already.
        if (distance <= range)
        {
            // Attack them!
            Attack(target);

            // Done!
            return true;
        }

        return true;
    }

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
        Move(newX, newY);

        // Return true!
        return true;
    }

    // Attack the target ship with this ship's primary weapons.
    // Note: Does NOT error check!
    public void Attack(Ship target)
    {
        Debug.Log(myName + " is attacking " + target.myName + " for " + damage + " damage!");

        // Deal damage.
        target.ReceiveDamage(damage);
    }

    // Receive damage.
    public void ReceiveDamage(int incomingDamage)
    {
        // Minus armor.
        incomingDamage -= armor;

        // Lose health
        health -= incomingDamage;

        // Check death?
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    // Move the ship to new coordinates.
    // Note: Does NOT error check!
    public void Move(int newX, int newY)
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

        // Claim for your faction!
        newTile.Claim(faction);

        // Hide path preview.
        newTile.ClearPathPreview();
    }
}
