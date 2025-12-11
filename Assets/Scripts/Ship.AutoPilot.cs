using UnityEngine;
using System.Collections.Generic;

public partial class Ship : MonoBehaviour
{
    [Header("AUTO-PILOT")]
    public string autoPilot = "Off";

    // Set the auto pilot mode.
    public void SetAutoPilot(string newAutoPilotMode)
    {
        // Set new auto pilot mode.
        autoPilot = newAutoPilotMode;

        // Highlight new auto pilot mode.
        UI.I.HighlightAutoPilot();

        // Check if this was the last ship to manage and we should reveal the end turn button.
        UI.I.WhichButtonInTopRight();
    }

    // Perform this ship's turn, decided by its auto pilot.
    public void AutoPilot()
    {
        // Auto-Pilot mode: Off
        if (autoPilot == "Off")
            return;

        else if (autoPilot == "Explore")
            Explore();
        else if (autoPilot == "Rest")
            AttemptRest();
        else if (autoPilot == "Guard")
            Guard();
        else if (autoPilot == "Full")
            FullAutoPilot();
    }

    // TBD!
    public void Explore()
    {

    }

    // Guard an area, waking up upon seeing an enemy within attack range.
    public void Guard()
    {
        // Look for enemies within attack range.
        List<Ship> enemiesInAttackRange = GetEnemiesInAttackRange();

        // Check if there are any.
        if (enemiesInAttackRange.Count > 0)
        {
            // Wake up!
            autoPilot = "Off";
        }
    }

    // TBD!
    public void FullAutoPilot()
    {

    }
}