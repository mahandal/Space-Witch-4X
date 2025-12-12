using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class Ship : MonoBehaviour
{
    [Header("AUTO-PILOT")]
    public string autoPilot = "Off";

    // Set the auto pilot mode.
    public void SetAutoPilot(string newAutoPilotMode)
    {
        // Set new auto pilot mode.
        autoPilot = newAutoPilotMode;

        // Highlight new auto pilot mode.
        UI.I.HighlightAutoPilot();

        // Check if this was the last ship to manage and we should reveal the end turn button.
        UI.I.WhichButtonInTopRight();
    }

    // Perform this ship's turn, decided by its auto pilot.
    public IEnumerator AutoPilot()
    {
        // Auto-Pilot mode: Off
        if (autoPilot == "Off")
            yield break;

        // Center camera on ship.
        Utility.MoveCamera(currentTile);

        // Give each ship a first moment on screen.
        yield return new WaitForSeconds(0.3f);

        if (autoPilot == "Explore")
            yield return Explore();
        else if (autoPilot == "Rest")
            AttemptRest();
        else if (autoPilot == "Guard")
            Guard();
        else if (autoPilot == "Full")
            yield return FullAutoPilot();

        // Give each ship a last moment on screen.
        yield return new WaitForSeconds(0.3f);
    }

    // Explore the stars!
    // Find the nearest neutral tile and move toward it!
    // Returns to manual control if no neutral tiles remain.
    public IEnumerator Explore()
    {
        // Find the nearest neutral tile
        Tile destination = FindNearestNeutralTile();

        // Return to manual control if no neutral tiles remain.
        if (destination == null)
        {
            autoPilot = "Off";
            yield break;
        }

        // Check if we have enough movement to get to our destination.
        // if (destination.moveCostFromCurrentTile > movementRemaining)
        //     yield break;

        // Move toward our destination!
        bool successfullyMoved = MoveToward(destination);

        // Follow our ships exploring around!
        Utility.MoveCamera(currentTile);
        yield return new WaitForSeconds(0.3f);

        // Keep going?
        if (successfullyMoved)
            yield return Explore();
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
            autoPilot = "Off";
        }
    }

    // TBD!
    public IEnumerator FullAutoPilot()
    {
        // Full auto pilot takes a moment to think?
        yield return new WaitForSeconds(0.1f);
    }

    // Reveal this ship's vision to the player.
    public void ShowVision()
    {
        // Get a set of tiles this ship can see.
        HashSet<Tile> visibleTiles = currentTile.GetTilesInVisionRange();

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
}