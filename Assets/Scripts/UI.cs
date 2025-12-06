using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI : MonoBehaviour
{
    [Header("Fade in/out")]
    public float fadeTime = 1f;
    public Image overlayBG;
    public Image packOverlay;
    public Image covenOverlay;
    public Image syndicateOverlay;
    public Image neutralOverlay;

    [Header("Top Bar")]
    public TMP_Text currentMana;
    public Image toggleEdgePanning;
    public Button endTurnButton;
    public Button selectNextShipButton;

    [Header("Post Game")]
    public GameObject postGameParent;
    public Image postgameBackground;

    [Header("Tooltips")]
    // Parent object of our selection tooltips.
    public GameObject selectedTooltipParent;

    // Selected tile.
    public TMP_Text selectedTileName;
    public TMP_Text selectedTileMoveCost;
    public TMP_Text selectedTileArmor;

    // Selected ship.
    public GameObject selectedShipParent;
    public Image selectedShipFactionIcon;
    public TMP_Text selectedShipName;
    public TMP_Text selectedShipCurrentHealth;
    public TMP_Text selectedShipMaxHealth;
    public TMP_Text selectedShipMovesRemaining;
    public TMP_Text selectedShipSpeed;
    public TMP_Text selectedShipRange;
    public TMP_Text selectedShipVision;
    public TMP_Text selectedShipDamage;
    public TMP_Text selectedShipArmor;

    // Parent object of our hover tooltips.
    public GameObject hoveredTooltipParent;

    // Hovered tile.
    public TMP_Text hoveredTileName;
    public TMP_Text hoveredTileMoveCost;
    public TMP_Text hoveredTileArmor;

    // Hovered ship.
    public GameObject hoveredShipParent;
    public Image hoveredShipFactionIcon;
    public TMP_Text hoveredShipName;
    public TMP_Text hoveredShipCurrentHealth;
    public TMP_Text hoveredShipMaxHealth;
    public TMP_Text hoveredShipMovesRemaining;
    public TMP_Text hoveredShipSpeed;
    public TMP_Text hoveredShipRange;
    public TMP_Text hoveredShipVision;
    public TMP_Text hoveredShipDamage;
    public TMP_Text hoveredShipArmor;


    // Singleton
    public static UI I;

    void Awake()
    {
        // Enforce singleton pattern.
        if (I == null)
            I = this;
        else
            Destroy(this);

        // Disable what should not be.
        ClearSelection();

        packOverlay.gameObject.SetActive(false);
        covenOverlay.gameObject.SetActive(false);
        syndicateOverlay.gameObject.SetActive(false);
        neutralOverlay.gameObject.SetActive(false);

        postGameParent.SetActive(false);

        // Fade in from black.
        overlayBG.color = new Color(0f, 0f, 0f, 1f);
    }

    // Set up the UI for a new turn for the given faction.
    public void NewTurn(Faction faction)
    {
        // Get the faction's leader.
        Leader leader = GM.I.leaders[faction];

        // Set mana text.
        currentMana.text = leader.mana.ToString();

        // Move camera to leader's position.
        Utility.MoveCamera(leader.currentTile);

        // Reveal select ship button for the player.
        if (faction == GM.I.playerFaction)
            selectNextShipButton.gameObject.SetActive(true);
            // endTurnButton.gameObject.SetActive(true);
    }

    // Handle the UI for ending a turn for a given faction.
    public void EndTurn(Faction faction)
    {
        // Fade out the screen
        if (faction == GM.I.playerFaction)
            FadeOverlay(true, fadeTime);
    }

    // Set up the UI for a newly hovered tile.
    public void HoverTile(Tile hoveredTile)
    {
        // - Tile

        // Clear if hovered tile is null or hidden in fog of war.
        if (hoveredTile == null || !hoveredTile.isVisibleToPlayer)
        {
            hoveredTooltipParent.SetActive(false);
            return;
        }

        // Set up tooltip.
        hoveredTileName.text = hoveredTile.myType.ToString();
        hoveredTileMoveCost.text = hoveredTile.moveCost.ToString();
        hoveredTileArmor.text = hoveredTile.armorBonus.ToString();

        // - Ship
        Ship hoveredShip = hoveredTile.ship;

        // Clear if selecting nothing.
        if (hoveredShip == null)
        {
            // Hide hovered ship tooltip.
            hoveredShipParent.SetActive(false);

            // Return!
            return;
        } else {
            // Reveal hovered ship tooltip.
            hoveredShipParent.SetActive(true);
        }
        
        // Load faction icon.
        Utility.LoadFactionIcon(hoveredShipFactionIcon, hoveredShip.faction);

        // Load text.
        hoveredShipName.text = hoveredShip.myName;
        hoveredShipCurrentHealth.text = hoveredShip.currentHealth.ToString();
        hoveredShipMaxHealth.text = hoveredShip.maxHealth.ToString();
        hoveredShipMovesRemaining.text = hoveredShip.movementRemaining.ToString();;
        hoveredShipSpeed.text = hoveredShip.speed.ToString();
        hoveredShipRange.text = hoveredShip.range.ToString();
        hoveredShipVision.text = hoveredShip.vision.ToString();
        hoveredShipDamage.text = hoveredShip.damage.ToString();
        hoveredShipArmor.text = hoveredShip.armor.ToString();

        // Make sure tooltip is visible!
        hoveredTooltipParent.SetActive(true);
    }

    // Set up the UI for a newly selected tile.
    public void SelectTile(Tile selectedTile)
    {
        // - Tile

        // Clear if selecting nothing or attempting to select in fog of war.
        if (selectedTile == null || !selectedTile.isVisibleToPlayer)
        {
            // Deactivate parent object.
            selectedTooltipParent.SetActive(false);

            // Return!
            return;
        }

        // Set up selected tile tooltip.
        selectedTileName.text = selectedTile.myType.ToString();
        selectedTileMoveCost.text = selectedTile.moveCost.ToString();
        selectedTileArmor.text = selectedTile.armorBonus.ToString();

        // - Ship
        Ship selectedShip = selectedTile.ship;

        // Clear if selecting nothing.
        if (selectedShip == null)
        {
            // Deactivate parent object.
            selectedShipParent.SetActive(false);

            // Return!
            return;
        } else {
            // Activate!
            selectedShipParent.SetActive(true);
        }

        // Load faction icon.
        Utility.LoadFactionIcon(selectedShipFactionIcon, selectedShip.faction);

        // Load text.
        selectedShipName.text = selectedShip.myName;
        selectedShipCurrentHealth.text = selectedShip.currentHealth.ToString();
        selectedShipMaxHealth.text = selectedShip.maxHealth.ToString();
        selectedShipMovesRemaining.text = selectedShip.movementRemaining.ToString();;
        selectedShipSpeed.text = selectedShip.speed.ToString();
        selectedShipRange.text = selectedShip.range.ToString();
        selectedShipVision.text = selectedShip.vision.ToString();
        selectedShipDamage.text = selectedShip.damage.ToString();
        selectedShipArmor.text = selectedShip.armor.ToString();

        // Ensure selection tooltips are visible.
        selectedTooltipParent.SetActive(true);
    }

    // Clear the selection UI.
    public void ClearSelection()
    {
        // Hide selection tooltips.
        selectedTooltipParent.SetActive(false);

        // Hide ship selection specifically.
        selectedShipParent.SetActive(false);
    }

    // Fade the overlay in or out.
    public void FadeOverlay(bool fadeIn = false, float duration = 0.5f)
    {
        float targetAlpha = fadeIn ? 1f : 0f;
        Utility.FadeImage(overlayBG, targetAlpha, duration);
    }

    // - Post Game

    // Victory!
    public void Victory()
    {
        // Load background.
        Utility.LoadImage(postgameBackground, "Backgrounds/Victory 1");

        // Shared post game.
        PostGame();
    }

    // Defeat :(
    public void Defeat()
    {
        // Load background.
        Utility.LoadImage(postgameBackground, "Backgrounds/Defeat 1");

        // Shared post game.
        PostGame();
    }

    // Shared post game stuff.
    public void PostGame()
    {
        // Activate post game parent.
        postGameParent.SetActive(true);
    }
}
