using UnityEngine;

// Manage menu navigation.
public class MenuManager : MonoBehaviour
{
    [Header("Manual Machinery")]
    public InputManager inputManager;
    public GameObject settingsMenu;

    // Awaken!
    void Awake()
    {
        // Disable what should not be.
        settingsMenu.SetActive(false);
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
        if (!inputManager.edgePanningEnabled)
        {
            // Set bool.
            inputManager.edgePanningEnabled = true;

            // Load image.
            // Utility.LoadImage(UI.I.toggleEdgePanning, "Toggle - On");
        } else {
            // Set bool.
            inputManager.edgePanningEnabled = false;

            // Load image.
            // Utility.LoadImage(UI.I.toggleEdgePanning, "Toggle - Off");
        }
        // inputManager.edgePanningEnabled = !inputManager.edgePanningEnabled; 
    }
}
