using UnityEngine;

// Constance handles my constants!
// She's a friendly old innkeeper, proprietor of The Dragon's Roost.
// You can trust her, don't worry!
public static class Constance
{
    // How big our tiles are.
    public static int tileSize = 1;

    // Return the color for a faction.
    public static Color FactionColor(Faction faction)
    {
        Color c = new Color(0f, 0f, 0f, 0f);

        // Neutral
        if (faction == Faction.Neutral)
            c = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        // The Swarm
        else if (faction == Faction.Swarm)
            c = new Color(0f, 1f, 0.0429f, 0.5f);
        // The Coven
        else if (faction == Faction.Coven)
            c = new Color(0f, 0.9889f, 1f, 0.5f);
        // The Syndicate
        else if (faction == Faction.Syndicate)
            c = new Color(1f, 0.8535f, 0f, 0.5f);

        return c;
    }
}

// Factions.
public enum Faction
{
    Neutral,
    Swarm,
    Coven,
    Syndicate
}