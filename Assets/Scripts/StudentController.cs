using UnityEngine;
using UnityEngine.AI;

public enum NPCMood
{
    Friendly,
    Neutral,
    Suspicious,
    Angry
}

public class StudentController : MonoBehaviour
{
    [Header("Components")]
    private NavMeshAgent agent;

    [Header("Animation")]
    public Animator animator;
    


    [Header("Student Info")]
    public GroupType1 groupType;
    public int studentID;
    public Pack currentPack;

    [Header("Movement Settings")]
    public float wanderRadius = 5f;
    public float wanderTimer = 5f;
    private float timer;

    [Header("Personality")]
    public NPCMood currentMood = NPCMood.Neutral;


    [Header("Gathering")]
    [SerializeField] private float gatherRadius = 2.5f;
    [SerializeField] private float gatherMoveSpeed = 4f;

    private bool isGathering = false;
    private Transform gatherTarget;
    private float gatherTimer = 0f;
    private float gatherDuration = 5f;

    private PlayerController nearbyPlayer;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isGathering)
        {
            HandleGathering();
            return;
        }

        timer += Time.deltaTime;

        if (timer >= wanderTimer && !agent.pathPending)
        {
            Vector3 newPos = GetRandomPointInHangout();
            agent.SetDestination(newPos);
            animator.SetBool("Walk", true);
            timer = 0;
        }
    }

    public void StartGathering(Transform target, float duration = 5f)
    {
        if (target == null) return;

        gatherTarget = target;
        gatherDuration = duration;
        gatherTimer = duration;
        isGathering = true;

        if (agent != null)
            agent.speed = gatherMoveSpeed;
    }

    private void HandleGathering()
    {
        if (gatherTarget == null)
        {
            StopGathering();
            return;
        }

        gatherTimer -= Time.deltaTime;

        if (gatherTimer <= 0f)
        {
            StopGathering();
            return;
        }

        Vector3 direction = (transform.position - gatherTarget.position).normalized;
        if (direction == Vector3.zero)
            direction = Random.insideUnitSphere;

        direction.y = 0f;

        Vector3 targetPos = gatherTarget.position + direction * gatherRadius;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, gatherRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    public void StopGathering()
    {
        isGathering = false;
        gatherTarget = null;
        timer = 0f;

        if (agent != null)
            agent.speed = 3.5f; // or your normal student speed
    }

    private Vector3 GetRandomPointInHangout()
    {
        if (currentPack != null && currentPack.currentHangout != null)
        {
            Vector3 randomDir = Random.insideUnitSphere * currentPack.currentHangout.zoneRadius;
            randomDir.y = 0;
            return currentPack.currentHangout.transform.position + randomDir;
        }

        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
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

            Vector3 targetPos = hangout.transform.position +
                (Random.insideUnitSphere * hangout.zoneRadius);

            targetPos.y = 0;

            agent.SetDestination(targetPos);
            animator.SetBool("Walk", true);
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
     player.GetPlayerNumber(),
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

            UIManager.Instance?.HideInteractPrompt(player.GetPlayerNumber());
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

        NPCMovement.NPCGroup npcGroup = ConvertGroup(groupType);

        float rep = ReputationManager.Instance.GetReputation(
            player.GetPlayerNumber(),
            npcGroup
        );

        float requiredRep = currentMood switch
        {
            NPCMood.Friendly => 0.2f,
            NPCMood.Neutral => 0.4f,
            NPCMood.Suspicious => 0.6f,
            NPCMood.Angry => 0.8f,
            _ => 0.4f
        };

        if (rep < requiredRep)
        {
            UIManager.Instance?.ShowPlayerMessage(
                player.GetPlayerNumber(),
                "They refuse to trade with you!",
                Color.red
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
    }

    void AcceptTrade(PlayerController player, ItemType item)
    {
        int reward = ItemGroupHelper.GetVoteReward(item);

        player.AddVotes(reward);

        WorldItem heldWorldItem = player.GetHeldWorldItem();

        if (heldWorldItem != null && heldWorldItem.HasRareRule())
        {
            NPCMovement.NPCGroup rareTarget = heldWorldItem.GetRareTargetGroup();
            float repChange = heldWorldItem.GetRareReputationChange();

            if (ReputationManager.Instance != null)
            {
                ReputationManager.Instance.ModifyReputation(
                    player.GetPlayerNumber(),
                    rareTarget,
                    repChange
                );
            }

            string repText = repChange >= 0 ? "Reputation increased!" : "Reputation decreased!";

            UIManager.Instance?.ShowPlayerMessage(
                player.GetPlayerNumber(),
                repText,
                repChange >= 0 ? Color.green : Color.red
            );
        }

        player.ClearHeldItem();

        UIManager.Instance?.ShowPlayerMessage(
            player.GetPlayerNumber(),
            $"+{reward} Votes!",
            Color.green
        );

        NPCMovement.NPCGroup npcGroup = ConvertGroup(groupType);
        ReputationManager.Instance.ModifyReputation(player.GetPlayerNumber(), npcGroup, 0.1f);

        ChangeMoodAfterTrade(true);
    }

    void RejectTrade(PlayerController player)
    {
        UIManager.Instance?.ShowPlayerMessage(
            player.GetPlayerNumber(),
            "They don't want that item!",
            Color.red
        );

        NPCMovement.NPCGroup npcGroup = ConvertGroup(groupType);

        ReputationManager.Instance.ModifyReputation(
            player.GetPlayerNumber(),
            npcGroup,
            -0.05f
        );

        ChangeMoodAfterTrade(false);
    }

    void ChangeMoodAfterTrade(bool success)
    {
        if (success)
        {
            if (currentMood == NPCMood.Angry)
                currentMood = NPCMood.Suspicious;
            else if (currentMood == NPCMood.Suspicious)
                currentMood = NPCMood.Neutral;
        }
        else
        {
            if (currentMood == NPCMood.Friendly)
                currentMood = NPCMood.Neutral;
            else if (currentMood == NPCMood.Neutral)
                currentMood = NPCMood.Suspicious;
        }
    }

    public static NPCMovement.NPCGroup ConvertGroup(GroupType1 group)
    {
        return group switch
        {
            GroupType1.Athlete => NPCMovement.NPCGroup.Athlete,
            GroupType1.Nerd => NPCMovement.NPCGroup.Nerd,
            GroupType1.Artist => NPCMovement.NPCGroup.Artist,
            _ => NPCMovement.NPCGroup.Grade8
        };
    }
}