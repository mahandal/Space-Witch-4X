using UnityEngine;
using UnityEngine.EventSystems;

public class AutoPilotButton : MonoBehaviour,  IPointerEnterHandler, IPointerExitHandler
{
    [Header("Auto Pilot Mode")]
    public AutoPilotMode apm = AutoPilotMode.Full;

    [TextArea(3, 7)]
    public string description = "";


    public void OnPointerEnter(PointerEventData eventData)
    {
        Hover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Unhover();
    }

    // Called when you hover this button.
    // TBD!
    public void Hover()
    {
        // Bool.
        UI.I.isHoveringUIElement = true;

        UI.I.HoverAutoPilotButton(this);
    }

    // Called when you stop hovering this button.
    public void Unhover()
    {
        // Bool.
        UI.I.isHoveringUIElement = false;

        UI.I.UnhoverAutoPilotButton();
    }
}
