using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class UI : MonoBehaviour
{
    [Header("Top Bar")]
    public TMP_Text currentMana;
    public List<BuildButton> buildButtons;
    public Button endTurnButton;
    public Button selectNextShipButton;

    [Header("Post Game")]
    public GameObject postGameParent;
    public Image postgameBackground;

    [Header("Settings")]
    public Image toggleEdgePanning;

    // [Header("Tooltips")]

    [Header("Tooltips - Selection")]
    // Parent object of our selection tooltips.
    public GameObject selectedTooltipParent;

    [Header("Selected Tile")]
    // Selected tile.
    public TMP_Text selectedTileName;
    public TMP_Text selectedTileMoveCost;
    public TMP_Text selectedTileArmor;

    [Header("Selected Ship")]
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

    [Header("Selected Ship Buttons")]

    // Parent object of behavior buttons, only available for ships we own.
    public GameObject selectedShipButtonsParent;

    // The button to recycle a ship, hidden for your leader.
    public GameObject recycleShipButton;

    // The text object displaying how much mana you'll gain from recycling a ship.
    public TMP_Text recycleMana;

    // - Auto pilot
    public Image autoPilotOffIcon;
    public Image autoPilotExploreIcon;
    public Image autoPilotRestIcon;
    public Image autoPilotGuardIcon;
    public Image autoPilotHuntIcon;
    public Image autoPilotFullIcon;

    [Header("Tooltips - Hover")]
    // Parent object of our hover tooltips.
    public GameObject hoveredTooltipParent;

    [Header("Hovered Tile")]
    // Hovered tile.
    public TMP_Text hoveredTileName;
    public TMP_Text hoveredTileMoveCost;
    public TMP_Text hoveredTileArmor;

    [Header("Hovered Ship")]
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
        postGameParent.SetActive(false);
    }

    

    // - Selection & Hovering

    // Set up the UI for a newly hovered tile.
    public void HoverTile(Tile hoveredTile)
    {
        // - Tile

        // Clear if hovered tile is null or hidden in fog of war.
        if (hoveredTile == null || !hoveredTile.isVisibleToPlayer)
        {
            hoveredTooltipParent.SetActive(false);
            return;
        } else {
            // Make sure tooltip is visible!
            hoveredTooltipParent.SetActive(true);
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
        hoveredShipArmor.text = hoveredShip.armor.ToString();
        // hoveredShipDamage.text = hoveredShip.damage.ToString();
        string damageText = hoveredShip.damage.ToString();
        if (hoveredShip.attacks > 1)
            damageText += "x" + hoveredShip.attacks.ToString();
        hoveredShipDamage.text = damageText;
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

        // Check if we are selecting a friendly ship and should reveal behavior buttons.
        selectedShipButtonsParent.SetActive(selectedShip.faction == GM.I.playerFaction);

        // Check if we are selecting our leader and should hide the recycle button.
        recycleShipButton.SetActive(selectedShip != GM.I.leaders[GM.I.playerFaction]);

        // Display current recycle value.
        recycleMana.text = selectedShip.GetRecycleValue().ToString();

        // Highlight the currently active auto pilot mode.
        HighlightAutoPilot();

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
        // selectedShipDamage.text = selectedShip.damage.ToString();
        selectedShipArmor.text = selectedShip.armor.ToString();

        string damageText = selectedShip.damage.ToString();
        if (selectedShip.attacks > 1)
            damageText += "x" + selectedShip.attacks.ToString();
        selectedShipDamage.text = damageText;

        // Ensure selection tooltips are visible.
        selectedTooltipParent.SetActive(true);
    }

    // Highlight the currently active auto pilot mode.
    public void HighlightAutoPilot()
    {
        // Check we are selecting a ship.
        if (GM.I.selectedTile == null || GM.I.selectedTile.ship == null) return;

        // First, unhighlight all of them.
        autoPilotOffIcon.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
        autoPilotExploreIcon.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
        autoPilotRestIcon.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
        autoPilotGuardIcon.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
        autoPilotHuntIcon.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
        autoPilotFullIcon.color = new Color (0.5f, 0.5f, 0.5f, 0.5f);

        // Get the currently selected ship.
        Ship ship = GM.I.selectedTile.ship;

        // Activate the right one.
        if (ship.autoPilotMode == AutoPilotMode.Off)
            autoPilotOffIcon.color = new Color(1f, 1f, 1f, 1f);
        else if (ship.autoPilotMode == AutoPilotMode.Explore)
            autoPilotExploreIcon.color = new Color(1f, 1f, 1f, 1f);
        else if (ship.autoPilotMode == AutoPilotMode.Rest)
            autoPilotRestIcon.color = new Color(1f, 1f, 1f, 1f);
        else if (ship.autoPilotMode == AutoPilotMode.Guard)
            autoPilotGuardIcon.color = new Color(1f, 1f, 1f, 1f);
        else if (ship.autoPilotMode == AutoPilotMode.Hunt)
            autoPilotHuntIcon.color = new Color(1f, 1f, 1f, 1f);
        else if (ship.autoPilotMode == AutoPilotMode.Full)
            autoPilotFullIcon.color = new Color(1f, 1f, 1f, 1f);
        else
            Debug.LogError("ERROR! Ship " + ship.myName + " has unknown auto pilot mode: " + ship.autoPilotMode);
    }

    // Clear the selection UI.
    public void ClearSelection()
    {
        // Hide selection tooltips.
        selectedTooltipParent.SetActive(false);

        // Hide ship selection specifically.
        selectedShipParent.SetActive(false);
    }


    // - Turn Management

    // Set up the UI for a new battle.
    public void SetUp()
    {
        // Load build buttons.
        for (int i = 0; i < buildButtons.Count; i++)
        {
            // Get the player's leader.
            Leader leader = GM.I.leaders[GM.I.playerFaction];

            // Get blueprint
            Ship blueprint = null;
            if (i < leader.blueprints.Count)
                blueprint = leader.blueprints[i];

            // Load blueprint into button.
            buildButtons[i].LoadShip(blueprint);
        }
    }

    // Set up the UI for a new turn for the given faction.
    public void NewTurn(Faction faction)
    {
        // Get the faction's leader.
        Leader leader = GM.I.leaders[faction];

        // Move camera to leader's position.
        Utility.MoveCamera(leader.currentTile);

        // Reveal select ship button for the player.
        if (faction == GM.I.playerFaction)
            WhichButtonInTopRight();
    }

    // Check whether we should display the Select Next Ship button, End Turn button, or neither.
    public void WhichButtonInTopRight()
    {
        // If it is not our turn, then show neither.
        if (GM.I.leaders[GM.I.playerFaction].hasEndedTurn)
        {
            selectNextShipButton.gameObject.SetActive(false);
            endTurnButton.gameObject.SetActive(false);
            return;
        }

        // Get the player's leader.
        Leader leader = GM.I.leaders[GM.I.playerFaction];

        // Don't worry about it if the game is over!
        if (leader == null || leader.currentHealth <= 0)
            return; 

        // Check if our fleet has any actions remaining.
        bool fleetCanAct = leader.CanFleetAct(true);

        // If our fleet has any actions remaining...
        if (fleetCanAct)
        {
            // Reveal the select next ship button!
            selectNextShipButton.gameObject.SetActive(true);

            // Hide the end turn button.
            endTurnButton.gameObject.SetActive(false);
        } else {
            // Fleet has no actions remaining!
            
            // Hide select next ship button.
            selectNextShipButton.gameObject.SetActive(false);

            // Reveal end turn button.
            endTurnButton.gameObject.SetActive(true);
        }
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


    // - Mana
    // Update our mana display.
    // Should be called any time our mana changes.
    public void UpdateManaDisplay()
    {
        // Get the player's leader.
        Leader leader = GM.I.leaders[GM.I.playerFaction];

        // Pause mana display if we've lost.
        if (leader == null) return;

        // Update text display.
        UI.I.currentMana.text = leader.mana.ToString();

        // Update the opacity of our blueprints, reflecting if we can afford them.
        BuildButton.SetOpacityForAll();
    }
}
