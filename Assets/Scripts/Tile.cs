using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Tile")]
    public string myType = "Air";
    public int x = 0;
    public int y = 0;
    public Ship ship;

    [Header("Manual Machinery")]
    // The background color on top of this tile's sprite.
    // Completely clear by default.
    // Highlights differently to show:
    // - Selected tile is highlighted pink!
    // - Movement range is shown in blue.
    // - Attack range is shown in red.
    // - Purple is used for tiles a ship can move and attack to.
    // (so blue is actually rare, only used for pacifists and ships with minimum range)
    public SpriteRenderer bg;

    // Clear our selection so no tiles are highlighted.
    public static void ClearSelection()
    {
        // Clear selection.
        GM.I.selectedTile = null;

        // Clear highlights.
        ClearAllHighlights();
    }

    // Clear ALL highlighting for ALL tiles.
    public static void ClearAllHighlights()
    {
        // Loop through columns.
        for (int x = 0; x < GM.I.gridWidth; x++)
        {
            // Loop through rows.
            for (int y = 0; y < GM.I.gridHeight; y++)
            {
                // Clear!
                GM.I.grid[x,y].ClearHighlight();
            }
        }
    }


    // Select this tile!
    public void Select()
    {
        // Clear old highlighting.
        ClearAllHighlights();

        // Highlight the current tile!
        HighlightSelected();

        // Highlight movement and attack ranges.
        if (ship != null)
            HighlightRanges();

        // Set as currently selected tile.
        GM.I.selectedTile = this;
    }

    // Highlight nearby tiles to show the selected ship's movement and attack ranges.
    // TBD!
    public void HighlightRanges()
    {
        // Loop through columns.
        for (int x = 0; x < GM.I.gridWidth; x++)
        {
            // Loop through rows.
            for (int y = 0; y < GM.I.gridHeight; y++)
            {
                // Get tile.
                Tile otherTile = GM.I.grid[x, y];
            }
        }
    }

    // - Colorize background.

    // Clear background color.
    public void ClearHighlight()
    {
        bg.color = new Color(0f, 0f, 0f, 0f);
    }

    // Set background color to pink to show the currently selected tile.
    public void HighlightSelected()
    {
        bg.color = new Color(1f, 0.154f, 0.7932f, 0.7843f);
    }

    // Set background color to purple to show a tile can be both moved to and attacked,
    // by the currently selected ship.
    public void HighlightMoveAndAttack()
    {
        bg.color = new Color(0.6978f, 0.2402f, 0.9433f, 0.4823f);
    }

    // Set background color to blue to show a tile can be moved to (but not attacked!),
    // by the currently selected ship.
    public void HighlightMove()
    {
        bg.color = new Color(0.2932f, 0.2638f, 0.7295f, 0.6431f);
    }

    // Set background color to red to show a tile can be attacked (but not moved to!),
    // by the currently selected ship.
    public void HighlightAttack()
    {
        bg.color = new Color(0.7861f, 0.22f, 0.2256f, 0.6431f);
    }
}
