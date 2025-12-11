using UnityEngine;
using UnityEngine.UI;

public class GGG : MonoBehaviour
{
    [Header("Faction Choice")]
    public Image selectThePack;
    public Image selectTheCoven;
    public Image selectTheSyndicate;
    public Image selectNeutral;

    // Singleton
    public static GGG I;

    void Awake()
    {
        // Enforce singleton pattern.
        if (I == null)
            I = this;
        else
            Destroy(this);
    }

    void Start()
    {
        HighlightCurrentFactionChoice();
    }

    // Lower the opacity of all faction choices,
    // then increase the opacity of the one we currently have selected.
    public void HighlightCurrentFactionChoice()
    {
        // Lower opacity of each faction choice image.
        selectThePack.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
        selectTheCoven.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        selectTheSyndicate.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
        selectNeutral.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        // Find current selection to increase opacity.
        if (GM.I.playerFaction == Faction.Pack)
            selectThePack.color = new Color(1f, 1f, 1f, 1f);

        if (GM.I.playerFaction == Faction.Coven)
            selectTheCoven.color = new Color(1f, 1f, 1f, 1f);

        if (GM.I.playerFaction == Faction.Syndicate)
            selectTheSyndicate.color = new Color(1f, 1f, 1f, 1f);

        if (GM.I.playerFaction == Faction.Neutral)
            selectNeutral.color = new Color(1f, 1f, 1f, 1f);
    }
}
