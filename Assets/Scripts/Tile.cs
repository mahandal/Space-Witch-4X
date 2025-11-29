using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Tile")]
    public string myType = "Air";
    public int x = 0;
    public int y = 0;

    // The background color on top of this tile's sprite.
    // Completely clear by default.
    // Highlights differently to show:
    // - Selected tile is highlighted pink!
    // - Movement range is shown in blue.
    // - Attack range is shown in red.
    // - Purple is used for tiles a ship can move and attack to.
    // (so blue is actually rare, only used for pacifists and ships with minimum range)
    public SpriteRenderer bg;

    // - Colorize background.

    // Clear background color.
    public void ClearBackground()
    {
        bg.color = new Color(0f, 0f, 0f, 0f);
    }

    // Set background color to pink to show the currently selected tile.
    public void BackgroundPink()
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
