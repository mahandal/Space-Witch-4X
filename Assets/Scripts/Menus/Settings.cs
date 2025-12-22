using UnityEngine;

public class Settings : MonoBehaviour
{
    [Header("HUD")]
    // Spectator mode
    public GameObject spectatorModeButton;
    public GameObject takeControlButton;

    [Header("Settings")]
    public static bool edgePanningEnabled = true;
    public static bool spectatorMode = false;

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

        // - Load player prefs.

        // Edge panning (default to true).
        edgePanningEnabled = PlayerPrefs.GetInt("EdgePanning", 1) == 1;

        // Spectator mode (default to false).
        spectatorMode = PlayerPrefs.GetInt("SpectatorMode", 0) == 1;

        // - Activate appropriate buttons.

        // TBD: Edge panning

        // Spectator mode
        spectatorModeButton.SetActive(!spectatorMode);
        takeControlButton.SetActive(spectatorMode);
    }

    // Enter spectator mode!
    public void Button_SpectatorMode()
    {
        // Enable the AI playing for the player.
        spectatorMode = true;

        // Disable spectator mode button.
        spectatorModeButton.SetActive(false);

        // Enable take control button.
        takeControlButton.SetActive(true);

        // If it is the player's turn, start the AI playing for them.
        if (GM.I.activeFaction == GM.I.playerFaction)
        {
            // Get the player's leader.
            Leader leader = GM.I.leaders[GM.I.playerFaction];

            // Let the leader start their turn.
            leader.StartCoroutine(leader.AITurn());
        }

        // Save to PlayerPrefs (1 for true, 0 for false)
        PlayerPrefs.SetInt("SpectatorMode", Settings.spectatorMode ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Exit spectator mode!
    public void Button_TakeControl()
    {
        // Disable the AI playing for the player.
        spectatorMode = false;

        // Enable spectator mode button.
        spectatorModeButton.SetActive(true);

        // Disable take control button.
        takeControlButton.SetActive(false);

        // If it is the player's turn, stop the AI playing for them.
        if (GM.I.activeFaction == GM.I.playerFaction)
        {
            // Get the player's leader.
            Leader leader = GM.I.leaders[GM.I.playerFaction];

            // Stop all coroutines to interrupt the AI.
            leader.StopAllCoroutines();
        }

        // Save to PlayerPrefs (1 for true, 0 for false)
        PlayerPrefs.SetInt("SpectatorMode", Settings.spectatorMode ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Button pressed to enter god mode.
    // Should maybe make a toggle? but not a priority.
    public void Button_GodMode()
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

    // Toggle edge panning on/off
    public void Button_ToggleEdgePanning()
    {
        // If it's off, turn it on.
        if (!edgePanningEnabled)
        {
            // Set bool.
            edgePanningEnabled = true;

            // Load image.
            // TBD!
            // Utility.LoadImage(UI.I.toggleEdgePanning, "Toggle - On");
        } else {
            // Set bool.
            edgePanningEnabled = false;

            // Load image.
            // TBD!
            // Utility.LoadImage(UI.I.toggleEdgePanning, "Toggle - Off");
        }

        // Save to PlayerPrefs (1 for true, 0 for false)
        PlayerPrefs.SetInt("EdgePanning", Settings.edgePanningEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Choose your faction.
    // TBD: Remember in player prefs!
    public void Button_SelectFaction(string factionName)
    {
        // Get the faction from its name.
        Faction faction = Constance.factions[factionName];

        // Set the player's faction.
        GM.I.playerFaction = faction;

        GGG.I.HighlightCurrentFactionChoice();
    }
}
