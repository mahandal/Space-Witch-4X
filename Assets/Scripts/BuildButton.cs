using UnityEngine;
using UnityEngine.UI;

public class BuildButton : MonoBehaviour
{
    [Header("Build Button")]
    public string shipName = "";

    // The image displaying the ship this button can build.
    private Image image;

    // This button's highlight image.
    public Image highlight;

    // Awaken!
    void Awake()
    {
        image = GetComponent<Image>();
    }

    // Called when this button is pressed.
    public void Button_Pressed()
    {
        // Clear prior unit selection.
        Tile.ClearSelection();

        // Clear prior building selection.
        // (redundant cause it's now called in Tile.ClearSelection above)
        // ClearAllBuildHighlights();

        // Get mana cost.
        int manaCost = GM.I.GetManaCost(shipName);

        // Get player's leader.
        Leader leader = GM.I.leaders[GM.I.playerFaction];

        // Check if player has enough mana to build this ship.
        if (leader.mana < manaCost)
        {
            // Return.
            return;
        }

        // Set current build selection.
        GM.I.currentlyBuilding = shipName;

        // Highlight.
        Highlight();
    }

    // Clear whichever build button was highlighted.
    public static void ClearAllBuildHighlights()
    {
        foreach (BuildButton buildButton in UI.I.buildButtons)
        {
            buildButton.ClearHighlight();
        }
    }

    // Clear the highlight effect.
    public void ClearHighlight()
    {
        Debug.Log(name + " is clearing its highlight!");

        // image.color = new Color(1f, 1f, 1f, 0.5f);
        highlight.gameObject.SetActive(false);
    }

    // Highlight this build button.
    public void Highlight()
    {
        Debug.Log(name + " is activating its highlight!");
        
        // image.color = new Color(1f, 1f, 1f, 1f);
        highlight.gameObject.SetActive(true);
    }
}
