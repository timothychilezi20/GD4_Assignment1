using UnityEngine;
using UnityEngine.AI;

public class StudentController : MonoBehaviour
{
    [Header("Components")]
    private NavMeshAgent agent;

    [Header("Student Info")]
    public GroupType1 groupType;
    public int studentID;
    public Pack currentPack;

    [Header("Movement Settings")]
    public float wanderRadius = 5f;
    public float wanderTimer = 5f;
    private float timer;

    private PlayerController nearbyPlayer;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= wanderTimer && !agent.pathPending)
        {
            Vector3 newPos = GetRandomPointInHangout();
            agent.SetDestination(newPos);
            timer = 0;
        }
    }

    private Vector3 GetRandomPointInHangout()
    {
        if (currentPack != null && currentPack.currentHangout != null)
        {
            Vector3 randomDir = UnityEngine.Random.insideUnitSphere * currentPack.currentHangout.zoneRadius;
            randomDir.y = 0;
            return currentPack.currentHangout.transform.position + randomDir;
        }

        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1);
        return hit.position;
    }

    public void MoveToHangout(HangoutZone hangout)
    {
        if (currentPack != null)
        {
            currentPack.currentHangout = hangout;

            Vector3 targetPos = hangout.transform.position + (UnityEngine.Random.insideUnitSphere * hangout.zoneRadius);
            targetPos.y = 0;

            agent.SetDestination(targetPos);
        }
    }

    // =========================
    // PLAYER INTERACTION
    // =========================

    void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            nearbyPlayer = player;

            UIManager.Instance?.ShowInteractPrompt(
                $"Press E to trade with {groupType} student"
            );
        }
    }

    void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null && player == nearbyPlayer)
        {
            nearbyPlayer = null;

            UIManager.Instance?.HideInteractPrompt();
        }
    }

    public void TryTrade(PlayerController player)
    {
        ItemType heldItem = player.GetHeldItem();

        if (heldItem == ItemType.None)
        {
            UIManager.Instance?.ShowPlayerMessage(
                player.GetPlayerNumber(),
                "You have nothing to trade!",
                Color.yellow
            );
            return;
        }

        GroupType1 itemGroup = ItemGroupHelper.GetGroupForItem(heldItem);

        if (itemGroup == groupType)
        {
            AcceptTrade(player, heldItem);
        }
        else
        {
            RejectTrade(player);
        }

        float rep = ReputationManager.Instance.GetReputation(player.GetPlayerNumber(), ConvertGroup(groupType));

        if (rep < 0.6f)
        {
            UIManager.Instance?.ShowPlayerMessage(
                player.GetPlayerNumber(),
                "They refuse to trade with you!",
                Color.red
            );
            return;
        }
    }

    void AcceptTrade(PlayerController player, ItemType item)
    {
        int reward = ItemGroupHelper.GetVoteReward(item);

        player.AddVotes(reward);
        player.ClearHeldItem();

        UIManager.Instance?.ShowPlayerMessage(
            player.GetPlayerNumber(),
            $"+{reward} Votes!",
            Color.green
        );

        NPCMovement.NPCGroup npcGroup = ConvertGroup(groupType);
        ReputationManager.Instance.ModifyReputation(player.GetPlayerNumber(), npcGroup, 0.1f); // increase reputation
    }

    public static NPCMovement.NPCGroup ConvertGroup(GroupType1 group)
    {
        return group switch
        {
            GroupType1.Athlete => NPCMovement.NPCGroup.Athlete,
            GroupType1.Nerd => NPCMovement.NPCGroup.Nerd,
            GroupType1.Artist => NPCMovement.NPCGroup.Artist,
            _ => NPCMovement.NPCGroup.Grade8 // fallback
        };
    }

    void RejectTrade(PlayerController player)
    {
        UIManager.Instance?.ShowPlayerMessage(
            player.GetPlayerNumber(),
            "They don't want that item!",
            Color.red
        );

        NPCMovement.NPCGroup npcGroup = ConvertGroup(groupType);
        ReputationManager.Instance.ModifyReputation(player.GetPlayerNumber(), npcGroup, -0.05f); // decrease reputation
    }
}