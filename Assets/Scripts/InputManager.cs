using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    void Update()
    {
        // Check for mouse click
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Get mouse position in world space
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mouseWorldPos.z = 0;

            // Convert to grid coordinates and get the tile
            Tile clickedTile = Utility.WorldToGrid(mouseWorldPos);
            
            // Make sure the tile exists (bounds checking)
            if (clickedTile != null)
            {
                // Select tile!
                clickedTile.Select();
            } else {
                // Clear selection!
                Tile.ClearSelection();
            }
        }
    }
}