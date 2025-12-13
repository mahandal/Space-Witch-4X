using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Your leader ship runs your fleet!
public class Leader : Ship
{
    [Header("Leader")]
    // How much mana we currently have available.
    public int mana = 0;

    // The list of ship types we know how to build.
    public List<Ship> blueprints;

    // A list of all our ships.
    public List<Ship> fleet = new List<Ship>();

    // - AI

    // Start running a turn for an AI.
    // Wrapper of AITurnCoroutine
    // public void AITurn()
    // {
    //     // Start coroutine.
    //     StartCoroutine(AITurnCoroutine());
    // }

    // Handle running an AI's turn.
    public IEnumerator AITurn()
    {
        // Center camera on leader.
        Utility.MoveCamera(currentTile);

        // Reveal ship's vision.
        ShowVision();

        Debug.Log(myName + " is thinking...");

        // Wait a bit, to think.
        yield return new WaitForSeconds(0.5f);

        Debug.Log(myName + " is going now!");

        // Loop until we are out of actions.
        while (CanFleetAct() || CanBuild())
        {
            // TBD:
            // Get next ship.
            Ship ship = GetRandomAvailableShip();

            // Check if we have a ship available.
            // If we do, let it auto pilot.
            if (ship != null)
                yield return ship.AutoPilot();
                

            // Check if we can build.
            if (CanBuild())
                yield return Build();
        }
    }

    // Handle running an AI's turn.
    // public IEnumerator AITurnCoroutine()
    // {
    //     // Make a copy of the fleet list to avoid modification during iteration
    //     List<Ship> fleetList = new List<Ship>(fleet);
        
    //     // Loop through each ship in our fleet
    //     foreach (Ship ship in fleetList)
    //     {
    //         // Skip if ship is dead
    //         if (ship == null || ship.currentHealth <= 0) continue;

    //         // Center camera on ship.
    //         Utility.MoveCamera(ship.currentTile);

    //         // Reveal ship's vision.
    //         ship.ShowVision();

    //         // Wait a bit, give each ship their moment.
    //         yield return new WaitForSeconds(0.5f);
            
    //         // Get visible enemies
    //         List<Ship> visibleEnemies = ship.GetVisibleEnemies();
            
    //         // Check if we can see any enemies
    //         if (visibleEnemies.Count > 0)
    //         {
    //             // Attack the closest enemy
    //             Ship closestEnemy = null;
    //             int shortestDistance = int.MaxValue;
                
    //             foreach (Ship enemy in visibleEnemies)
    //             {
    //                 int distance = Utility.Distance(ship.currentTile, enemy.currentTile);
    //                 if (distance < shortestDistance)
    //                 {
    //                     shortestDistance = distance;
    //                     closestEnemy = enemy;
    //                 }
    //             }
                
    //             // Try to attack
    //             bool attackSuccessful = ship.AttemptAttackMove(closestEnemy, true);
                
    //             // If we couldn't attack, try to move closer
    //             if (!attackSuccessful)
    //             {
    //                 ship.MoveToward(closestEnemy.currentTile);
    //             }
    //         } else {
    //             // - No enemies sighted.

    //             // TBD: Refine AI, add personality

    //             // For now, move toward nearest neutral tile.

    //             // Move until we can't no mo!
    //             while (ship.movementRemaining > 0)
    //             {
    //                 // Find the nearest neutral tile.
    //                 Tile destination = ship.FindNearestNeutralTile();

    //                 // Check if there is one.
    //                 if (destination != null)
    //                 {
    //                     // Move to destination.
    //                     bool successfullyMoved = ship.MoveToward(destination);

    //                     // Failed to move toward destination for some reason. Rest?
    //                     if (!successfullyMoved)
    //                         ship.AttemptRest();
    //                     // ship.MoveToward(destination);
    //                 } else {
    //                     // TBD: Attack? Defend?

    //                     // For now, just rest.
    //                     ship.AttemptRest();
    //                 }
    //             }
    //         }

    //         // Spend rest of movement and/or attacks resting.
    //         ship.AttemptRest();

    //         // Center camera on ship.
    //         Utility.MoveCamera(ship.currentTile);

