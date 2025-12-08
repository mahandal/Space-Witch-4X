using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class Ship : MonoBehaviour
{
    [Header("Meta")]
    public string myName;
    public Faction faction;
    public int x;
    public int y;
    public Tile currentTile;

    [Header("Traits")]
    // A list of this ship's special traits.
    public List<string> traits = new List<string>();

    [Header("Core")]
    public int manaCost = 0;
    public float currentHealth;
    public int maxHealth;
    public int damage;
    public int armor;
    public int speed;
    public int range;
    public int vision;

    [Header("Secret / Special")]
    public int attacks = 1;

    [Header("Per turn")]
    public int movementRemaining;
    public int attacksRemaining;

    [Header("Manual Machinery")]
    // This ship's sprite renderer.
    public SpriteRenderer sr;

    // This ship's health bar.
    public Image healthBar;

    // This ship's movement remaining text indicator.
    public TMP_Text movementRemainingText;

    // The outline behind this ship's movement remaining text indicator.
    // Also the parent object of the movement remaining text indicator.
    public Image movementRemainingOutline;

    // This ship's attacks remaining text indicator.
    public TMP_Text attacksRemainingText;

    // The outline behind this ship's attacks remaining text indicator.
    // Also the parent object of the attacks remaining text indicator.
    public Image attacksRemainingOutline;

    // The image showing this ship's allegiance.
    public Image factionIcon;

    // Attempt to move into range and attack the target ship.
    // Fails if:
    // - We don't have any attacks remaining.
    // - It's not our turn.
    // - The target doesn't exist.
    // - The target is not an enemy.
    // - We can't get close enough.
    public bool AttemptAttackMove(Ship target)
    {
        // Check if we have attacks remaining.
        if (attacksRemaining <= 0)
            return false;

        // Check if it's our turn.
        if (GM.I.activeFaction != faction)
            return false;

        // Make sure target exists.
        if (target == null) return false;

        // Make sure target is an enemy.
        if (target.faction == faction) return false;

        // Get the target's tile.
        Tile targetTile = target.currentTile;

        // Find the closest tile to us that we can attack our target from.
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

    // Find the closest tile to us that we can attack our target from.
    public Tile FindVantagePoint(Tile targetTile)
    {
        // Measure distance apart.
        int distance = Utility.Distance(currentTile, targetTile);

        // Check if we're in range to attack them already.
        if (distance <= range)
        {
            // Done!
            return currentTile;
        }

        // - Not in range, have to move closer.

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

    // Attempt to move the ship to the target tile.
    public bool AttemptMove(Tile targetTile)
    {
        return AttemptMove(targetTile.x, targetTile.y);
    }

    // Attempt to move the ship to the new coordinates.
    // Fails if
    // - it's not our turn.
    // - the tile is too far away.
    // - there is already a ship there.
    public bool AttemptMove(int newX, int newY)
    {
        // Check if it's our turn.
        if (GM.I.activeFaction != faction)
            return false;

        // Get new tile.
        Tile newTile = GM.I.grid[newX, newY];

        // Check if tile is empty.
        if (newTile.ship != null)
            return false;

        // Check if tile is too far away.
        if (newTile.moveCostFromSelectedTile > movementRemaining || newTile.moveCostFromSelectedTile < 0)
            return false;

        // Delegate to Move!
        Move(newX, newY);

        // Return true!
        return true;
    }

    // Consume one of this ship's attacks to deal damage to a target ship.
    // Note: Does NOT error check!
    public void Attack(Ship target)
    {
        // Spend attack.
        SpendAttack();

        // - Find total damage dealt.

        // Start with the attacker's damage.
        float totalDamage = damage;

        // Add 10% per adjacent friendly tile.
        totalDamage *= 1 + (0.1f * CountFriendlyAdjacentTiles());

        int totalArmor = target.armor + target.currentTile.GetArmorBonus();
        totalDamage -= totalArmor;

        // Do a minimum of 1 damage.
        if (totalDamage < 1)
            totalDamage = 1;

        // Deal damage.
        target.ReceiveDamage((int)totalDamage, this);
    }

    // Count how many tiles next to us we own.
    // Note: For this context, the tile you are on is considered adjacent.
    public int CountFriendlyAdjacentTiles()
    {
        // Initialize count.
        int count = 0;

        // Loop through each tile.
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                // Get tile.
                Tile tile = Utility.GetTile(x + i, y + j);

                // Add to count, if tile exists and is friendly.
                if (tile != null && tile.faction == faction)
                    count++;
            }
        }

        // Return!
        return count;
    }

    // Receive damage.
    public void ReceiveDamage(int incomingDamage, Ship attacker = null)
    {
        // Ignore 0 damage.
        if (incomingDamage == 0) return;

        // Lose health
        currentHealth -= incomingDamage;

        // Check death?
        if (currentHealth <= 0)
            Death(attacker);

        // Cap max hp, just in case!
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        // Update health bar.
        healthBar.fillAmount = currentHealth / maxHealth;


        // Update tooltip?
        if (GM.I.hoveredTile != null && GM.I.hoveredTile.ship == this)
            UI.I.HoverTile(GM.I.hoveredTile);

        // Log it!
        if (attacker != null)
            Debug.Log(attacker.myName + " attacked " + myName + " for " + incomingDamage + " damage!");
        else
            Debug.Log(myName + " lost " + incomingDamage + " health!");
    }

    // Gain health.
    public void GainHealth(int incomingHealing, Ship healer = null)
    {
        // Ignore 0 healing.
        if (incomingHealing == 0) return;

        // Gain health.
        currentHealth += incomingHealing;

        // Cap max hp.
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        // Update health bar.
        healthBar.fillAmount = currentHealth / maxHealth;

        // Log it!
        if (healer == null || healer == this)
            Debug.Log(myName + " is healing herself for " + incomingHealing + " health!");
        else
            Debug.Log(myName + " is being healed by " + healer.myName + " for " + incomingHealing + " health!");
    }

    // Handle 'death'.
    // Note: When you 'die' by the hand of the Coven, you are reborn anew!
    // Note 2: Other faction mechanics are handled here also!
    public void Death(Ship killer = null)
    {
        // Remember our old leader.
        Leader oldLeader = GM.I.leaders[faction];

        // Remember our old faction.
        // (in case we convert!)
        Faction oldFaction = faction;

        // - Faction mechanics

        // null
        if (killer == null)
        {
            Debug.Log("Killed by nothing! What a way to go...");
        }
        // Pack
        else if (killer.faction == Faction.Pack)
        {
            // Consume EVERYTHING
            killer.Refresh();
        }
        // Coven
        else if (killer.faction == Faction.Coven)
        {
            // Make Love Not War!
            Convert(Faction.Coven);
        }
        // Syndicate
        else if (killer.faction == Faction.Syndicate)
        {
            // War Profiteers
            GM.I.leaders[Faction.Syndicate].GainMana(manaCost);
        }

        // Check if we were recruited!
        bool wasRecruited = (killer != null && killer.faction == Faction.Coven);

        // Get leader.
        // Leader leader = GM.I.leaders[faction];

        // Remove from old leader's fleet.
        if (!wasRecruited)
            oldLeader.fleet.Remove(this);

        // Check if we were the leader.
        if (oldLeader == this)
        {
            // Remove from UN.
            // GM.I.leaders.Remove(faction);
            GM.I.leaders[oldFaction] = null;

            // Our fleet abandons the fight!
            if (!wasRecruited)
                oldLeader.AbandonFleet();

            // Check if the player just lost.
            GM.I.CheckDefeat();

            // Check if the player just won!
            GM.I.CheckVictory();
        }

        // - Clean up.
        // (Unless we were recruited!)
        if (wasRecruited) return;
            
        // Clean up object.
        Object.Destroy(gameObject);
    }

    // Move the ship to new coordinates.
    // Note: Does NOT error check!
    public void Move(int newX, int newY, bool costMovement = true)
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

        // Spend movement.
        if (costMovement)
            SpendMovement(newTile.moveCostFromSelectedTile);

        // Update fog of war if player faction
        if (faction == GM.I.playerFaction)
            GM.I.UpdateFogOfWar();

        // Call tile's OnEnter function.
        newTile.OnEnter(this);
    }

    // Refreshes this ship's movement and attacks.
    // Should be called once at the beginning of each turn.
    public void Refresh()
    {
        // Refresh movement.
        SetMovementRemaining(speed);

        // Refresh attacks.
        SetAttacksRemaining(attacks);
    }

    // Spend the given amount of movement.
    public void SpendMovement(int movementSpent)
    {
        // Decrease moves remaining.
        movementRemaining -= movementSpent;

        // Minimum of 0.
        // (dunno if this is necessary?)
        if (movementRemaining < 0)
            movementRemaining = 0;

        // Set text.
        movementRemainingText.text = movementRemaining.ToString();

        // Consider greying ourselves out to show we are done for the turn.
        ConsiderGoingGrey();
    }

    // Set remaining movement to the given value.
    public void SetMovementRemaining(int _movementRemaining)
    {
        // Set new movement remaining.
        movementRemaining = _movementRemaining;

        // Set text.
        movementRemainingText.text = movementRemaining.ToString();

        // Consider greying ourselves out to show we are done for the turn.
        ConsiderGoingGrey();
    }

    // Spend an attack.
    public void SpendAttack()
    {
        // Decrease attacks remaining.
        attacksRemaining--;

        // Set text.
        attacksRemainingText.text = attacksRemaining.ToString();

        // Consider greying ourselves out to show we are done for the turn.
        ConsiderGoingGrey();
    }

    // Set attacks remaining.
    public void SetAttacksRemaining(int _attacksRemaining)
    {
        // Set new attacks remaining.
        attacksRemaining = _attacksRemaining;

        // Set text.
        attacksRemainingText.text = attacksRemaining.ToString();

        // Consider greying ourselves out to show we are done for the turn.
        ConsiderGoingGrey();
    }

    // Consider greying ourselves out to show we are done for the turn.
    // To qualify for going grey, a ship must:
    // - Have no movement remaining.
    // - Have no attacks remaining.
    // If a ship has none of one but some of the other,
    // the one with none goes away but the other stays and the ship doesn't go grey.
    public void ConsiderGoingGrey()
    {
        // Check if we have movement remaining.
        if (movementRemaining <= 0)
        {
            // Hide movement remaining.
            movementRemainingOutline.gameObject.SetActive(false);
        } else {
            // Reveal movement remaining.
            movementRemainingOutline.gameObject.SetActive(true);
        }

        // Check if we have attacks remaining.
        if (attacksRemaining <= 0)
        {
            // Hide attacks remaining.
            attacksRemainingOutline.gameObject.SetActive(false);
        } else {
            // Reveal attacks remaining.
            attacksRemainingOutline.gameObject.SetActive(true);
        }

        // Check if we have movement or attacks remaining.
        if (movementRemaining > 0 || attacksRemaining > 0)
        {
            // Make sure we aren't grey!
            UnGrey();
        } else {
            // Go grey!
            GoGrey();
        }

        // See if we should suggest ending the turn.
        UI.I.WhichButtonInTopRight();
    }

    // Grey yourself out!
    public void GoGrey()
    {
        sr.color = new Color(0.5f, 0.5f, 0.5f, 0.9f);
    }

    // Ungrey yourself!
    public void UnGrey()
    {
        sr.color = new Color(1f, 1f, 1f, 1f);
    }

    // Hide movement and attack remaining indicators.
    // Should be called once at the end of each turn.
    public void HideMovementAndAttacks()
    {
        // Hide movement and attacks.
        movementRemainingOutline.gameObject.SetActive(false);
        attacksRemainingOutline.gameObject.SetActive(false);
    }

    // Reveal movement and attack remaining indicators.
    // Should be called once at the beginning of each turn.
    public void RevealMovementAndAttacks()
    {
        // Reveal movement and attacks.
        movementRemainingOutline.gameObject.SetActive(true);
        attacksRemainingOutline.gameObject.SetActive(true);
    }


    // Convert to the given faction.
    // Also claims the tile this ship is on.
    // If shouldFullHeal is true, which it is by default,
    // then it also fully heals the ship, though draining it of all movement and attacks in the process.
    public void Convert(Faction newFaction, bool shouldFullHeal = true)
    {
        // Check if we are the leader of our faction.
        Leader thisAsLeader = this as Leader;
        if (thisAsLeader != null)
        {
            // Convert all of our fleet.
            foreach (Ship ship in thisAsLeader.fleet)
            {
                // Don't loop infinitely on ourself!
                if (ship != this)
                    ship.Convert(newFaction);
            }
        } else {
            // Get leader of old faction.
            Leader oldLeader = GM.I.leaders[faction];

            // Remove from old faction leader's fleet.
            oldLeader.fleet.Remove(this);
        }

        // Set new faction.
        faction = newFaction;

        // Get new faction leader.
        Leader newLeader = GM.I.leaders[newFaction];

        // Add to new faction leader's fleet.
        newLeader.fleet.Add(this);

        // Claim tile.
        currentTile.Claim(newFaction);

        // Check that we should also full heal.
        if (shouldFullHeal)
        {
            // Heal to full health.
            currentHealth = maxHealth;

            // Drain of movement.
            SetMovementRemaining(0);

            // Drain of attacks.
            SetAttacksRemaining(0);
        }

        // Update health bar to reflect you have changed teams.
        healthBar.color = Constance.FactionColor(newFaction, 1f);

        // Update health bar to show you have fully healed.
        healthBar.fillAmount = currentHealth / maxHealth;

        // Update faction icon.
        string factionIconName = "Faction Icon - " + newFaction.ToString();
        Utility.LoadImage(factionIcon, factionIconName);

        // Update fog of war.
        GM.I.UpdateFogOfWar();
    }

    // Handle upkeep for this ship.
    // E.g. burning in fire!
    // Should be called once at the beginning of each turn, by this ship's leader.
    public void Upkeep()
    {
        // Damage!
        ReceiveDamage(currentTile.damageOnUpkeep);

        // Reveal movement and attacks remaining.
        RevealMovementAndAttacks();
    }

    // Attempt to rest.
    // Delegates to Rest() if successful.
    // Fails if:
    // - It is not our turn.
    // - This ship has no movement or attacks remaining.
    public bool AttemptRest()
    {
        // Check if it is our turn.
        if (faction != GM.I.activeFaction) return false;

        // Check if we have no movement or attacks remaining.
        if (movementRemaining <= 0 && attacksRemaining <= 0) return false;

        // Delegate to Rest();
        Rest();

        // Return successful!
        return true;
    }

    // Rest.
    // Consume all remaining movement and attacks to regain health.
    // Regain up to 29% of max health in total:
    // - Regain up to 10% of max health if you are resting on a planet.
    // - Regain up to 5% of max health with all movement remaining.
    // - Regain up to 5% of max health with all attacks remaining.
    // - Regain 1% of max health per adjacent friendly tile (including this one!)
    public void Rest()
    {
        // Initialize our count of how much health we'll heal.
        float percentMaxHealthToHeal = 0f;

        // Check if we're on a planet.
        if (currentTile.myType == TileType.Planet)
            percentMaxHealthToHeal += 0.1f;

        // Get percentage of movement remaining.
        float percentMovementRemaining = movementRemaining / speed;

        // Multiply by 5% and add to total heal amount.
        percentMaxHealthToHeal += percentMovementRemaining * 0.05f;

        // Get percentage of attacks remaining.
        float percentAttacksRemaining = attacksRemaining / attacks;

        // Multiply by 5% and add to total heal amount.
        percentMaxHealthToHeal += percentAttacksRemaining * 0.05f;

        // Go through each adjacent tile:
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                // Get tile.
                // Tile adjacentTile = GM.I.grid[x + i, y + j];
                Tile adjacentTile = GM.I.GetTile(x + i, y + j);

                // Check tile faction.
                if (adjacentTile != null && adjacentTile.faction == faction)
                    percentMaxHealthToHeal += 0.01f; // Heal 1% per friendly adjacent tile.
            }
        }

        // Get total health recovered.
        int totalHealthRecovered = (int)(percentMaxHealthToHeal * maxHealth);

        // Heal up!
        GainHealth(totalHealthRecovered, this);

        // Consume remaining movement.
        SetMovementRemaining(0);

        // Consume remaining attacks.
        SetAttacksRemaining(0);

        // Update hover tooltip.
        currentTile.Hover();
    }


    // Get all enemy ships within vision range.
    public List<Ship> GetVisibleEnemies()
    {
        List<Ship> visibleEnemies = new List<Ship>();
        
        // Check all tiles within vision range
        for (int dx = -vision; dx <= vision; dx++)
        {
            for (int dy = -vision; dy <= vision; dy++)
            {
                // Get the tile
                Tile tile = GM.I.GetTile(x + dx, y + dy);
                
                // Skip if tile doesn't exist
                if (tile == null) continue;
                
                // Skip if no ship on tile
                if (tile.ship == null) continue;
                
                // Skip if ship is friendly
                if (tile.ship.faction == faction) continue;
                
                // Calculate actual distance
                int distance = Utility.Distance(currentTile, tile);
                
                // Check if within vision range
                if (distance <= vision)
                {
                    visibleEnemies.Add(tile.ship);
                }
            }
        }
        
        return visibleEnemies;
    }


    // Move this ship in a random direction.
    public void MoveShipRandomly()
    {
        // Skip if no movement remaining
        if (movementRemaining <= 0) return;
        
        // Get all tiles this ship can move to
        HashSet<Tile> moveableTiles = currentTile.GetTilesInMovementRange();
        
        // Filter out tiles with ships on them
        List<Tile> validTiles = new List<Tile>();
        foreach (Tile tile in moveableTiles)
        {
            if (tile.ship == null)
                validTiles.Add(tile);
        }
        
        // Check if we have any valid tiles
        if (validTiles.Count == 0) return;
        
        // Pick a random tile
        int randomIndex = Random.Range(0, validTiles.Count);
        Tile randomTile = validTiles[randomIndex];
        
        // Move there
        AttemptMove(randomTile.x, randomTile.y);
    }

     // Move a ship toward a target tile.
    public void MoveToward(Tile targetTile)
    {
        // Skip if no movement remaining
        if (movementRemaining <= 0) return;
        
        // Find a path to the target tile.
        List<Tile> path = FindPathTo(targetTile);

        // Check if we found a path
        if (path == null) return;

        // Move along path until we are out of movement or we reach our target.
        int pathIndex = 1;
        while (movementRemaining > 0 && currentTile != targetTile)
        {
            // Get the next tile in our path.
            Tile nextTile = path[pathIndex];

            // Move to the next tile.
            AttemptMove(nextTile);

            // Increment our path index.
            pathIndex++;

            // Check if we've reached the end?
            if (pathIndex == path.Count)
                return;
        }
    }

    // Find the shortest path from this ship's current tile to the target tile.
    // Returns null if no path exists.
    // Uses Dijkstra's algorithm.
    public List<Tile> FindPathTo(Tile targetTile)
    {
        if (targetTile == null) return null;
        
        // Track tiles we've visited and the cost to reach them
        Dictionary<Tile, int> costToReach = new Dictionary<Tile, int>();
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        
        // Priority queue: store tiles with their costs, always process lowest cost first
        List<(Tile tile, int cost)> frontier = new List<(Tile, int)>();
        
        // Start from current tile
        frontier.Add((currentTile, 0));
        costToReach[currentTile] = 0;
        cameFrom[currentTile] = null;
        
        while (frontier.Count > 0)
        {
            // Get the tile with lowest cost (priority queue behavior)
            frontier.Sort((a, b) => a.cost.CompareTo(b.cost));
            var current = frontier[0];
            frontier.RemoveAt(0);
            
            // Found the target!
            if (current.tile == targetTile)
            {
                // Reconstruct path
                List<Tile> path = new List<Tile>();
                Tile step = targetTile;
                while (step != null)
                {
                    path.Add(step);
                    step = cameFrom[step];
                }
                path.Reverse();
                return path;
            }
            
            // Check all neighbors
            List<Tile> neighbors = new List<Tile>();
            if (current.tile.x > 0) neighbors.Add(GM.I.grid[current.tile.x - 1, current.tile.y]);
            if (current.tile.x < GM.I.gridWidth - 1) neighbors.Add(GM.I.grid[current.tile.x + 1, current.tile.y]);
            if (current.tile.y > 0) neighbors.Add(GM.I.grid[current.tile.x, current.tile.y - 1]);
            if (current.tile.y < GM.I.gridHeight - 1) neighbors.Add(GM.I.grid[current.tile.x, current.tile.y + 1]);
            
            foreach (Tile neighbor in neighbors)
            {
                int moveCost = neighbor.GetMovementCost(this);
                
                int newCost = costToReach[current.tile] + moveCost;
                
                if (!costToReach.ContainsKey(neighbor) || newCost < costToReach[neighbor])
                {
                    costToReach[neighbor] = newCost;
                    cameFrom[neighbor] = current.tile;
                    frontier.Add((neighbor, newCost));
                }
            }
        }
        
        // No path found
        return null;
    }
}
