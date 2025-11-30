using UnityEngine;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
    [Header("Tile")]
    public string myType = "Air";
    public string faction = "Neutral";
    public int x = 0;
    public int y = 0;
    public Ship ship;

    [Header("Automated Machinery")]
    public int moveCostFromSelectedTile = -1;
    public Tile previousTileInPath;

    [Header("Manual Machinery")]
    // The faction color for this tile.
    public SpriteRenderer factionBG;

    // The contextual background color on top of this tile's sprite.
    // Completely clear by default.
    // Highlights differently to show:
    // - Selected tile is highlighted pink!
    // - Movement range is shown in blue.
    // - Attack range is shown in red.
    // - Purple is used for tiles a ship can move and attack to.
    // (so blue is actually rare, only used for pacifists and ships with minimum range)
    public SpriteRenderer bg;

    // Clear our selection so no tiles are highlighted.
    public static void ClearSelection()
    {
        // Clear selection.
        GM.I.selectedTile = null;

        // Clear highlights.
        ClearAllHighlights();
    }

    // Clear ALL highlighting for ALL tiles.
    public static void ClearAllHighlights()
    {
        // Loop through columns.
        for (int i = 0; i < GM.I.gridWidth; i++)
        {
            // Loop through rows.
            for (int j = 0; j < GM.I.gridHeight; j++)
            {
                // Clear!
                GM.I.grid[i, j].ClearHighlight();
            }
        }
    }


    // Select this tile!
    public void Select()
    {
        // Clear old highlighting.
        ClearAllHighlights();

        // Highlight movement and attack ranges.
        if (ship != null)
            HighlightRanges();

        // Highlight the current tile!
        HighlightSelected();

        // Set as currently selected tile.
        GM.I.selectedTile = this;
    }

    // Highlight nearby tiles to show the selected ship's movement and attack ranges.
    // TBD!
    public void HighlightRanges()
    {
        // Make sure we have a ship!
        if (ship == null) return;

        // Get all tiles within movement range.
        HashSet<Tile> moveableTiles = GetTilesInMovementRange();

        // - Loop through all tiles again now that we know where we can go.

        // Loop through columns.
        for (int i = 0; i < GM.I.gridWidth; i++)
        {
            // Loop through rows.
            for (int j = 0; j < GM.I.gridHeight; j++)
            {
                // Get tile.
                Tile otherTile = GM.I.grid[i, j];

                // Calculate distance for determining attack range.
                Tile closestMoveableTile = otherTile.GetClosestTile(moveableTiles);
                int distance = Utility.Distance(closestMoveableTile, otherTile);

                // Assign booleans.
                bool canMove = moveableTiles.Contains(otherTile) && otherTile.ship == null;
                bool canAttack = distance <= ship.range;

                // - Highlight!
                if (canMove && canAttack)
                {
                    otherTile.HighlightMoveAndAttack();
                }
                else if (canMove)
                {
                    otherTile.HighlightMove();
                }
                else if (canAttack)
                {
                    otherTile.HighlightAttack();
                }
            }
        }
    }

    // Search through the given hash set to find the closest tile to the tile that calls this function.
    // Note: Closest is defined as the bird flies here. Used for attack ranges!
    public Tile GetClosestTile(HashSet<Tile> validTiles)
    {
        // Remember the best tile we've found so far.
        Tile bestTileYet = null;

        // Initialize shortest distance to max value.
        int shortestDistance = int.MaxValue;

        // Go through each potential tile.
        foreach (Tile potentialTile in validTiles)
        {
            // Get the distance between this tile and the potential tile.
            int distance = Utility.Distance(this, potentialTile);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                bestTileYet = potentialTile;
            }
        } 

        // Return the best tile we found in total!
        return bestTileYet;
    }

    // Get a set of all tiles the ship on this tile can move to.
    public HashSet<Tile> GetTilesInMovementRange()
    {
        // Make sure we have a ship!
        if (ship == null) return null;

        // Remember all tiles that are reachable, to be returned at the end.
        HashSet<Tile> reachable = new HashSet<Tile>();

        // Remember the cost to reach each tile.
        Dictionary<Tile, int> costToReach = new Dictionary<Tile, int>();

        // Queue of tiles to be examined.
        Queue<Tile> frontier = new Queue<Tile>();

        // Start from current tile.
        frontier.Enqueue(this);

        // Reset current tile.
        costToReach[this] = 0;
        moveCostFromSelectedTile = 0;
        previousTileInPath = null;

        // Loop until we've explored the frontier!
        while (frontier.Count > 0)
        {
            // Get the next tile up.
            Tile current = frontier.Dequeue();

            // Get its neighbors.
            List<Tile> neighbors = new List<Tile>();
            Tile leftNeighbor;
            Tile rightNeighbor;
            Tile downNeighbor;
            Tile upNeighbor;

            // Left neighbor needs you to be away from the left border.
            if (current.x > 0)
            {
                leftNeighbor = GM.I.grid[current.x - 1, current.y];
                neighbors.Add(leftNeighbor);
            }

            // Right neighbor needs you to be away from the right border.
            if (current.x < GM.I.gridWidth - 1)
            {
                rightNeighbor = GM.I.grid[current.x + 1, current.y];
                neighbors.Add(rightNeighbor);
            }

            // Down neighbor needs you to be away from the bottom border.
            if (current.y > 0)
            {
                downNeighbor = GM.I.grid[current.x, current.y - 1];
                neighbors.Add(downNeighbor);
            }

            // Up neighbor needs you to be away from the top border.
            if (current.y < GM.I.gridHeight - 1)
            {
                upNeighbor = GM.I.grid[current.x, current.y + 1];
                neighbors.Add(upNeighbor);
            }

            // Check if we can move to each neighbor.
            foreach (Tile neighbor in neighbors)
            {
                // Get total move cost for this neighbor.
                int moveCost = costToReach[current] + neighbor.GetMovementCost();

                // Check if neighbor is within movement range.
                if (moveCost <= ship.speed)
                {
                    // Check if we already have a path to this tile.
                    if (costToReach.ContainsKey(neighbor))
                    {
                        // Check if we've found a better path!
                        if (moveCost < costToReach[neighbor])
                        {
                            // Set new cost to reach.
                            costToReach[neighbor] = moveCost;

                            // Remember how we got here.
                            neighbor.moveCostFromSelectedTile = moveCost;
                            neighbor.previousTileInPath = current;

                            // Add back to the frontier!
                            frontier.Enqueue(neighbor);
                        }
                    } else {
                        // First path here!
                        // So it has to be the best!
                        // (The worst too, but we don't track those!)

                        // Add to reachable tiles!
                        reachable.Add(neighbor);

                        // Set new cost to reach.
                        costToReach[neighbor] = moveCost;

                        // Remember how we got here.
                        neighbor.moveCostFromSelectedTile = moveCost;
                        neighbor.previousTileInPath = current;

                        // Add to the frontier!
                        frontier.Enqueue(neighbor);
                    }
                }
            }
        }

        // Return all reachable tiles!
        return reachable;
    }

    // Get the movement cost for a tile.
    public int GetMovementCost()
    {
        // Default to 1.
        int moveCost = 1;

        // Go through each special case!
        if (myType == "Water")
        {
            moveCost = 2;
        }
        else if (myType == "Asteroids")
        {
            moveCost = 2;
        }

        // Return!
        return moveCost;
    }


    // - Colorize background.

    // Clear background color.
    public void ClearHighlight()
    {
        bg.color = new Color(0.5f, 0.5f, 0.5f, 0f);
    }

    // Set background color to pink to show the currently selected tile.
    public void HighlightSelected()
    {
        bg.color = new Color(1f, 0.154f, 0.7932f, 0.7843f);
    }

    // Set background color to purple to show a tile can be both moved to and attacked,
    // by the currently selected ship.
    public void HighlightMoveAndAttack()
    {
        bg.color = new Color(0.6978f, 0.2402f, 0.9433f, 0.4823f);
    }

    // Set background color to blue to show a tile can be moved to (but not attacked!),
    // by the currently selected ship.
    public void HighlightMove()
    {
        bg.color = new Color(0.2932f, 0.2638f, 0.7295f, 0.6431f);
    }

    // Set background color to red to show a tile can be attacked (but not moved to!),
    // by the currently selected ship.
    public void HighlightAttack()
    {
        bg.color = new Color(0.7861f, 0.22f, 0.2256f, 0.6431f);
    }

    // - Hovering
    private float unhoveredOpacity;

    // Hovering increases background opacity.
    public void Hover()
    {
        // Remember opacity
        unhoveredOpacity = bg.color.a;

        // Set new opacity.
        Color c = bg.color;
        c.a = 1f;
        bg.color = c;
    }

    // Unhovering returns background opacity as it was.
    public void Unhover()
    {
        // Avoid unhovering the selected tile.
        if (this == GM.I.selectedTile) return;
        
        Color c = bg.color;
        c.a = unhoveredOpacity;
        bg.color = c;
    }



    // Set this tile's faction.
    public void Claim(string newFaction)
    {
        // Set faction.
        faction = newFaction;

        // - Set faction color.

        // Neutral
        if (faction == "Neutral")
            factionBG.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        // The Swarm
        else if (faction == "Swarm")
            factionBG.color = new Color(0f, 1f, 0.0429f, 0.5f);

        // The Coven
        else if (faction == "Coven")
            factionBG.color = new Color(0f, 0.9889f, 1f, 0.5f);

        // The Syndicate
        else if (faction == "Syndicate")
            factionBG.color = new Color(1f, 0.8535f, 0f, 0.5f);

        // Claim other tiles in path!
        if (previousTileInPath != null)
            previousTileInPath.Claim(newFaction);
    }

        
}