    //         // Reveal ship's vision.
    //         ship.ShowVision();

    //         // Wait a bit, give each ship their moment.
    //         yield return new WaitForSeconds(0.5f);
    //     }

    //     // Build more ships!
    //     yield return StartCoroutine(Build());

    //     // End our turn!
    //     GM.I.EndTurn();
    // }

    // - Core

    // Initialize this leader:
    // - Move to its corner of the map.
    // - Heal it to full health.
    // - Refresh its movement and attacks.
    // - Reset mana to 0.
    // - Clear any previous ships in its fleet.
    public void Init()
    {
        // Move to its corner of the map.
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
        fleet = new List<Ship>();
        fleet.Add(this);

        // Set health bar color.
        healthBar.color = Constance.FactionColor(faction, 1f);

        // Start with movement and attacks hidden.
        HideMovementAndAttacks();
    }

    // Move this leader to its corner of the map as its starting location.
    // TBD: Scale! Improve! Or maybe just keep it if it's fun enough!
    public void MoveToStartingLocation()
    {
        // Initialize to (0, 0) cause why not?
        // (keep it for the Pack!)
        Tile corner = GM.I.GetTile(0, 0);

        // Get the Coven's corner.
        if (faction == Faction.Coven)
            corner = GM.I.GetTile(0, GM.I.gridHeight - 1);

        // Get the Syndicate's corner.
        if (faction == Faction.Syndicate)
            corner = GM.I.GetTile(GM.I.gridWidth - 1, GM.I.gridHeight - 1);

        // Get the Neutral corner.
        if (faction == Faction.Neutral)
            corner = GM.I.GetTile(GM.I.gridWidth - 1, 0);

        // Move to corner.
        Move(corner, false);
    }

    // Handle ending a turn for this leader's faction.
    // - Refreshes all ships in our fleet.
    // - Hides movement and attacks remaining for all ships.
    // --- (Not hidden like a secret, just so you can see which faction is active currently easier)
    public IEnumerator EndTurn()
    {
        // // Handle auto-pilots.
        // foreach (Ship ship in fleet)
        // {
        //     ship.AutoPilot();
        // }

        // Refresh our fleet!
        RefreshFleet();

        // Hide movement and attacks remaining for all ships in our fleet.
        HideFleetMovementAndAttacks();

        // Have to return from an ienumerator for w/e reason
        yield return new WaitForSeconds(0.1f);
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

    // - Mana

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
                {
                    // Check if tile is a planet.
                    if (tile.myType == TileType.Planet)
                    {
                        // Gain mana equal to the current round.
                        // GainMana(GM.I.round);

                        // Gain 1 mana.
                        GainMana(1);
                    } else {
                        // Gain 1 mana.
                        GainMana(1);
                    }
                }
            }
        }
    }

    // Gain mana.
    public void GainMana(int manaGained = 1)
    {
        // Update mana variable.
        mana += manaGained;

        // Visuals.
        UI.I.UpdateManaDisplay();
    }

    // Spend mana.
    public void SpendMana(int manaSpent = 1)
    {
        // Update mana variable.
        mana -= manaSpent;

        // Visuals.
        UI.I.UpdateManaDisplay();
    }

    // - Fleet management

    // Get a random available ship in your fleet.
    public Ship GetRandomAvailableShip(bool clickedByPlayer = false)
    {
            
    }


