using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Traits
public enum AutoPilotMode
{
    Off,
    Explore,
    Rest,
    Guard,
    Hunt,
    Full
}

public partial class Ship : MonoBehaviour
{
    [Header("AUTO-PILOT")]
    // public string autoPilot = "Off";
    public AutoPilotMode autoPilotMode = AutoPilotMode.Off;

    // Set the auto pilot mode.
    public void SetAutoPilot(AutoPilotMode newAutoPilotMode)
    {
        // Set new auto pilot mode.
        autoPilotMode = newAutoPilotMode;

        // Highlight new auto pilot mode.
        UI.I.HighlightAutoPilot();

        // Check if this was the last ship to manage and we should reveal the end turn button.
        UI.I.WhichButtonInTopRight();
    }

    // Perform this ship's turn, decided by its auto pilot.
    public IEnumerator AutoPilot()
    {
        // Auto-Pilot mode: Off
        if (autoPilotMode == AutoPilotMode.Off)
            yield break;

        // Center camera on ship.
        ShowVision();

        // Give each ship a first moment on screen.
        yield return new WaitForSeconds(0.3f);

        if (autoPilotMode == AutoPilotMode.Explore)
            yield return Explore();
        else if (autoPilotMode == AutoPilotMode.Rest)
            AttemptRest();
        else if (autoPilotMode == AutoPilotMode.Guard)
            Guard();
        else if (autoPilotMode == AutoPilotMode.Hunt)
            yield return Hunt();
        else if (autoPilotMode == AutoPilotMode.Full)
            yield return FullAutoPilot();

        // Give each ship a last moment on screen.
        yield return new WaitForSeconds(0.3f);
    }

    // Explore the stars!
    // Find the nearest neutral tile and move toward it!
    // Returns to manual control if no neutral tiles remain.
    public IEnumerator Explore(bool fullAuto = false)
    {
        // Find the nearest neutral tile
        Tile destination = FindNearestNeutralTile();

        // Return to manual control if no neutral tiles remain.
        if (destination == null)
        {
            autoPilotMode = AutoPilotMode.Off;
            yield break;
        }

        Debug.Log(myName + " is exploring and found tile (" + destination.x + ", "
            + destination.y + ") as their destination.");

        // Remember the tile we start on.
        Tile startingTile = currentTile;

        // Move toward our destination.
        yield return MoveToward(destination);

        // Check if we moved successfully.
        bool successfullyMoved = (startingTile != currentTile);

        // Follow our ships exploring around!
        ShowVision();
        yield return new WaitForSeconds(0.3f);

        // Keep going?
        if (successfullyMoved)
        {
            Debug.Log(myName + " explored to ("
                + currentTile.x + ", " + currentTile.y + ").");

            // Should we return to full auto pilot mode?
            if (fullAuto)
                yield return FullAutoPilot();
            else
                yield return Explore();
        }
        else
        {
            Debug.Log(myName + " failed to move toward ("
                + destination.x + ", " + destination.y + ")"
                + " and is going to rest with " + movementRemaining
                + " movement remaining.");
            AttemptRest();
        }
    }

    // Guard an area, waking up upon seeing an enemy within attack range.
    public void Guard()
    {
        // Look for enemies within attack range.
        List<Ship> enemiesInAttackRange = GetEnemiesInAttackRange();

        // Check if there are any.
        if (enemiesInAttackRange.Count > 0)
        {
            // Wake up!
            SetAutoPilot(AutoPilotMode.Off);
        }
    }

    // Hunt
    public IEnumerator Hunt()
    {
        // Look for an enemy.
        List<Ship> visibleEnemies = GetVisibleEnemies();

        // Check if there are any enemies in sight.
        if (visibleEnemies.Count > 0 && attacksRemaining > 0)
        {
            // Remember our xp to see if we gained any, or if we are stuck.
            int priorExperience = xp;

            // If there is an enemy in sight, attack move toward them!
            yield return AttackNearestEnemy();

            // Check if gained any experience.
            // Any movement or attack should gain some xp,
            // so if we haven't gained any that means we are unable to move or attack any more.
            if (priorExperience == xp)
            {
                // Couldn't move or attack.

                // Rest?
                AttemptRest();

                yield break;
            }
        }
        else
        {
            // If there are no enemies in sight, look for another faction's tile to claim.
            yield return Conquer(true);
        }

        // Follow with the camera.
        ShowVision();

        // Wait a moment.
        yield return new WaitForSeconds(0.1f);

        // Keep going?
        if (movementRemaining > 0 || attacksRemaining > 0)
            yield return Hunt();
    }

