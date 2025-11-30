using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // Remember the last hovered tile so we can unhighlight it.
    private Tile lastHoveredTile;

    // Update!
    void Update()
    {
        // Get mouse position in world space.
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = 0;

        // Get the tile we are hovering over (if extant).
        GM.I.hoveredTile = Utility.WorldToGrid(mouseWorldPos);

        // Check if we should update which tile is being hovered.
        if (GM.I.hoveredTile != lastHoveredTile)
        {
            // Clear the old hovered tile back to normal opacity.
            if (lastHoveredTile != null)
                lastHoveredTile.Unhover();

            // Hover the new tile.
            if (GM.I.hoveredTile != null)
                GM.I.hoveredTile.Hover();

            // Remember!
            lastHoveredTile = GM.I.hoveredTile;
        }

        // Check for left click.
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Check if we have a tile?
            if (GM.I.hoveredTile != null)
            {
                // Select tile!
                GM.I.hoveredTile.Select();
            } else {
                // Clear selection!
                Tile.ClearSelection();
            }
        }
    }
}