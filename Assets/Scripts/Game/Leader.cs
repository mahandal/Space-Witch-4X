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

    // A boolean tracking whether this leader has ended their turn.
    // Used to prevent ending your turn twice.
    // Seems like it shouldn't be necessary tbh, but other options didn't work so w/e.
    public bool hasEndedTurn;

    // - AI

    // Handle running an AI's turn.
    public IEnumerator AITurn()
    {
        // Center camera on leader.
        ShowVision();

        Debug.Log(myName + " is thinking...");

        // Wait a bit, to think.
        yield return new WaitForSeconds(0.5f);

        Debug.Log(myName + " is going now!");

        // Loop until we are out of actions.
        int shipCount = 1;
        while (CanFleetAct() || CanBuild())
        {
            Debug.Log(myName + " is in while loop");

            // Get next ship.
            Ship ship = GetRandomAvailableShip();

            if (ship != null)
            {
                shipCount++;
                Debug.Log(myName + "'s ship #" + shipCount + ". Name: " + ship.myName);
            }
            else{
                Debug.Log(myName + " is out of ships!");
            }

            // Check if we have a ship available.
            // If we do, let it auto pilot.
            if (ship != null)
                yield return ship.AutoPilot();
                
            // Check if we can build.
            if (CanBuild())
                yield return Build();
        }

        Debug.Log(myName + " while loop completed!");

        // End turn!
        yield return GM.I.EndTurn();
    }


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
        StartCoroutine(Move(corner, false));
    }

    // Handle starting a turn for this leader's faction.
    // - Harvest 1 mana per tile owned.
    // - Handle upkeep for each ship (e.g. burning in fire!)
    public IEnumerator StartTurn()
    {
        // Bools.
        hasEndedTurn = false;

        // Harvest mana.
        HarvestMana();

        // Handle upkeep for each ship in our fleet!
        // (make a temp list in case any die throughout)
        List<Ship> fleetCopy = new List<Ship>(fleet);
        foreach (Ship ship in fleetCopy)
        {
            ship.Upkeep();
        }

        // ienumerators have to return something
        yield return new WaitForSeconds(0.1f);
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
        // Build list of available ships
        List<Ship> availableShips = new List<Ship>();
        
        // Iterate through each ship in our fleet.
        foreach (Ship ship in fleet)
        {
            // Skip dead ships.
            // (so far they shouldn't be in the list at all but maybe in the future?)
            if (ship == null || ship.currentHealth <= 0) continue;
            
            // Skip ships on auto pilot if pressed by the player.
            if (clickedByPlayer) continue;
            
            // Check if ship has actions remaining
            if (ship.movementRemaining > 0 || ship.attacksRemaining > 0)
            {
                availableShips.Add(ship);
            }
        }
        
        // No available ships
        if (availableShips.Count == 0) return null;
        
        // Get random ship from available list
        int randomIndex = Random.Range(0, availableShips.Count);
        Ship selectedShip = availableShips[randomIndex];
        
        // Select and focus on the ship.
        // selectedShip.currentTile.Select();
        selectedShip.ShowVision();
        
        // Return the selected ship.
        return selectedShip;
    }

    // Get a list of all ships in our fleet that still have actions remaining.
    public List<Ship> GetAvailableShips()
    {
        // Initialize a new list of ships.
        List<Ship> availableShips = new List<Ship>();

        // Go through every ship in our fleet.
        foreach (Ship ship in fleet)
        {
            // Check if it has actions remaining.
            if (ship.movementRemaining > 0 || ship.attacksRemaining > 0)
                availableShips.Add(ship);
        }

        // Return.
        return availableShips;
    }

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

    // Lose all territory your faction had claimed.
    // Territory reverts to neutral.
    // Which means Neutral leaders don't do anything! ;)
    // Note: Faction has to be passed in, because this leader may have changed factions!
    public void LoseAllTerritory(Faction oldFaction)
    {
        // Loop through all tiles.
        foreach (Tile tile in Tile.GetTiles())
        {
            // Check if it was ours.
            if (tile.faction == oldFaction)
            {
                // Revert to neutral.
                tile.faction = Faction.Neutral;
            }
        }
    }

    // Check if our fleet has any actions remaining.
    // Returns true if any ship in our fleet has any movement or attacks remaining.
    // Returns false if we have no available ships.
    public bool CanFleetAct(bool ignoreAutoPilots = false)
    {
        // Iterate through our whole fleet.
        foreach (Ship ship in fleet)
        {
            // If we should ignore auto pilots and this ship is on auto pilot, ignore it.
            if (ignoreAutoPilots && ship.autoPilotMode != AutoPilotMode.Off)
                continue;

            // Check if we have any actions remaining.
            if (ship.movementRemaining > 0 || ship.attacksRemaining > 0)
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
        // For now at least, neutrals don't build.
        if (faction == Faction.Neutral) return false;

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
                Ship ship = GM.I.BuildShip(shipName, planet);

                // Reveal ship's vision.
                ship.ShowVision();

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
        // Remember the biggest blueprint.
        string biggestBlueprint = "";

        // Remember the highest cost we've seen.
        int highestCostSoFar = -1;

        // Look through each of our blueprints.
        foreach (Ship blueprint in blueprints)
        {
            // Check if its cost is higher than the highest we've seen so far,
            // but less than or equal to how much mana we currently have.
            if (blueprint.manaCost > highestCostSoFar && blueprint.manaCost <= mana)
            {
                // Remember this blueprint!
                biggestBlueprint = blueprint.myName;
                highestCostSoFar = blueprint.manaCost;
            }
        }

        // Return!
        return biggestBlueprint;
    }
}
