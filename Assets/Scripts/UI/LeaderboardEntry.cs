using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Class for handling each entry in the leaderboard.
public class LeaderboardEntry : MonoBehaviour
{
    [Header("Leaderboard Entry")]
    public Image factionIcon;
    public TMP_Text score;
    public TMP_Text leaderName;

    // Set this leaderboard entry based off the given faction.
    public void UpdateEntry(Faction faction)
    {
        // Load the faction's icon.
        Utility.LoadFactionIcon(factionIcon, faction);

        // Get the faction's leader.
        Leader leader = GM.I.leaders[faction];

        // Set name.
        leaderName.text = leader.myName;
        leaderName.color = Constance.FactionColor(faction);

        // Get score.
        int _score = Leaderboard.GetScore(faction);

        // Set score.
        score.text = _score.ToString();
        score.color = Constance.FactionColor(faction);

        // Cross out name if score is 0.
        if (_score == 0)
        {
            leaderName.text = "<s>" + leader.myName + "</s>";
        }
    }

    
}
