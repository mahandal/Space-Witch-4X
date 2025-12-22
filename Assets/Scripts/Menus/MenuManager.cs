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
        // Disable time.
        Time.timeScale = 0f;
        
        // Enable the options menu.
        settingsMenu.SetActive(true);
    }

    // Close the settings menu.
    public void Button_CloseSettings()
    {
        // Enable time.
        Time.timeScale = 1f;

        // Disable the options menu.
        settingsMenu.SetActive(false);
    }
}
