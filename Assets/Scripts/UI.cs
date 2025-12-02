using UnityEngine;
using TMPro;

public class UI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text activeFaction;
    public TMP_Text currentMana;

    // Singleton
    public static UI I;

    void Awake()
    {
        // Enforce singleton pattern.
        if (I == null)
            I = this;
        else
            Destroy(this);
    }

    // Set up the UI for a new turn for the given faction.
    public void NewTurn(Faction faction)
    {
        // Get the faction's leader.
        Leader leader = GM.I.leaders[faction];

        // Set faction text.
        activeFaction.text = faction.ToString();

        // Set faction color.
        activeFaction.color = Constance.FactionColor(faction);

        // Set mana text.
        currentMana.text = leader.mana.ToString();
    }
}
