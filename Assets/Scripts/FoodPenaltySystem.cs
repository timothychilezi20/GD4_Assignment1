using UnityEngine;

public class FoodPenaltySystem : MonoBehaviour
{
    [Header("Penalty Settings")]
    [SerializeField] private float teacherReputationPenalty = -0.15f;

    public void OnFoodDropped(PlayerController player)
    {
        if (player == null) return;

        int playerNumber = player.GetPlayerNumber();

        if (ReputationManager.Instance != null)
        {
            ReputationManager.Instance.ModifyReputation(
                playerNumber,
                NPCMovement.NPCGroup.Teacher,
                teacherReputationPenalty
            );
        }

        UIManager.Instance?.ShowPlayerMessage(
            playerNumber,
            "You dropped food! Teachers are angry!",
            Color.red
        );

        Destroy(gameObject);
    }
}