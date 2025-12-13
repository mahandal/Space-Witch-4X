using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public partial class Ship : MonoBehaviour
{
    [Header("Meta")]
    public string myName;
    public Faction faction;
    public int x;
    public int y;
    public Tile currentTile;

    [Header("Traits")]
    // A list of this ship's special traits.
    public List<Trait> traits = new List<Trait>();

    [Header("Core")]
    public int manaCost = 0;
    public int maxHealth;
    public int damage;
    public int armor;
    public int speed;
    public int range;
    public int vision;

    [Header("Secret / Special")]
    public int attacks = 1;

    [Header("Live")]
    public float currentHealth;
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
    public IEnumerator AttemptAttackMove(Ship target, bool useAllAttacks = false)
    {
        // Check if we have attacks remaining.
        if (attacksRemaining <= 0)
            yield break;

        // Check if it's our turn.
        if (GM.I.activeFaction != faction)
            yield break;

        // Make sure target exists.
        if (target == null) yield break;

        // Make sure target is an enemy.
        if (target.faction == faction) yield break;

        // Get the target's tile.
        Tile targetTile = target.currentTile;

        // Find the closest tile to us that we can attack our target from.
        Tile bestTile = FindVantagePoint(targetTile);

        // Check if we found a tile.
        if (bestTile == null)
        {
            // Can't get close enough.
            yield break;
        }

        // Move to our vantage point.
        yield return Move(bestTile);

        // Attack!
        Attack(target);

        // Use additional attacks?
        if (useAllAttacks)
        {
            yield return FireEverything(target);
        }
    }

    // FIRE EVERYTHING!
    public IEnumerator FireEverything(Ship target)
    {
        // Loop until all attacks are used or target is destroyed (or converted!).
        while (attacksRemaining > 0 && target.currentHealth > 0 && target.faction != faction)
        {
            // First, wait a bit to space out each attack.
            yield return new WaitForSeconds(0.2f);

            // Attack!
            Attack(target);
        }
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
            // TBD: Improve?
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
    // - it's not our turn.
    // - the tile is too far away.
    // - there is already a ship there.
    public IEnumerator AttemptMove(Tile destination)
    {
        // Check if it's our turn.
        if (GM.I.activeFaction != faction)
        {
            Debug.Log(myName + " failed to move. Not our turn!");
            yield break;
        }

        // Check if tile is empty.
        if (destination.ship != null)
        {
            Debug.Log(myName + " failed to move. Target tile has a ship already!");
            yield break;
        }

        // Check if tile is too far away.
        if (destination.moveCostFromCurrentTile > movementRemaining || destination.moveCostFromCurrentTile < 0)
        {
            Debug.Log(myName + " failed to move. Not enough movement remaining!"
                + " movement remaining: " + movementRemaining
                + ". tile movement cost: " + destination.moveCostFromCurrentTile);
            yield break;
        }

        // Delegate to Move!
        yield return Move(destination);
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
            // Don't need to do anything here, just avoiding triggering faction abilities.
            Debug.Log("Killed by nothing! What a way to go...");
        }
        // Suicide
        else if (killer == this)
        {
            // Don't need to do anything here, just avoiding triggering faction abilities.
            Debug.Log("Suicide is badass!");
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

        // Reveal end turn button if our last ship just died.
        UI.I.WhichButtonInTopRight();
    }


    // Move the ship to a destination tile.
    // Note: Does NOT error check!
    public IEnumerator Move(Tile destination, bool costMovement = true)
    {
        // Remove from old tile.
        if (currentTile != null)
            currentTile.ship = null;

        // Set coordinates.
        x = destination.x;
        y = destination.y;

        // Set new tile
        currentTile = destination;
        currentTile.ship = this;

        // Claim for your faction!
        destination.Claim(faction);

        // Hide path preview.
        destination.ClearPathPreview();

        // Spend movement.
        if (costMovement)
        {
            SpendMovement(destination.moveCostFromCurrentTile);

            // Move physically
            yield return PhysicallyMove(destination);
        } else {
            // Teleport.
            transform.position = destination.transform.position;
        }

        // Update fog of war if player faction
        if (faction == GM.I.playerFaction)
            GM.I.UpdateFogOfWar();

        // Call tile's OnEnter function.
        destination.OnEnter(this);
    }

    // Physically move this ship from its current tile to the destination tile.
    public IEnumerator PhysicallyMove(Tile destination)
    {
        // Remember our starting position.
        Vector3 startPosition = transform.position;

        // Remember the end position just so it's easier to read.
        Vector3 endPosition = destination.transform.position;

        // Animate the movement
        float elapsed = 0f;
        float duration = 0.3f;
        
        // Last duration seconds.
        while (elapsed < duration)
        {
            // Count time.
            elapsed += Time.deltaTime;

            // Get percent complete.
            float t = elapsed / duration;

            // Set position using lerp.
            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            // Move camera to follow.
            Utility.MoveCamera(this);

            // Wait a frame.
            yield return null;
        }

        // Ensure we end exactly at the target position
        transform.position = endPosition;
    }

    // Refreshes this ship's movement and attacks.
    // Should be called once at the end of each turn.
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

            // Make a copy of our fleet so we don't edit what we're iterating over.
            List<Ship> fleetCopy = new List<Ship>(thisAsLeader.fleet);
            foreach (Ship ship in fleetCopy)
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
        // Tile damage!
        ReceiveDamage(currentTile.damageOnUpkeep);

        // Reveal movement and attacks remaining.
        RevealMovementAndAttacks();

        // Wake up from guard mode?
        if (autoPilotMode == AutoPilotMode.Guard)
            Guard();

        // Wake up from rest mode?
        if (autoPilotMode == AutoPilotMode.Rest && currentHealth >= maxHealth)
            SetAutoPilot(AutoPilotMode.Off);
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
    // Minimum of 1.
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

        // Minimum of 1.
        if (totalHealthRecovered < 1)
            totalHealthRecovered = 1;

        // Heal up!
        GainHealth(totalHealthRecovered, this);

        // Consume remaining movement.
        SetMovementRemaining(0);

        // Consume remaining attacks.
        SetAttacksRemaining(0);

        // Update hover tooltip.
        currentTile.Hover();
    }


    // Get a list of all enemy ships within vision range.
    public List<Ship> GetVisibleEnemies()
    {
        // Get a list of all tiles this ship can see.
        HashSet<Tile> visibleTiles = currentTile.GetTilesInVisionRange();

        // Keep a list of all enemies in this ship's vision range.
        List<Ship> visibleEnemies = new List<Ship>();

        // Look through each tile.
        foreach (Tile tile in visibleTiles)
        {
            // Get the ship on this tile (if there is one).
            Ship ship = tile.ship;

            // If there is an enemy ship, add it to the list.
            if (ship != null && ship.faction != faction)
                visibleEnemies.Add(ship);
        }

        // Return.
        return visibleEnemies;
    }

    // Get a list of all enemy ships within attack range.
    public List<Ship> GetEnemiesInAttackRange()
    {
        List<Ship> attackableEnemies = new List<Ship>();
        
        // Check all tiles within vision range.
        for (int dx = -vision; dx <= vision; dx++)
        {
            for (int dy = -vision; dy <= vision; dy++)
            {
                // Get the tile.
                Tile tile = GM.I.GetTile(x + dx, y + dy);
                
                // Skip if tile doesn't exist.
                if (tile == null) continue;
                
                // Skip if no ship on tile.
                if (tile.ship == null) continue;
                
                // Skip if ship is friendly.
                if (tile.ship.faction == faction) continue;
                
                // Calculate actual distance.
                int distance = Utility.Distance(currentTile, tile);
                
                // Check if within attack range.
                if (distance <= range)
                {
                    attackableEnemies.Add(tile.ship);
                }
            }
        }
        
        return attackableEnemies;
    }

     // Move a ship toward a target tile.
    public IEnumerator MoveToward(Tile targetTile)
    {
        Debug.Log(myName + " is attempting to move toward destination (" + 
            targetTile.x + ", " + targetTile.y + ").");

        // Skip if no movement remaining
        if (movementRemaining <= 0) yield break;
        
        // Find a path to the target tile.
        List<Tile> path = FindPathTo(targetTile);

        // Check if we found a path
        if (path == null) yield break;

        // Move along path until we are out of movement or we reach our target.
        int pathIndex = 1;
        while (movementRemaining > 0 && currentTile != targetTile)
        {
            // Get the next tile in our path.
            Tile destination = path[pathIndex];

            // Check if we need to move through friendly ships.
            while (destination.ship != null)
            {
                destination = destination.nextTileInPath;

                // Can't make it through!
                if (destination == null)
                    yield break;
            }

            // Move to the next tile.
            yield return AttemptMove(destination);

            // Follow this ship's progress.
            ShowVision();

            // Wait a moment on each tile.
            yield return new WaitForSeconds(0.3f);

            // Check if we moved successfully.
            bool successfullyMoved = (destination == currentTile);
            if (successfullyMoved)
                // Recalculate tile movement costs.
                // Note: Recalculates movement cost for ALL tiles. Could be optimized!
                currentTile.GetAllConnectedTiles();
            else
                yield break; // Return false if we fail to move for whatever reason.

            // Increment our path index.
            pathIndex++;

            // Check if we've reached the end?
            if (pathIndex >= path.Count)
                yield break;
        }

        Debug.Log(myName + " failed to reach destination (" + 
            targetTile.x + ", " + targetTile.y + ").");
    }

    // Find the shortest path from this ship's current tile to the target tile.
    // Returns null if no path exists.
    // Uses Dijkstra's algorithm.
    public List<Tile> FindPathTo(Tile targetTile)
    {
        if (targetTile == null) return null;

        // First clear all prior path traces.
        Tile.ClearAllPathTraces();
        
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
                    // Remember how we got here.
                    costToReach[neighbor] = newCost;
                    neighbor.moveCostFromCurrentTile = newCost;
                    cameFrom[neighbor] = current.tile;
                    neighbor.previousTileInPath = current.tile;
                    current.tile.nextTileInPath = neighbor;

                    // Add neighbor to frontier.
                    frontier.Add((neighbor, newCost));
                }
            }
        }

        // No path found
        return null;
    }

    // Get the percent of adjacent tiles that are friendly.
    public float GetPercentFriendlyAdjacentTiles()
    {
        // Get the number of friendly adjacent tiles.
        float friendlyTiles = (float)CountFriendlyAdjacentTiles();

        // Get the percent.
        return friendlyTiles / 9f;
    }

    // Find the nearest neutral tile.
    // Returns null if there are no neutral tiles remaining.
    public Tile FindNearestNeutralTile()
    {
        // Get a set of all tiles we can move to.
        HashSet<Tile> moveableTiles = currentTile.GetAllConnectedTiles();
        // HashSet<Tile> moveableTiles = currentTile.GetTilesInMovementRange();

        // Track the best tile we've seen so far.
        Tile bestTile = null;
        int closestDistance = int.MaxValue;

        // Loop through each tile.
        foreach (Tile tile in moveableTiles)
        {
            // Check tile is neutral.
            if (tile.faction != Faction.Neutral)
                continue;

            // Check tile is empty.
            if (tile.ship != null)
                continue;

            // Compare distance.
            if (tile.moveCostFromCurrentTile < closestDistance)
            {
                // Remember new best tile.
                bestTile = tile;
                closestDistance = tile.moveCostFromCurrentTile;
            }
        }

        // Return!
        return bestTile;
    }

    // Get the amount of mana that could be gained from recycling this ship as-is.
    public int GetRecycleValue()
    {
        // Get health percent
        float healthPercent = currentHealth / maxHealth;

        // Get tile multiplier.
        float tileMultiplier = 0.1f * CountFriendlyAdjacentTiles();

        // Get total mana returned.
        int manaReturned = (int)(manaCost * healthPercent * tileMultiplier);

        // Return!
        return manaReturned;
    }
}
