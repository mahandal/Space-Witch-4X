using UnityEngine;

// Handle ship traits & unique abilities.
public partial class Ship : MonoBehaviour
{
    // Called at the beginning of this ship's turn.
    public void OnUpkeep()
    {

    }

    // Called whenever this ship enters a tile.
    public void OnEnter(Tile newTile)
    {
        // Miner
        if (traits.Contains(Trait.Miner) && newTile.myType == TileType.Asteroids)
        {
            // Terraform to air.
            newTile.Terraform(TileType.Air);

            // Gain mana.
            GainManaForFaction(10);
        }
    }
}
