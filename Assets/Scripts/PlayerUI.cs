using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("Player Identification")]
    public int playerNumber = 1;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI playerNameText;      
    [SerializeField] private TextMeshProUGUI totalPointsText;
    [SerializeField] private TextMeshProUGUI roundPointsText;
    [SerializeField] private TextMeshProUGUI heldVotesText;

    private PlayerPoints playerPoints;

    public void RegisterPlayer(PlayerPoints player, string playerName = null)
    {
        playerPoints = player;

        // Subscribe to events
        playerPoints.OnTotalPointsChanged += UpdateTotalPoints;
        playerPoints.OnRoundPointsChanged += UpdateRoundPoints;
        playerPoints.OnHeldVotesChanged += UpdateHeldVotes;

        // Initialize UI values
        UpdateTotalPoints(playerNumber, playerPoints.GetTotalPoints());
        UpdateRoundPoints(playerNumber, playerPoints.GetRoundPoints());
        UpdateHeldVotes(playerNumber, playerPoints.GetHeldVotes());

        // Set player name
        if (playerNameText != null)
        {
            playerNameText.text = string.IsNullOrEmpty(playerName) ? $"Player {playerNumber}" : playerName;
        }
    }

    void UpdateTotalPoints(int playerNum, int value)
    {
        if (playerNum != playerNumber) return;
        totalPointsText.text = "Total: " + value;
    }

    void UpdateRoundPoints(int playerNum, int value)
    {
        if (playerNum != playerNumber) return;
        roundPointsText.text = "Round: " + value;
    }

    void UpdateHeldVotes(int playerNum, int value)
    {
        if (playerNum != playerNumber) return;
        heldVotesText.text = "Votes: " + value;
    }

    private void OnDestroy()
    {
        if (playerPoints == null) return;

        playerPoints.OnTotalPointsChanged -= UpdateTotalPoints;
        playerPoints.OnRoundPointsChanged -= UpdateRoundPoints;
        playerPoints.OnHeldVotesChanged -= UpdateHeldVotes;
    }
}