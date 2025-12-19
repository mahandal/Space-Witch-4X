using UnityEngine;

public class Settings : MonoBehaviour
{
    [Header("HUD")]
    public GameObject spectatorModeButton;
    public GameObject takeControlButton;

    [Header("Settings")]
    public static bool edgePanningEnabled = true;
    public bool aiPlaysForPlayer = false;

    // singleton
    public static Settings I;

    // Awaken!
    void Awake()
    {
        // Enforce singleton pattern.
        if (I == null)
            I = this;
        else
            Destroy(this);

        // Load edge panning preference (default to true if not set)
        edgePanningEnabled = PlayerPrefs.GetInt("EdgePanning", 1) == 1;
    }

    // Enter spectator mode!
    public void Button_SpectatorMode()
    {
        // Enable the AI playing for the player.
        aiPlaysForPlayer = true;

        // Disable spectator mode button.
        spectatorModeButton.SetActive(false);

        // Enable take control button.
        takeControlButton.SetActive(true);
    }

    // Exit spectator mode!
    public void Button_TakeControl()
    {
        // Disable the AI playing for the player.
        aiPlaysForPlayer = false;

        // Enable spectator mode button.
        spectatorModeButton.SetActive(true);

        // Disable take control button.
        takeControlButton.SetActive(false);
    }

    // Turn on God Mode!
    public void GodMode()
    {
        // Get leader.
        Leader leader = GM.I.leaders[GM.I.playerFaction];

        // Give godly stats.
        leader.currentHealth = 999;
        leader.maxHealth = 999;
        leader.speed = 99;
        leader.vision = 99;
        leader.range = 99;
        leader.damage = 99;
        leader.armor = 99;
        leader.attacks = 9;
        leader.Refresh();

        GM.I.UpdateFogOfWar();
    }

    // Choose your faction.
    public void Button_SelectFaction(string factionName)
    {
        // Get the faction from its name.
        Faction faction = Constance.factions[factionName];

        // Set the player's faction.
        GM.I.playerFaction = faction;

        GGG.I.HighlightCurrentFactionChoice();
    }
}
