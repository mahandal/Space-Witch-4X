using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Ship : MonoBehaviour
{
    [Header("Meta")]
    public string myName;
    public string faction;
    public int x;
    public int y;
    public Tile currentTile;

    [Header("Core")]
    public float currentHealth;
    public int maxHealth;
    public int damage;
    public int armor;
    public int speed;
    public int range;

    [Header("Machinery")]
    public Image healthBar;

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
        int distance = Utility.Distance(currentTile, targetTile);

        // Check if we're in range to attack them already.
        if (distance <= range)
        {
            // Attack them!
            Attack(target);

            // Done!
            return true;
        }

        // We need to move closer. Find the best tile to move to.
        Tile bestTile = FindVantagePoint(targetTile);

        // Check if we found a tile.
        if (bestTile == null)
        {
            // Can't get close enough.
            return false;
        }

        // Move to our vantage point.
        Move(bestTile.x, bestTile.y);

        // Attack!
        Attack(target);

        // Return successful.
        return true;
    }

    // Find the best position to attack from,
    // meaning the closest valid tile (for now!)
    public Tile FindVantagePoint(Tile targetTile)
    {
        // Get all tiles we can move to.
        HashSet<Tile> moveableTiles = currentTile.GetTilesInMovementRange();

        // Remember our best tile.
        Tile bestTile = null;

        // Remember the shortest travel distance.
        int shortestTravelDistance = int.MaxValue;

        // Look through all moveable tiles.
        foreach (Tile potentialTile in moveableTiles)
        {
            // Ignore tiles with ships already on them.
            if (potentialTile.ship != null)
                continue;

            // Calculate distance from this ship.
            int travelDistance = Utility.Distance(currentTile, potentialTile);

            // Calculate distance to our target.
            int attackDistance = Utility.Distance(potentialTile, targetTile);

            // Check if we're in range.
            if (attackDistance <= range)
            {
                // Check if it's a new best.
                if (travelDistance < shortestTravelDistance)
                {
                    // Remember this as our new best tile.
                    bestTile = potentialTile;

                    // Remember this as our new shortest distance.
                    shortestTravelDistance = travelDistance;
                }
            }
        }

        return bestTile;
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
        currentHealth -= incomingDamage;

        // Check death?
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }

        // Update health bar.
        healthBar.fillAmount = currentHealth / maxHealth;
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