    // Attack move toward the nearest enemy we can see.
    public IEnumerator AttackNearestEnemy()
    {
        // Get list of visible enemies.
        List<Ship> visibleEnemies = GetVisibleEnemies();

        // Find movement cost to them.
        // (and to everything else, but who's counting?)
        currentTile.GetAllConnectedTiles();

        // Find the closest enemy to us.
        Ship closestEnemy = null;
        int closestDistance = int.MaxValue;

        foreach (Ship enemy in visibleEnemies)
        {
            // Check movement cost.
            if (enemy.currentTile.moveCostFromCurrentTile < closestDistance)
            {
                // Remember new best.
                closestEnemy = enemy;
                closestDistance = enemy.currentTile.moveCostFromCurrentTile;
            }
        }

        // Attack move toward our target!
        yield return AttemptAttackMove(closestEnemy, true);
    }

    // Auto Pilot - Conquer
    // Find the closest tile owned by another faction and move toward it.
    public IEnumerator Conquer(bool isHunting = false)
    {
        // Get the nearest enemy tile.
        Tile destination = GetNearestEnemyTile();

        Debug.Log(myName + " is conquering toward tile ("
            + destination.x + ", " + destination.y + ")");

        // Move toward our destination.
        yield return MoveToward(destination);

        // Check if we moved successfully.
        bool successfullyMoved = (destination == currentTile);

        if (successfullyMoved)
            Debug.Log(myName + " thinks she has successfully moved toward her destination!");
        else
            Debug.Log(myName + " failed to move toward her destination. Should stop now...");

        // Follow ships exploring around!
        ShowVision();
        yield return new WaitForSeconds(0.3f);

        // Keep going?
        if (successfullyMoved)
        {
            if (isHunting)
                yield return Hunt();
            else
                yield return Conquer();
        }
        else
        {
            // Can't move. Attack?
            if (CanAttack())
                yield return AttackNearestEnemy();
            else
                AttemptRest();
        }
    }

    // Returns true if there is an enemy ship within this ship's vision and attack range,
    // and we have attacks remaining.
    public bool CanAttack()
    {
        // Check if we have attacks remaining.
        if (attacksRemaining <= 0) return false;

        // Get a list of enemy ships in this ship's vision range.
        List<Ship> visibleEnemies = GetVisibleEnemies();

        // Get a list of enemy ships in this ship's attack range.
        List<Ship> attackableEnemies = GetEnemiesInAttackRange();

        // Iterate through our list of visible enemies.
        foreach (Ship ship in visibleEnemies)
        {
            // See if any of them are attackable.
            if (attackableEnemies.Contains(ship))
                return true; // We can attack this one!
        }

        // Return false if we didn't find a ship in both our vision and attack range.
        return false;
    }

    // Find the nearest tile belonging to another faction.
    public Tile GetNearestEnemyTile()
    {
        // Set movement costs for all tiles.
        HashSet<Tile> tiles = currentTile.GetAllConnectedTiles();

        // Remember the closest enemy tile we've seen so far.
        Tile closestEnemyTile = null;
        int closestDistance = int.MaxValue;

        // Iterate through all tiles
        foreach (Tile tile in tiles)
        {
            // Check if tile is owned by another faction and empty.
            if (tile.faction != Faction.Neutral &&
                tile.faction != faction &&
                tile.ship == null)
            {
                // Compare distance.
                if (tile.moveCostFromCurrentTile < closestDistance)
                {
                    // Remember best tile.
                    closestEnemyTile = tile;
                    closestDistance = tile.moveCostFromCurrentTile;
                }
            }
        }

        // If closest enemy tile is still null,
        // that must mean enemy ships are covering all their tiles.
        // Find nearest neutral tile instead!
        if (closestEnemyTile == null)
        {
            return FindNearestNeutralTile();
        }

        // Return best tile.
        return closestEnemyTile;
    }

    // Full auto pilot:
    // - Prioritizes fighting an enemy if we can see one.
    // - Otherwise, explore!
    public IEnumerator FullAutoPilot()
    {
        // Full auto pilot takes a moment to think?
        yield return new WaitForSeconds(0.1f);

        // Find all visible enemies.
        List<Ship> visibleEnemies = GetVisibleEnemies();

        // Hunt if we see an enemy.
        if (visibleEnemies.Count > 0)
            yield return Hunt();
        else
            yield return Explore(true);
    }

    // Reveal this ship's vision to the player.
    // Also centers the camera on this ship's tile.
    public void ShowVision()
    {
        // Get a set of tiles this ship can see.
        HashSet<Tile> visibleTiles = currentTile.GetTilesInVisionRange();

        // Check if this ship died.
        if (visibleTiles == null)
        {
            currentTile.RevealFromFog();
        }
        else
        {   
            // Go through every tile.
            foreach (Tile tile in Tile.GetAllTiles())
            {
                // Check if tile should be visible.
                if (visibleTiles.Contains(tile))
                {
                    tile.RevealFromFog();
                } else {
                    tile.HideInFog();
                }
            }
        }

        // Center the camera.
        Utility.MoveCamera(currentTile);

        // Select the current tile.
        currentTile.Select();
    }
}