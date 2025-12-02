using UnityEngine;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
    [Header("Tile")]
    public string myType = "Air";
    public Faction faction = Faction.Neutral;
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
                // Get tile.
                Tile tile = GM.I.grid[i, j];

                // Clear highlight.
                tile.ClearHighlight();

                // Clear path trace.
                tile.moveCostFromSelectedTile = -1;
                tile.previousTileInPath = null;
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
        reachable.Add(this);

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
                int moveCost = costToReach[current] + neighbor.GetMovementCost(ship);

                // Check if neighbor is within movement range.
                if (moveCost <= ship.movementRemaining)
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
    public int GetMovementCost(Ship incomingShip)
    {
        // Default to 1.
        int moveCost = 1;

        // - Geography
        if (myType == "Water")
        {
            moveCost = 2;
        }
        else if (myType == "Asteroids")
        {
            moveCost = 2;
        }

        // - Ships

        // Block enemy ships
        if (ship != null && ship.faction != incomingShip.faction)
        {
            moveCost = 100;
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
        // bg.color = new Color(1f, 0.154f, 0.7932f, 0.7843f);
        bg.color = new Color(1f, 0.154f, 0.7932f, 1f);
    }

    // Set background color to purple to show a tile can be both moved to and attacked,
    // by the currently selected ship.
    public void HighlightMoveAndAttack()
    {
        // bg.color = new Color(0.6978f, 0.2402f, 0.9433f, 0.4823f);
        bg.color = new Color(0.6978f, 0.2402f, 0.9433f, 1f);
    }

    // Set background color to blue to show a tile can be moved to (but not attacked!),
    // by the currently selected ship.
    public void HighlightMove()
    {
        // bg.color = new Color(0.2932f, 0.2638f, 0.7295f, 0.6431f);
        bg.color = new Color(0.2932f, 0.2638f, 0.7295f, 1f);
    }

    // Set background color to red to show a tile can be attacked (but not moved to!),
    // by the currently selected ship.
    public void HighlightAttack()
    {
        // bg.color = new Color(0.7861f, 0.22f, 0.2256f, 0.6431f);
        bg.color = new Color(0.7861f, 0.22f, 0.2256f, 1f);
    }

    // - Hovering
    private float unhoveredOpacity;

    // Hovering increases background opacity,
    // and previews a path to the hovered tile if you are selecting a ship.
    // public void Hover()
    // {
    //     // Remember opacity
    //     unhoveredOpacity = bg.color.a;

    //     // Set new opacity.
    //     Color c = bg.color;
    //     c.a = 1f;
    //     bg.color = c;

    //     // Check if we should show a preview of a path to this tile.
    //     if (GM.I.selectedTile != null && GM.I.selectedTile.ship != null)
    //         ShowPathPreview();
    // }

    public void Hover()
    {
        // Remember opacity
        unhoveredOpacity = bg.color.a;

        // Set new opacity.
        Color c = bg.color;
        c.a = 1f;
        bg.color = c;

        // Check if we are selecting a ship and should show a preview of a path to this tile.
        if (GM.I.selectedTile != null && GM.I.selectedTile.ship != null)
        {
            // Check if this tile has an enemy ship we could attack
            if (ship != null && ship.faction != GM.I.selectedTile.ship.faction)
            {
                // Check if we have attacks remaining
                if (GM.I.selectedTile.ship.attacksRemaining > 0)
                {
                    // Find the vantage point to attack from
                    Tile vantagePoint = GM.I.selectedTile.ship.FindVantagePoint(this);
                    
                    if (vantagePoint != null)
                    {
                        // Check if we're already in range (vantage point is our current tile)
                        if (vantagePoint != GM.I.selectedTile)
                        {
                            // Show the path to the vantage point
                            vantagePoint.ShowPathPreview();
                            attackPathTile = vantagePoint;
                        }
                    }
                }
            }
            else
            {
                // Normal movement preview
                ShowPathPreview();
            }
        }
    }

    // Unhovering returns background opacity as it was.
    public void Unhover()
    {
        // Avoid unhovering the selected tile.
        if (this == GM.I.selectedTile) return;
        
        // Reset normal opacity.
        Color c = bg.color;
        c.a = unhoveredOpacity;
        bg.color = c;

        // Clear the preview of a path to this tile, if there was one.
        ClearPathPreview();

        // Clear attack path preview if one exists
        if (attackPathTile != null)
        {
            attackPathTile.ClearPathPreview();
            attackPathTile = null;
        }
    }



    // Set this tile's faction.
    public void Claim(Faction newFaction)
    {
        // Set faction.
        faction = newFaction;

        // - Set faction color.
        factionBG.color = Constance.FactionColor(newFaction, 0.5f);

        // Claim other tiles in path!
        if (previousTileInPath != null)
            previousTileInPath.Claim(newFaction);
    }

    

    // ---  Path previews

    // - Movement paths
    private LineRenderer pathLine;

    public void ShowPathPreview()
    {
        // Get selected ship.
        Ship selectedShip = GM.I.selectedTile.ship;
        if (selectedShip == null) return;

        // Create line renderer if needed.
        if (pathLine == null)
        {
            pathLine = gameObject.AddComponent<LineRenderer>();
            pathLine.startWidth = 0.1f;
            pathLine.endWidth = 0.1f;
            pathLine.material = new Material(Shader.Find("Sprites/Default"));
            pathLine.startColor = Constance.FactionColor(selectedShip.faction);
            pathLine.endColor = Constance.FactionColor(selectedShip.faction);
            pathLine.sortingOrder = 1000;
        }

        // Make sure we can actually move here.
        if (moveCostFromSelectedTile < 0 || moveCostFromSelectedTile > GM.I.selectedTile.ship.speed || ship != null)
        {
            ClearPathPreview();
            return;
        }

        // Build the path by walking backwards from this tile.
        List<Vector3> pathPositions = new List<Vector3>();
        Tile currentTile = this;

        while (currentTile != null)
        {
            Vector3 pos = currentTile.transform.position;
            pos.z = -0.5f;
            pathPositions.Add(pos);
            currentTile = currentTile.previousTileInPath;
        }

        pathPositions.Reverse();

        pathLine.positionCount = pathPositions.Count;
        pathLine.SetPositions(pathPositions.ToArray());
        pathLine.enabled = true;
    }

    public void ClearPathPreview()
    {
        if (pathLine != null)
        {
            Destroy(pathLine);
            // pathLine.enabled = false;
        }
    }

    // - Attack paths
    private static Tile attackPathTile;
}