/*

        // Search for next available ship.
        for (int i = 0; i < fleetList.Count; i++)
        {
            // Index into our fleet using our starting index.
            int index = (startIndex + i) % fleetList.Count;
            Ship ship = fleetList[index];
            
            // Skip dead ships.
            if (ship == null || ship.currentHealth <= 0) continue;

            // Skip ships on auto pilot.
            if (ship.autoPilotMode != AutoPilotMode.Off) continue;
            
            // Check if ship has actions remaining.
            if (ship.movementRemaining > 0 || ship.attacksRemaining > 0)
            {
                // Select this ship.
                ship.currentTile.Select();
                
                // Move camera to center on it.
                Utility.MoveCamera(ship.currentTile);

                // Done!
                return;
            }
        }
    }
*/
    // Your fleet abandons.
    // All ships are destroyed.
    // Called when a leader dies.
    public void AbandonFleet()
    {
        // Make a copy of the fleet to avoid modification during iteration.
        List<Ship> fleetCopy = new List<Ship>(fleet);
        
        // Destroy all ships in the fleet.
        foreach (Ship ship in fleetCopy)
        {
            // Remove from fleet
            fleet.Remove(ship);
            
            // Clean up object
            Object.Destroy(ship.gameObject);
        }
    }

    // Check if our fleet has any actions remaining.
    // Returns true if any ship in our fleet has any movement or attacks remaining.
    // Returns false if our turn is over (aside from ship production).
    public bool CanFleetAct()
    {
        // Iterate through our whole fleet.
        foreach (Ship ship in fleet)
        {
            if (ship.autoPilotMode == AutoPilotMode.Off && 
                (ship.movementRemaining > 0 || ship.attacksRemaining > 0))
                return true;
        }

        // Not a single action remaining in our whole fleet.
        // Return false!
        return false;
    }

    // Check if we are able to build any ships.
    // Returns true if we have a planet available and enough mana for a ship.
    // Returns false if we don't have a planet available, or enough mana for a ship.
    public bool CanBuild()
    {
        bool planetAvailable = false;
        bool enoughMana = false;

        // Iterate through each of our planets.
        foreach (Tile planet in GetMyPlanets())
        {
            // Check if it is available!
            if (planet.ship == null)
                planetAvailable = true;
        }

        // Iterate through each of our blueprints.
        foreach (Ship blueprint in blueprints)
        {
            // Check if we have enough mana to build it!
            if (blueprint.manaCost <= mana)
                enoughMana = true;
        }

        // Return!
        bool canBuild = planetAvailable && enoughMana;
        return canBuild;
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

    // - Ship building

    // Handle a turn of building for this leader.
    // Called once each turn by each AI.
    // Try to build the most expensive ship we can at each planet we can.
    public IEnumerator Build()
    {
        // Avoid pirate's building for now.
        // (soon will add space worm ritual!)
        if (faction == Faction.Neutral) yield break;

        // Go through each of our planets.
        List<Tile> myPlanets = GetMyPlanets();
        foreach (Tile planet in myPlanets)
        {
             // Center camera on planet.
            Utility.MoveCamera(planet);

            // Show planet to player.
            planet.RevealFromFog();

            // Get the name of the biggest blueprint we can afford to build.
            string shipName = GetBiggestBlueprint();

            // Check we have a valid ship to build, and a valid planet to build it on.
            if (shipName != "" && planet.ship == null)
            {
                // Build it!
                GM.I.BuildShip(shipName, planet);

                yield return new WaitForSeconds(0.5f);
            }

            yield return new WaitForSeconds(0.2f);

            // Hide planet to player.
            planet.HideInFog();
        }
    }


    // Return a list of all planets this leader owns.
    public List<Tile> GetMyPlanets()
    {
        // List of all planet tiles belonging to the same faction as this leader.
        List<Tile> myPlanets = new List<Tile>();

        // Loop through grid.
        for (int i = 0; i < GM.I.gridWidth; i++)
        {
            for (int j = 0; j < GM.I.gridHeight; j++)
            {
                // Get tile.
                Tile tile = GM.I.grid[i, j];

                // Check if tile is one of our planets.
                if (tile.myType == TileType.Planet && tile.faction == faction)
                {
                    // Add to list.
                    myPlanets.Add(tile);
                }
            }
        }

        // Return!
        return myPlanets;
    }

    // Return the highest costing blueprint this leader can build.
    public string GetBiggestBlueprint()
    {
        // Remember the biggest ship.
        string biggestShip = "";

        // Remember the highest cost we've seen.
        int highestCostSoFar = -1;

        // Look through each of our blueprints.
        foreach (Ship ship in blueprints)
        {
            // Check if its cost is higher than the highest we've seen so far,
            // but less than or equal to how much mana we currently have.
            if (ship.manaCost > highestCostSoFar && ship.manaCost <= mana)
            {
                // Remember this ship!
                biggestShip = ship.myName;
                highestCostSoFar = ship.manaCost;
            }
        }

        // Return!
        return biggestShip;
    }
}
