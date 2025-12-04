using UnityEngine;

// Manage menu navigation.
public class MenuManager : MonoBehaviour
{
    [Header("Manual Machinery")]
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
}
