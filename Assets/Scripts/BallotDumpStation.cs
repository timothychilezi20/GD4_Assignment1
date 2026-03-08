using UnityEngine;

public class BallotDumpStation : MonoBehaviour
{
    public int votesPerBallot = 1;

    public void DepositBallots(PlayerController player)
    {
        int ballots = player.GetBallotCount();

        if (ballots <= 0)
        {
            UIManager.Instance?.ShowPlayerMessage(
                player.GetPlayerNumber(),
                "No ballots to deposit!",
                Color.yellow
            );
            return;
        }

        int votesEarned = ballots * votesPerBallot;

        player.AddVotes(votesEarned);
        player.ClearBallots();

        UIManager.Instance?.ShowPlayerMessage(
            player.GetPlayerNumber(),
            $"+{votesEarned} Votes deposited!",
            Color.green
        );
    }
}