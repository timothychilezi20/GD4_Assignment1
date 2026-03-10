using UnityEngine;

public class FoodPenaltySystem : MonoBehaviour
{
    [SerializeField] private float teacherReputationPenalty = -0.15f;

    public void ApplyPenalty(PlayerController player)
    {
        if (player == null) return;

        if (ReputationManager.Instance != null)
        {
            ReputationManager.Instance.ModifyReputation(
                player.GetPlayerNumber(),
                NPCMovement.NPCGroup.Teacher,
                teacherReputationPenalty
            );
        }

        UIManager.Instance?.ShowPlayerMessage(
            player.GetPlayerNumber(),
            "You dropped food! Teachers are angry!",
            Color.red
        );
    }
}