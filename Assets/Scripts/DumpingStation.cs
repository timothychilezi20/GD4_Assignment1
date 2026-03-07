using UnityEngine;
using UnityEngine.UI;


public class DumpingStation : MonoBehaviour
{
    public GroupType stationType;
    public int totalVotes;
    public Text voteText;

    public void ReceiveBallots(int amount, GroupType ballotType)
    {
        int multiplier = (ballotType == stationType) ? 2: 1;

        int votesAdded = amount * multiplier;
        totalVotes += votesAdded;

        GameManager.Instance.AddVotes(stationType, votesAdded);
        Debug.Log($"Added {votesAdded} votes to {stationType}");

        UpdateVoteUI();
    }

    private void UpdateVoteUI()
    {
        voteText.text = "Votes: " + totalVotes.ToString();
    }

}
