using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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
    [SerializeField] private int heldVotes = 0;
    [SerializeField] private GameObject votePickupPrefab;

    [Header("Dump Zones")]
    [SerializeField] private Transform currentDumpZone;

    private NavMeshAgent agent;
    private RoundManager roundManager;
    private WaypointZone currentZone;
    private bool isWaiting = false;
    private Coroutine waitRoutine;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
        }
    }

    void Start()
    {
        roundManager = FindFirstObjectByType<RoundManager>();
        if (roundManager == null)
        {
            Debug.LogError("RoundManager not found in scene!");
            return;
        }

        MoveToNewZone();
    }

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

    public void MoveToNewZone()
    {
        if (roundManager == null) return;

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
    }

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

    public void AddVotes(int amount)
    {
        heldVotes += amount;
    }

    public void DropVotes()
    {
        if (heldVotes > 0 && votePickupPrefab != null)
        {
            GameObject droppedVotes = Instantiate(votePickupPrefab, transform.position + Vector3.up, Quaternion.identity);
            DroppedVotes voteComponent = droppedVotes.GetComponent<DroppedVotes>();
            if (voteComponent != null)
            {
                voteComponent.Initialize(heldVotes);
            }

            heldVotes = 0;
        }
    }

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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.IsDashing())
            {
                DropVotes();
            }
        }

        if (group == NPCGroup.Teacher && other.CompareTag("DroppedFood"))
        {
            Debug.Log("Teacher got angry at dropped food!");
            Destroy(other.gameObject);
        }
    }

    public NPCGroup GetGroup() => group;
    public int GetHeldVotes() => heldVotes;
    public bool IsMoving() => agent != null && agent.velocity.magnitude > 0.1f;

    void OnDrawGizmosSelected()
    {
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, agent.destination);
            Gizmos.DrawWireSphere(agent.destination, 0.3f);
        }

        if (heldVotes > 0)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2, 0.2f + (heldVotes * 0.01f));
        }
    }
}