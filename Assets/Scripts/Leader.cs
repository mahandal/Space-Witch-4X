using UnityEngine;
using System.Collections.Generic;

// Your leader ship runs your fleet!
public class Leader : Ship
{
    [Header("Leader")]
    public int mana = 0;
    public HashSet<Ship> fleet = new HashSet<Ship>();

    // Initialize this leader:
    // - Move it to a random position.
    // - Heal it to full health.
    // - Refresh its movement and attacks.
    // - Reset mana to 0.
    // - Clear any previous ships in its fleet.
    public void Init()
    {
        // Move to a random starting location.
        MoveToStartingLocation();

        // Heal to full.
        currentHealth = maxHealth;

        // Refresh.
        Refresh();

        // Set mana to 0.
        mana = 0;

        // Clear any previous ships.
        foreach (Ship ship in fleet)
        {
            // Except yourself!
            if (ship != this)
            {
                Object.Destroy(ship);
            }
        }

        // Reset fleet list.
        fleet = new HashSet<Ship>();
        fleet.Add(this);

        // Set health bar color.
        healthBar.color = Constance.FactionColor(faction, 1f);

        // Start with movement and attacks hidden.
        HideMovementAndAttacks();
    }

    // Move this leader to a random position as its starting location.
    // Note: DOES spend movement so make sure to refresh after!
    public void MoveToStartingLocation()
    {
        // Get random coordinates.
        int spawnX = Random.Range(0, GM.I.gridWidth);
        int spawnY = Random.Range(0, GM.I.gridHeight);

        // Make sure coordinates are empty.
        while (GM.I.grid[spawnX, spawnY].ship != null)
        {
            spawnX = Random.Range(0, GM.I.gridWidth);
            spawnY = Random.Range(0, GM.I.gridHeight);
        }

        // Move!
        Move(spawnX, spawnY);
    }

    // Refresh all ships that fly under this leader's banner.
    // Called at the end of each turn.
    public void RefreshFleet()
    {
        // Loop through each ship in our fleet.
        foreach (Ship ship in fleet)
        {
            // Refresh!
            ship.Refresh();
        }
    }

    // Hide movement and attacks remaining for all ships in our fleet.
    // Called at the end of each turn.
    public void HideFleetMovementAndAttacks()
    {
        // Loop through ech ship in our fleet.
        foreach (Ship ship in fleet)
        {
            ship.HideMovementAndAttacks();
        }
    }

    // Handle ending a turn for this leader's faction.
    // - Refreshes all ships in our fleet.
    // - Hides movement and attacks remaining for all ships.
    // --- (Not hidden like a secret, just so you can see which faction is active currently easier)
    public void EndTurn()
    {
        // Refresh our fleet!
        RefreshFleet();

        // Hide movement and attacks remaining for all ships in our fleet.
        HideFleetMovementAndAttacks();
    }

    // Handle starting a turn for this leader's faction.
    // - Harvest 1 mana per tile owned.
    // - Handle upkeep for each ship (e.g. burning in fire!)
    public void StartTurn()
    {
        // Harvest mana.
        HarvestMana();

        // Handle upkeep for each ship in our fleet!
        foreach (Ship ship in fleet)
        {
            ship.Upkeep();
        }
    }

    // Harvest 1 mana per tile owned.
    public void HarvestMana()
    {
        // Loop through each tile.
        for (int i = 0; i < GM.I.gridWidth; i++)
        {
            for (int j = 0; j < GM.I.gridHeight; j++)
            {
                // Get tile.
                Tile tile = GM.I.grid[i, j];

                // Check if tile is owned by this faction.
                if (tile.faction == faction)
                    GainMana();
            }
        }
    }

    // Gain mana.
    public void GainMana(int manaGained = 1)
    {
        // Update mana variable.
        mana += manaGained;

        // Update text display, if we're the active faction.
        if (faction == GM.I.activeFaction)
            UI.I.currentMana.text = mana.ToString();
    }
}
