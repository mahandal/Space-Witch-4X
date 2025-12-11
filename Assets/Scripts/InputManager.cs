using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Camera")]
    // Edge panning
    public float edgePanThreshold = 20f; // How close to edge before panning starts
    public float panSpeed = 10f; // Camera movement speed
    public float cameraPadding = 2f; // Extra space beyond grid edges

    // Camera dragging
    private bool isDragging = false;
    private Vector3 dragStartScreenPos;
    private Vector3 dragStartCameraPos;

    // Zoom
    public float zoomSpeed = 5f;
    public float minZoom = 1f;
    public float maxZoom = 5f;

    // Remember the last hovered tile so we can unhighlight it.
    private Tile lastHoveredTile;

    // Update!
    void Update()
    {
        // Wait for game to begin.
        if (GM.I == null || GM.I.gameState < 1)
            return;
        
        HandleHovering();
        HandleLeftClick();
        HandleRightClick();
        HandleCameraDragging();
        HandleEdgePanning();
        HandleCameraZoom();

        HandleHotkeys();
    }

    // Handle hovering over tiles.
    public void HandleHovering()
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
    }

    // Handle left clicks.
    // Select a tile if we click on it!
    // OR clear our selection if we click on nothing.
    public void HandleLeftClick()
    {
        // Ignore clicks on UI
        // (buttons are handled separately)
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        // Check for left click.
        // if (Mouse.current.leftButton.wasPressedThisFrame)
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            // Check if we have a tile?
            if (GM.I.hoveredTile != null)
            {
                // Check if we are hovering the tile we already have selected.
                if (GM.I.hoveredTile == GM.I.selectedTile)
                {
                    // Clear selection!
                    Tile.ClearSelection();
                } else {
                    // Select the new tile!
                    GM.I.hoveredTile.Select();
                }
            } else {
                // Clear selection!
                Tile.ClearSelection();
            }
        }
    }

    // Handle right clicks.
    public void HandleRightClick()
    {
        // Did we just right click?
        // if (!Mouse.current.rightButton.wasPressedThisFrame) return;
        if (!Mouse.current.rightButton.wasReleasedThisFrame) return;

        // Clear our building selection, if we were thinking of building something.
        GM.I.ResetBuildOrder();

        // Remember whether we should re-select the currently selected ship after the current action.
        bool shouldReselect = false;
  
        // Check if we are trying to control a ship.
        Ship selectedShip = null;
        if (GM.I.selectedTile != null)
            selectedShip = GM.I.selectedTile.ship;

        if (selectedShip != null)
        {
            // Make sure the target tile exists.
            if (GM.I.hoveredTile != null)
            {
                // Check if we're targeting the selected tile.
                if (GM.I.hoveredTile == GM.I.selectedTile)
                {
                    // Rest!
                    selectedShip.AttemptRest();
                }
                // Check if there's an enemy ship there.
                else if (GM.I.hoveredTile.ship != null &&
                    GM.I.hoveredTile.ship.faction != selectedShip.faction)
                {
                    // Try moving toward the enemy ship and attacking them.
                    selectedShip.AttemptAttackMove(GM.I.hoveredTile.ship);
                } else {
                    // Try to move the ship to the target tile.
                    selectedShip.AttemptMove(GM.I.hoveredTile.x, GM.I.hoveredTile.y);
                }

                // See if we should reselect this ship after.
                if (selectedShip.movementRemaining > 0 || selectedShip.attacksRemaining > 0)
                    shouldReselect = true;
            }
        }

        // Check if we should reselect the ship we just used,
        // or just clear the selection entirely.
        if (shouldReselect)
            selectedShip.currentTile.Select();
        else
            Tile.ClearSelection();
    }

    // Handle camera dragging.
    // Use middle click to drag the camera.
    public void HandleCameraDragging()
    {
        // Check if middle mouse button was just pressed
        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            isDragging = true;
            dragStartScreenPos = Mouse.current.position.ReadValue();
            dragStartCameraPos = Camera.main.transform.position;
        }

        // Check if middle mouse button was released
        if (Mouse.current.middleButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        // Handle dragging
        if (isDragging)
        {
            Vector3 currentScreenPos = Mouse.current.position.ReadValue();
            Vector3 screenDifference = dragStartScreenPos - currentScreenPos;
            
            // Convert screen difference to world difference
            Vector3 worldDifference = Camera.main.ScreenToWorldPoint(screenDifference) - Camera.main.ScreenToWorldPoint(Vector3.zero);
            
            Vector3 newPos = dragStartCameraPos + worldDifference;

            // Calculate dynamic boundaries from grid size
            float minX = -cameraPadding;
            float maxX = GM.I.gridWidth * Constance.tileSize + cameraPadding;
            float minY = -cameraPadding;
            float maxY = GM.I.gridHeight * Constance.tileSize + cameraPadding;

            // Clamp to boundaries
            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
            newPos.z = Camera.main.transform.position.z;

            Camera.main.transform.position = newPos;
        }
    }

    // Handle edge panning.
    // When the mouse gets near enough an edge of the screen,
    // move the camera in that direction.
    public void HandleEdgePanning()
    {
        // Check if it's disabled.
        if (!Settings.edgePanningEnabled) return;

        // - Edge panning

        // Get mouse screen position
        Vector2 mousePos = Mouse.current.position.ReadValue();
        
        // Calculate pan direction
        Vector3 panDirection = Vector3.zero;
        
        // Check left edge
        if (mousePos.x < edgePanThreshold)
            panDirection.x = -1f;
        
        // Check right edge
        if (mousePos.x > Screen.width - edgePanThreshold)
            panDirection.x = 1f;
        
        // Check bottom edge
        if (mousePos.y < edgePanThreshold)
            panDirection.y = -1f;
        
        // Check top edge
        if (mousePos.y > Screen.height - edgePanThreshold)
            panDirection.y = 1f;
        
        // Apply panning
        if (panDirection != Vector3.zero)
        {
            Vector3 newPos = Camera.main.transform.position + panDirection * panSpeed * Time.deltaTime;
            
            // Calculate dynamic boundaries from grid size
            float minX = -cameraPadding;
            float maxX = GM.I.gridWidth * Constance.tileSize + cameraPadding;
            float minY = -cameraPadding;
            float maxY = GM.I.gridHeight * Constance.tileSize + cameraPadding;
            
            // Clamp to boundaries
            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
            
            Camera.main.transform.position = newPos;
        }
    }

    // Handle camera zooming with mouse scroll wheel
    public void HandleCameraZoom()
    {
        // Get scroll input
        float scrollInput = Mouse.current.scroll.ReadValue().y;
        
        // Apply zoom if there's scroll input
        if (scrollInput != 0)
        {
            Camera cam = Camera.main;
            
            // Adjust orthographic size (smaller = more zoomed in)
            float newSize = cam.orthographicSize - (scrollInput * zoomSpeed * Time.deltaTime);
            
            // Clamp to min/max zoom
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }


    // Handle hotkeys for building ships (1-4)
    // public void HandleBuildHotkeys()
    // {
    //     // Only allow building during player's turn
    //     if (GM.I.activeFaction != GM.I.playerFaction) return;

    //     // Check number keys 1-4
    //     if (Keyboard.current.digit1Key.wasPressedThisFrame)
    //     {
    //         UI.I.buildButtons[0].Button_Pressed();
    //     }
    //     else if (Keyboard.current.digit2Key.wasPressedThisFrame)
    //     {
    //         UI.I.buildButtons[1].Button_Pressed();
    //     }
    //     else if (Keyboard.current.digit3Key.wasPressedThisFrame)
    //     {
    //         UI.I.buildButtons[2].Button_Pressed();
    //     }
    //     else if (Keyboard.current.digit4Key.wasPressedThisFrame)
    //     {
    //         UI.I.buildButtons[3].Button_Pressed();
    //     }
    // }

    // Handle hotkeys.
    // TBD: Allow hotkeys to be edited.
    public void HandleHotkeys()
    {
        // Ignore inputs during other factions' turns.
        if (GM.I.activeFaction != GM.I.playerFaction) return;

        // - Next ship
        // (and End Turn)
        if (Keyboard.current.spaceKey.wasReleasedThisFrame)
        {
            // Check if we have another ship to select.
            if (UI.I.selectNextShipButton.gameObject.activeSelf)
            {
                GM.I.Button_SelectNextShip();
            }
            // Otherwise check for End Turn button
            else if (UI.I.endTurnButton.gameObject.activeSelf)
            {
                GM.I.Button_EndTurn();
            }
        }


        // - Build hotkeys

        // Check number keys 1-4
        if (Keyboard.current.digit1Key.wasReleasedThisFrame)
        {
            UI.I.buildButtons[0].Button_Pressed();
        }
        else if (Keyboard.current.digit2Key.wasReleasedThisFrame)
        {
            UI.I.buildButtons[1].Button_Pressed();
        }
        else if (Keyboard.current.digit3Key.wasReleasedThisFrame)
        {
            UI.I.buildButtons[2].Button_Pressed();
        }
        else if (Keyboard.current.digit4Key.wasReleasedThisFrame)
        {
            UI.I.buildButtons[3].Button_Pressed();
        }
    }
}