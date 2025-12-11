using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildButton : MonoBehaviour
{
    [Header("Build Button")]
    public string shipName = "";

    [Header("Manual Machinery")]
    // The image displaying the ship this button can build.
    public Image image;

    // This button's highlight image.
    public Image highlight;

    // The text object displaying how much mana this button costs to press.
    public TMP_Text manaCost;

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
    
    // - Opacity
    // Fade opacity for buttons you can't afford.

    // Set opacity for all build buttons.
    public static void SetOpacityForAll()
    {
        // Look through each build button.
        foreach (BuildButton buildButton in UI.I.buildButtons)
        {
            // Get the progenitor for this button's ship.
            Ship progenitor = SpawnManager.I.GetProgenitor(buildButton.shipName);

            // Get player leader.
            Leader leader = GM.I.leaders[GM.I.playerFaction];

            // Check if the player can afford this button.
            if (leader.mana < progenitor.manaCost)
            {
                // Can't afford it.
                // Fade out!
                buildButton.LowOpacity();
            } else {
                // Can afford it!
                // Fade in!
                buildButton.HighOpacity();
            }
        }
    }

    // Fade the opacity for this build button.
    public void LowOpacity()
    {
        image.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    }

    // Heighten the opacity of this build button!
    public void HighOpacity()
    {
        image.color = new Color(1f, 1f, 1f, 1f);
    }

    // - Highlighting
    // Highlight the currently selected button.

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
        // image.color = new Color(1f, 1f, 1f, 0.5f);
        highlight.gameObject.SetActive(false);
    }

    // Highlight this build button.
    public void Highlight()
    {
        // image.color = new Color(1f, 1f, 1f, 1f);
        highlight.gameObject.SetActive(true);
    }


    // - Loading

    // Load a ship's image and cost into this build button.
    public void LoadShip(Ship blueprint)
    {
        // If blueprint is null, hide button instead.
        if (blueprint == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // Remember the ship's name.
        shipName = blueprint.myName;

        // Set this build button's mana cost.
        manaCost.text = blueprint.manaCost.ToString();

        // Get the file path for the image for this ship.
        string imageFilePath = "Ships/" + blueprint.faction.ToString() + "/Ship - " + shipName;

        // Load this ship's image.
        Utility.LoadImage(image, imageFilePath);

        // Ensure button is active!
        gameObject.SetActive(true);
    }
}
