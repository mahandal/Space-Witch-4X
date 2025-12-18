using UnityEngine;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
    [Header("Meta")]
    public TileType myType = TileType.Void;
    public Faction faction = Faction.Neutral;
    public int x = 0;
    public int y = 0;
    public Ship ship;

    [Header("Stats")]
    public int moveCost = 1;
    public int armorBonus = 0;
    public int visionCost = 1;
    public int damageOnEnter = 0;
    public int damageOnUpkeep = 0;

    [Header("Fog of War")]
    public bool isVisibleToPlayer = false;
    public SpriteRenderer fogOverlay;

    [Header("Pathfinding")]
    public int moveCostFromCurrentTile = -1;
    public Tile previousTileInPath;
    public Tile nextTileInPath;

    [Header("Manual Machinery")]
    // The sprite showing this tile's type.
    public SpriteRenderer typeImage;

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

    // Called when a ship enters this tile.
    public void OnEnter(Ship incomingShip)
    {
        // - Damage

        // Fire
        if (myType == TileType.Fire)
            incomingShip.ReceiveDamage(damageOnEnter, DamageType.Fire);

        // - Traits
        incomingShip.OnEnter(this);
    }

    // Clear our selection so no tiles are highlighted.
    public static void ClearSelection()
    {
        // Clear selected tile.
        GM.I.selectedTile = null;

        // Clear highlights.
        ClearAllHighlights();

        // Clear tooltips.
        UI.I.ClearSelection();

        // Clear building selection.
        GM.I.ResetBuildOrder();
    }

    // Clear ALL highlighting for ALL tiles.
    // Note: Also clears all path traces!
    public static void ClearAllHighlights()
    {
        // Go through each tile.
        foreach (Tile tile in GetAllTiles())
        {
            // Clear the highlight for this tile.
            tile.ClearHighlight();

            // Clear the path trace for this tile.
            tile.ClearPathTrace();
        }
    }

    // Clear all traces of all previous pathfinding!
    public static void ClearAllPathTraces()
    {
        // Go through each tile.
        foreach (Tile tile in GetAllTiles())
        {
            // Clear the tile's trace.
            tile.ClearPathTrace();
        }
    }

    // Clear the path trace for this tile.
    public void ClearPathTrace()
    {
        // Reset path trace!
        moveCostFromCurrentTile = int.MaxValue;
        previousTileInPath = null;
        nextTileInPath = null;
    }


    // Select this tile!
    public void Select()
    {
        // Clear old previews.
        ClearPathPreview();
        ClearAttackPreview();

        // Clear old highlighting.
        ClearAllHighlights();

        // Check if we're selecting a ship.
        if (ship != null)
        {
            // Clear building selection.
            GM.I.ResetBuildOrder();

            // Highlight movement and attack ranges.
            HighlightRanges();

            // Let GM remember.
            GM.I.lastSelectedShip = ship;
        }
        // Check if we're selecting one of our planets to build on.
        else if (GM.I.currentlyBuilding != "" &&
            myType == TileType.Planet && faction == GM.I.playerFaction)
        {
            // Build a ship!
            GM.I.BuildShip(GM.I.currentlyBuilding, this);

            // Return early.
            // (don't need to select a new ship cause they can't act immediately)
            return;
        }

        // Highlight the current tile!
        HighlightSelected();

        // Set as currently selected tile.
        GM.I.selectedTile = this;

        // UI!
        UI.I.SelectTile(this);
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
                bool canAttack = distance <= ship.range && otherTile.myType != TileType.Void;

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

    // Get a set of all tiles the ship on this tile can move to with its current movement.
    // Note: Also finds the movement cost it would take to get to each tile from here.
    public HashSet<Tile> GetTilesInMovementRange()
    {
        // Make sure we have a ship!
        if (ship == null) return null;

        // Clear all prior path traces first.
        ClearAllPathTraces();

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
        moveCostFromCurrentTile = 0;
        previousTileInPath = null;
        nextTileInPath = null;

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
                            neighbor.moveCostFromCurrentTile = moveCost;
                            neighbor.previousTileInPath = current;
                            current.nextTileInPath = neighbor;

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
                        neighbor.moveCostFromCurrentTile = moveCost;
                        neighbor.previousTileInPath = current;
                        current.nextTileInPath = neighbor;

                        // Add to the frontier!
                        frontier.Enqueue(neighbor);
                    }
                }
            }
        }

        // Return all reachable tiles!
        return reachable;
    }


    // Return a set of all tiles that are connected to this tile.
    // I.E. generally will return the full mainland, but will NOT include islands (or void!)
    // Note: Also finds the movement cost it would take to get to each tile from here.
    public HashSet<Tile> GetAllConnectedTiles()
    {
        // Clear all prior path traces first.
        ClearAllPathTraces();

        // Remember all tiles that are reachable, to be returned at the end.
        HashSet<Tile> reachable = new HashSet<Tile>();

        // Remember the cost to reach each tile.
        Dictionary<Tile, int> costToReach = new Dictionary<Tile, int>();

        // Queue of tiles to be examined.
        Queue<Tile> frontier = new Queue<Tile>();

        // Start from current tile.
        frontier.Enqueue(this);
        reachable.Add(this);
        costToReach[this] = 0;
        moveCostFromCurrentTile = 0;
        previousTileInPath = null;
        nextTileInPath = null;

        // Loop until we've explored the frontier!
        while (frontier.Count > 0)
        {
            // Get the next tile up.
            Tile current = frontier.Dequeue();

            // Get its neighbors.
            List<Tile> neighbors = new List<Tile>();
            Tile leftNeighbor = GM.I.GetTile(current.x - 1, current.y);
            Tile rightNeighbor = GM.I.GetTile(current.x + 1, current.y);
            Tile downNeighbor = GM.I.GetTile(current.x, current.y - 1);
            Tile upNeighbor = GM.I.GetTile(current.x, current.y + 1);

            // Respect boundaries and ignore void tiles.
            if (leftNeighbor != null && leftNeighbor.myType != TileType.Void)
                neighbors.Add(leftNeighbor);
            if (rightNeighbor != null && rightNeighbor.myType != TileType.Void)
                neighbors.Add(rightNeighbor);
            if (downNeighbor != null && downNeighbor.myType != TileType.Void)
                neighbors.Add(downNeighbor);
            if (upNeighbor != null && upNeighbor.myType != TileType.Void)
                neighbors.Add(upNeighbor);

            // Travel through each neighboring tile.
            foreach (Tile neighbor in neighbors)
            {
                // Get total move cost for this neighbor.
                int moveCost = costToReach[current] + neighbor.GetMovementCost(ship);

                // Check if we already know a path to this tile.
                if (costToReach.ContainsKey(neighbor))
                {
                    // Check if we've found a better path!
                    if (moveCost < costToReach[neighbor])
                    {
                        // Set new cost to reach.
                        costToReach[neighbor] = moveCost;

                        // Remember how we got here.
                        neighbor.moveCostFromCurrentTile = moveCost;
                        neighbor.previousTileInPath = current;
                        current.nextTileInPath = neighbor;

                        // Add back to the frontier!
                        frontier.Enqueue(neighbor);
                    }
                } else {
                    // First path here!
                    // So it has to be the best!

                    // Add to reachable tiles.
                    reachable.Add(neighbor);

                    // Set new cost to reach.
                    costToReach[neighbor] = moveCost;

                    // Remember how we got here.
                    neighbor.moveCostFromCurrentTile = moveCost;
                    neighbor.previousTileInPath = current;
                    current.nextTileInPath = neighbor;

                    // Add to the frontier!
                    frontier.Enqueue(neighbor);
                }
            }
        }

        // Return all tiles potentially reachable from this tile.
        return reachable;
    }

    // Get a set of all tiles NOT connected to this tile.
    // I.E. returns all tiles on islands, isolated and unreachable by normal means.
    // Note: Delegates to GetAllConnectedTiles()
    public HashSet<Tile> GetIslandTiles()
    {
        // Delegate to GetAllConnectedTiles for heavy lifting.
        HashSet<Tile> connectedTiles = GetAllConnectedTiles();

        // Remember island tiles.
        HashSet<Tile> islandTiles = new HashSet<Tile>();

        // Invert set manually by going through each tile and checking if it is connected.
        foreach (Tile tile in GetAllTiles())
        {
            // Check tile is NOT connected!
            if (!connectedTiles.Contains(tile))
                islandTiles.Add(tile); // Add to set!
        }

        // Return!
        return islandTiles;
    }

    // Get a set of ALL tiles in the grid.
    public static HashSet<Tile> GetAllTiles()
    {
        // Remember ALL tiles.
        HashSet<Tile> tiles = new HashSet<Tile>();

        // Loop through horizontally.
        for (int i = 0; i < GM.I.gridWidth; i++)
        {
            // Loop through vertically.
            for (int j = 0; j < GM.I.gridHeight; j++)
            {
                // Get tile.
                Tile tile = GM.I.grid[i, j];

                // Add to set.
                tiles.Add(tile);
            }
        }

        // Return.
        return tiles;
    }

    // Get a set of all non-void tiles in the grid.
    public static HashSet<Tile> GetTiles()
    {
        // Remember tiles.
        HashSet<Tile> tiles = new HashSet<Tile>();

        // Loop through horizontally.
        for (int i = 0; i < GM.I.gridWidth; i++)
        {
            // Loop through vertically.
            for (int j = 0; j < GM.I.gridHeight; j++)
            {
                // Get tile.
                Tile tile = GM.I.grid[i, j];

                // Check tile is not void.
                if (tile.myType != TileType.Void)
                {
                    // Add to set.
                    tiles.Add(tile);
                }
            }
        }

        // Return.
        return tiles;
    }

    // Get a set of all tiles the ship on this tile can see.
    public HashSet<Tile> GetTilesInVisionRange()
    {
        // Nothing can't see nothing!
        if (ship == null) return null;
        
        // Store all visible tiles.
        HashSet<Tile> visible = new HashSet<Tile>();

        // Store the cost to see each tile.
        Dictionary<Tile, int> costToSee = new Dictionary<Tile, int>();

        // Track tiles that we've heard of and need to investigate further.
        Queue<Tile> frontier = new Queue<Tile>();

        // Start with the current tile.
        frontier.Enqueue(this);
        visible.Add(this);

        // Can always see yourself!
        costToSee[this] = 0;

        // Go until there's no more relevant tiles.
        while (frontier.Count > 0)
        {
            // Get the current tile.
            Tile current = frontier.Dequeue();

            // Get each neighboring tile.
            List<Tile> neighbors = new List<Tile>();
            if (current.x > 0) neighbors.Add(GM.I.grid[current.x - 1, current.y]);
            if (current.y > 0) neighbors.Add(GM.I.grid[current.x, current.y - 1]);
            if (current.x < GM.I.gridWidth - 1) neighbors.Add(GM.I.grid[current.x + 1, current.y]);
            if (current.y < GM.I.gridHeight - 1) neighbors.Add(GM.I.grid[current.x, current.y + 1]);

            // Go through each neighbor.
            foreach (Tile neighbor in neighbors)
            {
                // Find the total cost to see this neighbor.
                int neighborVisionCost = costToSee[current] + neighbor.GetVisionCost(ship);

                // Check if this neighbor is within our vision range.
                if (neighborVisionCost <= ship.vision)
                {
                    // Check if we already have a path to see this tile.
                    if (costToSee.ContainsKey(neighbor))
                    {
                        // Check if we found a new best path.
                        if (neighborVisionCost < costToSee[neighbor])
                        {
                            // Set new best vision cost.
                            costToSee[neighbor] = neighborVisionCost;

                            // Add our new path to the frontier.
                            frontier.Enqueue(neighbor);
                        }
                    }
                    else
                    {
                        // Add this tile to our set of visible tiles.
                        visible.Add(neighbor);

                        // Remember our cost to see this neighbor.
                        costToSee[neighbor] = neighborVisionCost;

                        // Add the new tile to the frontier!
                        frontier.Enqueue(neighbor);
                    }
                }
            }
        }

        // Return all visible tiles.
        return visible;
    }

    // Get the movement cost for a tile.
    public int GetMovementCost(Ship incomingShip)
    {
        // Default.
        int totalMoveCost = moveCost;

        // Hypothetically, it's that simple!
        if (incomingShip == null)
            return totalMoveCost;


        // - Traits
            
        // Aquatic
        if (incomingShip.traits.Contains(Trait.Aquatic) && myType == TileType.Water)
        {
            totalMoveCost = 1;
        }

        // Pilot
        if (incomingShip.traits.Contains(Trait.Pilot) && 
            (myType == TileType.Asteroids || myType == TileType.Planet))
        {
            totalMoveCost = 1;
        }


        // - Territory

        // Claiming territory from another faction costs additional movement.
        // (except for Neutral ships!)
        if (faction != incomingShip.faction && faction != Faction.Neutral
            && incomingShip.faction != Faction.Neutral)
        {
            totalMoveCost++;
        }


        // - Minimum

        // Allow an adjacent ship to move to this tile by spending all of its movement,
        // if it would not be able to move here otherwise.
        // (except for Void tiles)
        if (Utility.Distance(this, incomingShip.currentTile) == 1 &&
            incomingShip.movementRemaining == incomingShip.speed &&
            totalMoveCost > incomingShip.speed &&
            myType != TileType.Void)
        {
            totalMoveCost = incomingShip.speed;
        }


        // - Ships

        // Block enemy ships
        if (ship != null && ship.faction != incomingShip.faction)
        {
            totalMoveCost = 100;
        }

        // Return!
        return totalMoveCost;
    }

    // Get the vision cost of a tile.
    public int GetVisionCost(Ship beholder)
    {
        // Default
        int totalVisionCost = visionCost;

        // - Minimum

        // Allow an adjacent ship to see this tile, even if otherwise it would exceed their vision
        // unless their vision is 0.
        if (Utility.Distance(this, beholder.currentTile) == 1 &&
            totalVisionCost > beholder.vision &&
            beholder.vision > 0)
        {
            totalVisionCost = beholder.vision;
        }

        // Return.
        return totalVisionCost;
    }

    // Get this tile's armor bonus.
    // (A bit odd looking on its own, perhaps, but it's to match the movement above!)
    public int GetArmorBonus()
    {
        // Default.
        int totalArmorBonus = armorBonus;

        // Return!
        return totalArmorBonus;
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
                    vantagePoint = GM.I.selectedTile.ship.FindVantagePoint(this);
                    
                    // Check if we found a vantage point.
                    if (vantagePoint != null)
                    {
                        // Check if we're already in range (vantage point is our current tile)
                        if (vantagePoint != GM.I.selectedTile)
                        {
                            // Show the path to the vantage point
                            vantagePoint.ShowPathPreview();
                        }

                        // Show attack preview.
                        PreviewAttack(vantagePoint);
                    }
                }
            }
            else
            {
                // Normal movement preview
                ShowPathPreview();
            }
        }

        // Update cursor.
        CursorManager.I.UpdateCursor();

        // UI.
        UI.I.HoverTile(this);
    }

    // Unhovering returns background opacity as it was.
    public void Unhover()
    {
        // Avoid unhovering the selected tile.
        if (this == GM.I.selectedTile) return;

        // Reset cursor.
        CursorManager.I.SetDefaultCursor();
        
        // Reset normal opacity.
        Color c = bg.color;
        c.a = unhoveredOpacity;
        bg.color = c;

        // Clear the preview of a path to this tile, if there was one.
        ClearPathPreview();
        ClearAttackPreview();

        // Clear attack path preview if one exists
        if (vantagePoint != null)
        {
            vantagePoint.ClearPathPreview();
            vantagePoint = null;
        }

        // Clear hover tooltip?
        if (GM.I.hoveredTile == null)
            UI.I.HoverTile(null);
    }



    // Set this tile's faction.
    public void Claim(Faction newFaction, bool claimAllTilesInPath = true)
    {
        // Void tiles can't be claimed.
        if (myType == TileType.Void) return;
        
        // Set faction.
        faction = newFaction;

        // - Set faction color.
        factionBG.color = Constance.FactionColor(newFaction, 0.5f);

        // Claim other tiles in path!
        if (claimAllTilesInPath && previousTileInPath != null)
            previousTileInPath.Claim(newFaction);

        // Update leaderboard.
        Leaderboard.I.UpdateLeaderboard();
    }

    

    // ---  Path previews

    // - Movement paths
    private LineRenderer pathLine;

    public void ShowPathPreview()
    {
        // Get selected ship.
        Ship selectedShip = GM.I.selectedTile.ship;
        if (selectedShip == null) return;

        // Make sure path line does not already exist.
        ClearPathPreview();

        // Create line renderer.
        pathLine = gameObject.AddComponent<LineRenderer>();
        pathLine.startWidth = 0.02f;
        pathLine.endWidth = 0.02f;
        pathLine.material = new Material(Shader.Find("Sprites/Default"));
        pathLine.startColor = Constance.FactionColor(selectedShip.faction);
        pathLine.endColor = Constance.FactionColor(selectedShip.faction);
        pathLine.sortingOrder = 1000;

        // Make sure we can actually move here.
        if (moveCostFromCurrentTile < 0 || moveCostFromCurrentTile > GM.I.selectedTile.ship.speed || ship != null)
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

    // Clear the movement path preview, if extant.
    public void ClearPathPreview()
    {
        if (pathLine != null)
        {
            // Destroy the path immediately!
            DestroyImmediate(pathLine);

            // Note: This is more obvious but does not work!
            // Because Unity bravely does not allow such nonsense.
            // pathLine.enabled = false;
        }
    }

    // - Attack paths

    // Whichever tile is currently being used as a vantage point (if any).
    private static Tile vantagePoint;

    // The line renderer used to display attack previews.
    private LineRenderer attackLine;

    // Display an attack preview from the vantage point to the current tile.
    public void PreviewAttack(Tile vantagePoint)
    {
        // Create attack line renderer if needed
        if (attackLine == null)
        {
            attackLine = gameObject.AddComponent<LineRenderer>();
            attackLine.startWidth = 0.02f;
            attackLine.endWidth = 0.01f;
            attackLine.material = new Material(Shader.Find("Sprites/Default"));
            attackLine.startColor = Color.red;
            attackLine.endColor = Color.red;
            attackLine.sortingOrder = 1001;
        }

        // Draw straight line from vantage point to this tile
        Vector3 startPos = vantagePoint.transform.position;
        startPos.z = -0.5f;
        
        Vector3 endPos = transform.position;
        endPos.z = -0.5f;

        attackLine.positionCount = 2;
        attackLine.SetPositions(new Vector3[] { startPos, endPos });
        attackLine.enabled = true;
    }

    // Clear the attack preview, if extant.
    public void ClearAttackPreview()
    {
        if (attackLine != null)
        {
            // Destroy the preview immediately!
            DestroyImmediate(attackLine);
            // attackLine.enabled = false;
        }
    }

    // - Fog of War

    // Hide this tile in fog of war.
    public void HideInFog()
    {
        isVisibleToPlayer = false;
        fogOverlay.color = new Color(0, 0, 0, 1f);
    }

    // Reveal this tile from fog of war.
    public void RevealFromFog()
    {
        isVisibleToPlayer = true;
        fogOverlay.color = new Color(0, 0, 0, 0f);
    }

    // - Terraforming

    // Set this tile's type and update its stats and visuals accordingly
    public void Terraform(TileType newType)
    {
        // Get our progenitor.
        Tile p = SpawnManager.I.p_Void;
        if (newType == TileType.Air) p = SpawnManager.I.p_Air;
        if (newType == TileType.Water) p = SpawnManager.I.p_Water;
        if (newType == TileType.Fire) p = SpawnManager.I.p_Fire;
        if (newType == TileType.Asteroids) p = SpawnManager.I.p_Asteroids;
        if (newType == TileType.Planet) p = SpawnManager.I.p_Planet;

        // Load stats
        myType = newType;
        moveCost = p.moveCost;
        visionCost = p.visionCost;
        armorBonus = p.armorBonus;
        damageOnEnter = p.damageOnEnter;
        damageOnUpkeep = p.damageOnUpkeep;

        // Load image.
        string fileName = "Tiles/Tile - " + myType.ToString();
        Utility.LoadImage(typeImage, fileName);

        // Set opacity.
        // (cause Void tiles are clear!)
        Utility.SetOpacity(typeImage, p.typeImage.color.a);
        Utility.SetOpacity(factionBG, p.factionBG.color.a);
        Utility.SetOpacity(bg, p.bg.color.a);
        // typeImage.color = p.typeImage.color;
        // factionBG.color = p.factionBG.color;
        // bg.color = p.bg.color;
    }
}
