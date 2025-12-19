using UnityEngine;

// Manage menu navigation.
public class MenuManager : MonoBehaviour
{
    [Header("Manual Machinery")]
    public InputManager inputManager;
    public GameObject settingsMenu;
    public GameObject ggg;

    // Singleton.
    public static MenuManager I;

    // Awaken!
    void Awake()
    {
        // Enforce singleton pattern.
        if (I != null && I != this)
            Destroy(this);
        else
            I = this;

        // Disable what should not be.
        settingsMenu.SetActive(false);

        // Enable what should be.
        ggg.SetActive(true);
    }

    // Close Granga's Guide to the Galaxy and begin a battle.
    public void Button_Play()
    {
        // Begin a new battle.
        GM.I.BeginBattle();

        // Hide Granga's Guide to the Galaxy.
        ggg.SetActive(false);
    }

    // Open the settings menu.
    public void Button_OpenSettings()
    {
        // Enable the options menu.
        settingsMenu.SetActive(true);
    }

    // Close the settings menu.
    public void Button_CloseSettings()
    {
        // Disable the options menu.
        settingsMenu.SetActive(false);
    }

    // Toggle edge panning on/off
    public void Button_ToggleEdgePanning()
    {
        // If it's off, turn it on.
        if (!Settings.edgePanningEnabled)
        {
            // Set bool.
            Settings.edgePanningEnabled = true;

            // Load image.
            // Utility.LoadImage(UI.I.toggleEdgePanning, "Toggle - On");
        } else {
            // Set bool.
            Settings.edgePanningEnabled = false;

            // Load image.
            // Utility.LoadImage(UI.I.toggleEdgePanning, "Toggle - Off");
        }

        // Save to PlayerPrefs (1 for true, 0 for false)
        PlayerPrefs.SetInt("EdgePanning", Settings.edgePanningEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }
}
