using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [Header("Cursor Textures")]
    public Texture2D defaultCursor;
    public Texture2D inspectCursor;
    public Texture2D selectCursor;
    public Texture2D moveCursor;
    public Texture2D attackCursor;
    
    [Header("Cursor Settings")]
    public Vector2 hotspot = Vector2.zero; // Click point on cursor (usually center or tip)
    
    public static CursorManager I;
    
    void Awake()
    {
        if (I == null)
            I = this;
        else
            Destroy(this);
            
        SetDefaultCursor();
    }
    
    public void SetDefaultCursor()
    {
        Cursor.SetCursor(defaultCursor, hotspot, CursorMode.Auto);
    }

    public void SetInspectCursor()
    {
        Cursor.SetCursor(inspectCursor, hotspot, CursorMode.Auto);
    }

    public void SetSelectCursor()
    {
        Cursor.SetCursor(selectCursor, hotspot, CursorMode.Auto);
    }
    
    public void SetMoveCursor()
    {
        Cursor.SetCursor(moveCursor, hotspot, CursorMode.Auto);
    }
    
    public void SetAttackCursor()
    {
        Cursor.SetCursor(attackCursor, hotspot, CursorMode.Auto);
    }
    
    

    // Update the cursor to match the currently hovered tile.
    public void UpdateCursor()
    {
        // Check if we are hovering anything!
        if (GM.I.hoveredTile == null)
        {
            // Use default cursor when hovering nothing.
            SetDefaultCursor();
        }
        // Check if we are selecting one of our ships.
        else if (GM.I.selectedTile != null && GM.I.selectedTile.ship != null &&
            GM.I.selectedTile.ship.faction == GM.I.playerFaction)
        {
            // Get selected ship.
            Ship selectedShip = GM.I.selectedTile.ship;

            // Check if we are hovering an enemy ship.
            Ship hoveredShip = GM.I.hoveredTile.ship;
            if (hoveredShip != null && hoveredShip.faction != selectedShip.faction)
            {
                // Hovering an enemy ship.
                // Cursor - Attack
                SetAttackCursor();
            }
            // Check if we are hovering a movable tile.
            else if (GM.I.hoveredTile.moveCostFromSelectedTile >= 0
                && GM.I.hoveredTile.moveCostFromSelectedTile <= selectedShip.movementRemaining)
            {
                SetMoveCursor();
            }
            else
            {
                // Otherwise use default cursor.
                SetDefaultCursor();
            }
        }
        // Not selecting one of our ships
        else
        {
            // Check if we are hovering a ship.
            Ship hoveredShip = GM.I.hoveredTile.ship;
            if (hoveredShip != null)
            {
                // Check if the hovered ship is ours.
                if (hoveredShip.faction == GM.I.playerFaction)
                {
                    // Hovering a friendly ship.
                    // Cursor - Select
                    SetSelectCursor();
                } else {
                    // Hovering a foreign ship.
                    // Cursor - Inspect
                    SetInspectCursor();
                }
            } else {
                // Not selecting one of our ships, not hovering a ship.

                // Check if we are hovering one of our planets.
                if (GM.I.hoveredTile.myType == TileType.Planet && GM.I.hoveredTile.faction == GM.I.playerFaction)
                {
                    // Hovering a friendly planet.
                    // Cursor - Select
                    SetSelectCursor();
                } else {
                    // Hovering a normal tile.
                    // Cursor - Default
                    SetDefaultCursor();
                }
            }
        }
    }
}