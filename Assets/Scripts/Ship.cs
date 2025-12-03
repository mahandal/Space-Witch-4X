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

    [Header("Core")]
    public int manaCost = 0;
    public float currentHealth;
    public int maxHealth;
    public int damage;
    public int armor;
    public int speed;
    public int range;
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

    // The outline behidn this ship's attacks remaining text indicator.
    // Also the parent object of the attacks remaining text indicator.
    public Image attacksRemainingOutline;

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

        // Find total damage dealt.
        int totalArmor = target.armor + target.currentTile.GetArmorBonus();
        int totalDamage = damage - totalArmor;

        // Deal damage.
        target.ReceiveDamage(totalDamage, this);
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

        // Update health bar.
        healthBar.fillAmount = currentHealth / maxHealth;

        // Log it!
        if (attacker != null)
            Debug.Log(attacker.myName + " attacked " + myName + " for " + incomingDamage + " damage!");
        else
            Debug.Log(myName + " lost " + incomingDamage + " health!");
    }

    // Handle 'death'.
    // Note: When you 'die' by the hand of the Coven, you are reborn anew!
    // Note 2: Other faction mechanics are handled here also!
    public void Death(Ship killer = null)
    {
        // - Faction mechanics

        // Swarm
        if (killer.faction == Faction.Swarm)
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
            GM.I.leaders[Faction.Syndicate].mana += manaCost;
        }

        // - Clean up.
        // (Unless we were recruited!)
        bool wasRecruited = (killer != null && killer.faction == Faction.Coven);
        if (!wasRecruited)
        {
            // Get leader.
            Leader leader = GM.I.leaders[faction];

            // Remove from leader's fleet.
            leader.fleet.Remove(this);

            // Clean up object.
            Object.Destroy(gameObject);
        }
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
    }

    // Handle upkeep for this ship.
    // E.g. burning in fire!
    // Should be called once at the beginning of each turn, by this ship's leader.
    public void Upkeep()
    {
        // Damage!
        ReceiveDamage(currentTile.damageOnUpkeep);
    }
}
