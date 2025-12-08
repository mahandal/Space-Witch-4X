using UnityEngine;
using System.Collections.Generic;

// Constance handles my constants!
// She's a friendly old innkeeper, proprietor of The Dragon's Roost.
// You can trust her, don't worry!
public static class Constance
{
    // How big our tiles are.
    public static int tileSize = 1;

    // Return the color for a faction.
    public static Color FactionColor(Faction faction, float opacity = 1f)
    {
        Color c = new Color(0f, 0f, 0f, opacity);

        // Neutral
        if (faction == Faction.Neutral)
            c = new Color(0.5f, 0.5f, 0.5f, opacity);
        // The Pack
        else if (faction == Faction.Pack)
            c = new Color(0f, 1f, 0.0429f, opacity);
        // The Coven
        else if (faction == Faction.Coven)
            c = new Color(0f, 0.9889f, 1f, opacity);
        // The Syndicate
        else if (faction == Faction.Syndicate)
            c = new Color(1f, 0.8535f, 0f, opacity);

        return c;
    }

    // - Ship Types
    public static List<string> GetShipTypesForFaction(Faction faction)
    {
        // Initialize a list.
        List<string> shipTypes = new List<string>();

        // Pack
        if (faction == Faction.Pack)
        {
            shipTypes.Add("Tarodactyl");
        }
        // Coven
        else if (faction == Faction.Coven)
        {
            shipTypes.Add("Space Witch");
        }
        // Syndicate
        else if (faction == Faction.Syndicate)
        {
            shipTypes.Add("Flybot");
        }
        // Neutral
        else
        {
            shipTypes.Add("Sky Pirate");
        }

        return shipTypes;
    }

    // Pack
    public static List<string> packShipTypes = new List<string>();
}

// - Enums

// Factions.
public enum Faction
{
    Neutral,
    Pack,
    Coven,
    Syndicate
}

// Tiles.
public enum TileType
{
    Void,
    Air,
    Water,
    Fire,
    Asteroids,
    Planet
}