using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class NPCMovement : MonoBehaviour
{
    public enum NPCGroup
    {
        Nerd,
        Athlete,
        Artist,
        Teacher,
        Grade8
    }

    [Header("NPC Configuration")]
    [SerializeField] private NPCGroup group;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float waitTimeAtWaypoint = 2f;

    [Header("Vote System")]
    [SerializeField] private int baseVotes = 5;
    [SerializeField] private int minVotes = 1;
    [SerializeField] private int maxVotes = 20;
    [SerializeField] private float populationMultiplier = 0.5f;
    [SerializeField] private GameObject votePickupPrefab;
    [SerializeField] private Transform voteDropOffset;

    [Header("Dump Zones")]
    [SerializeField] private Transform currentDumpZone;

    // NPC state
    private NavMeshAgent agent;
    private RoundManager roundManager;
    private WaypointZone currentZone;
    private bool isWaiting = false;
    private Coroutine waitRoutine;
    private int heldVotes = 0;

    // Population tracking
    private static Dictionary<(NPCGroup, int), int> groupPopulation = new Dictionary<(NPCGroup, int), int>();

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
        }

        if (voteDropOffset == null)
            voteDropOffset = transform;
    }

    void Start()
    {
        roundManager = FindFirstObjectByType<RoundManager>();
        if (roundManager == null)
        {
            Debug.LogError("RoundManager not found in scene!");
            return;
        }

        RegisterNPC();
        MoveToNewZone();
    }

    void OnDestroy()
    {
        UnregisterNPC();
    }

    void RegisterNPC()
    {
        int round = roundManager != null ? roundManager.currentRound : 1;
        var key = (group, round);

        if (groupPopulation.ContainsKey(key))
            groupPopulation[key]++;
        else
            groupPopulation[key] = 1;

        Debug.Log($"Registered {group} in Round {round}. Total: {GetGroupPopulation(group, round)}");
    }

    void UnregisterNPC()
    {
        int round = roundManager != null ? roundManager.currentRound : 1;
        var key = (group, round);

        if (groupPopulation.ContainsKey(key))
        {
            groupPopulation[key]--;
            if (groupPopulation[key] <= 0)
                groupPopulation.Remove(key);
        }
    }

    public static int GetGroupPopulation(NPCGroup group, int round)
    {
        var key = (group, round);
        return groupPopulation.ContainsKey(key) ? groupPopulation[key] : 0;
    }

    public void DropVotes()
    {
        int population = GetGroupPopulation(group, roundManager.currentRound);
        int votesToDrop = CalculateVoteValue(population);

        heldVotes = votesToDrop; // Store the votes

        if (votesToDrop > 0 && votePickupPrefab != null)
        {
            Vector3 dropPosition = voteDropOffset != null ? voteDropOffset.position : transform.position + Vector3.up;

            GameObject droppedVotes = Instantiate(votePickupPrefab, dropPosition, Quaternion.identity);
            DroppedVotes voteComponent = droppedVotes.GetComponent<DroppedVotes>();

            if (voteComponent != null)
            {
                voteComponent.Initialize(votesToDrop, group);

                Rigidbody rb = droppedVotes.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = new Vector3(Random.Range(-2f, 2f), 2f, Random.Range(-2f, 2f));
                }
            }

            Debug.Log($"{group} dropped {votesToDrop} votes (Population: {population})");
        }
    }

    int CalculateVoteValue(int population)
    {
        float dynamicValue = baseVotes + (population * populationMultiplier);

        float randomFactor = Random.Range(0.9f, 1.1f);
        dynamicValue *= randomFactor;

        return Mathf.Clamp(Mathf.RoundToInt(dynamicValue), minVotes, maxVotes);
    }

    public void MoveToNewZone()
    {
        if (roundManager == null) return;

        UnregisterNPC();

        WaypointZone newZone = GetZoneForCurrentRound();

        if (newZone != null && newZone != currentZone)
        {
            currentZone = newZone;
            Transform target = currentZone.GetRandomWaypoint();

            if (target != null && agent != null && agent.isActiveAndEnabled)
            {
                agent.SetDestination(target.position);
                isWaiting = false;

                if (waitRoutine != null)
                {
                    StopCoroutine(waitRoutine);
                    waitRoutine = null;
                }
            }
        }

        RegisterNPC();
    }

    // ========== PUBLIC GETTERS AND SETTERS ==========

    public int GetHeldVotes()
    {
        return heldVotes;
    }

    public int GetCurrentVoteValue()
    {
        int population = GetGroupPopulation(group, roundManager != null ? roundManager.currentRound : 1);
        return CalculateVoteValue(population);
    }

    public void SetHeldVotes(int amount)
    {
        heldVotes = amount;
    }

    public NPCGroup GetGroup() => group;

    public bool IsMoving() => agent != null && agent.velocity.magnitude > 0.1f;

    public void SetDumpZone(Transform dumpZone)
    {
        currentDumpZone = dumpZone;
    }

    public void MoveToDumpZone()
    {
        if (currentDumpZone != null && agent != null)
        {
            agent.SetDestination(currentDumpZone.position);
        }
    }

    // ========== MOVEMENT ==========

    void Update()
    {
        if (agent == null || !agent.isActiveAndEnabled) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f && !isWaiting)
        {
            StartWaiting();
        }
    }

    void StartWaiting()
    {
        if (waitRoutine != null)
            StopCoroutine(waitRoutine);

        waitRoutine = StartCoroutine(WaitAtWaypoint());
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTimeAtWaypoint);

        if (currentZone != null)
        {
            Transform nextWaypoint = currentZone.GetRandomWaypoint();
            if (nextWaypoint != null)
            {
                agent.SetDestination(nextWaypoint.position);
            }
        }

        isWaiting = false;
        waitRoutine = null;
    }

    // ========== ROUND ZONE SELECTION ==========

    WaypointZone GetZoneForCurrentRound()
    {
        int round = roundManager.currentRound;

        return round switch
        {
            1 => GetRoundOneZone(),
            2 => GetRoundTwoZone(),
            3 => roundManager.assemblyHall,
            _ => null
        };
    }

    WaypointZone GetRoundOneZone()
    {
        return group switch
        {
            NPCGroup.Nerd => roundManager.mathCore,
            NPCGroup.Athlete => roundManager.gymClass,
            NPCGroup.Artist => roundManager.artClass,
            NPCGroup.Teacher => roundManager.staffLounge,
            NPCGroup.Grade8 => roundManager.zuluClass,
            _ => null
        };
    }

    WaypointZone GetRoundTwoZone()
    {
        return group switch
        {
            NPCGroup.Nerd => roundManager.staffLounge,
            NPCGroup.Athlete => roundManager.gymClass,
            NPCGroup.Artist => roundManager.tuckShop,
            NPCGroup.Teacher => roundManager.mathCore,
            NPCGroup.Grade8 => roundManager.afrClass,
            _ => null
        };
    }

    // ========== COLLISION ==========

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.IsDashing())
            {
                DropVotes();
                player.OnDashIntoNPC(group);
            }
        }

        if (group == NPCGroup.Teacher && other.CompareTag("DroppedFood"))
        {
            Debug.Log("Teacher got angry at dropped food!");
            Destroy(other.gameObject);
        }
    }

    // ========== GIZMOS ==========

    void OnDrawGizmosSelected()
    {
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, agent.destination);
            Gizmos.DrawWireSphere(agent.destination, 0.3f);
        }

        // Show held votes
        if (heldVotes > 0)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2, 0.2f + (heldVotes * 0.01f));
        }
    }
}