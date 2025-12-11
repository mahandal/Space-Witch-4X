using UnityEngine;

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
    }
}