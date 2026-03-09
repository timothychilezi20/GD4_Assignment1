using UnityEngine;

public class TeacherSuspicion : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 4f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Suspicion Rules")]
    [SerializeField] private float suspiciousRepThreshold = 0.7f;
    [SerializeField] private float caughtRepPenalty = -0.1f;
    [SerializeField] private float playerSlowMultiplier = 0.5f;
    [SerializeField] private float playerSlowDuration = 1.5f;
    [SerializeField] private float warningCooldown = 3f;

    [Header("Confiscation")]
    [SerializeField] private bool confiscateItems = true;
    [SerializeField] private bool destroyConfiscatedItem = true;
    [SerializeField] private string confiscationMessage = "Teacher confiscated your item!";

    [Header("Feedback")]
    [SerializeField] private string suspiciousMessage = "A teacher is watching you!";
    [SerializeField] private string caughtMessage = "Teacher caught you acting suspicious!";
    [SerializeField] private Color suspiciousColor = Color.yellow;
    [SerializeField] private Color caughtColor = Color.red;

    private float lastWarningTime = -999f;

    void Update()
    {
        CheckForSuspiciousPlayers();
    }

    void CheckForSuspiciousPlayers()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);

        foreach (Collider hit in hits)
        {
            PlayerController player = hit.GetComponent<PlayerController>();
            if (player == null) continue;

            if (ReputationManager.Instance == null) continue;

            float teacherRep = ReputationManager.Instance.GetReputation(
                player.GetPlayerNumber(),
                NPCMovement.NPCGroup.Teacher
            );

            if (teacherRep < suspiciousRepThreshold)
            {
                HandleSuspiciousPlayer(player);
            }
        }
    }

    void HandleSuspiciousPlayer(PlayerController player)
    {
        if (Time.time < lastWarningTime + warningCooldown)
            return;

        lastWarningTime = Time.time;

        UIManager.Instance?.ShowPlayerMessage(
            player.GetPlayerNumber(),
            suspiciousMessage,
            suspiciousColor
        );

        if (ReputationManager.Instance != null)
        {
            ReputationManager.Instance.ModifyReputation(
                player.GetPlayerNumber(),
                NPCMovement.NPCGroup.Teacher,
                caughtRepPenalty
            );
        }

        UIManager.Instance?.ShowPlayerMessage(
            player.GetPlayerNumber(),
            caughtMessage,
            caughtColor
        );

        player.ApplyMovementSlow(playerSlowMultiplier, playerSlowDuration);

        if (confiscateItems && player.GetHeldItem() != ItemType.None)
        {
            ConfiscatePlayerItem(player);
        }
    }

    void ConfiscatePlayerItem(PlayerController player)
    {
        WorldItem heldWorldItem = player.GetHeldWorldItem();
        ItemType heldItemType = player.GetHeldItem();

        if (heldWorldItem == null)
        {
            // fallback if player only tracks type
            player.ClearHeldItem();

            UIManager.Instance?.ShowPlayerMessage(
                player.GetPlayerNumber(),
                confiscationMessage,
                caughtColor
            );
            return;
        }

        if (destroyConfiscatedItem)
        {
            Destroy(heldWorldItem.gameObject);
            player.ForceClearHeldItemReference();
        }
        else
        {
            Vector3 dropPos = transform.position + transform.forward + Vector3.up * 0.5f;
            heldWorldItem.DropItem(dropPos);
            player.ForceClearHeldItemReference();
        }

        UIManager.Instance?.ShowPlayerMessage(
            player.GetPlayerNumber(),
            $"{confiscationMessage} ({heldItemType})",
            caughtColor
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}