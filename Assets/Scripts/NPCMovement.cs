using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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
    [SerializeField] private string npcName;

    [Header("Trading")]
    [SerializeField] private int baseTradeValue = 5;
    [SerializeField] private AudioClip acceptSound;
    [SerializeField] private AudioClip rejectSound;
    [SerializeField] private ParticleSystem acceptEffect;
    [SerializeField] private ParticleSystem rejectEffect;

    [Header("Interaction UI")]
    [SerializeField] private GameObject tradePrompt;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private CanvasGroup promptCanvasGroup;

    private NavMeshAgent agent;
    private RoundManager roundManager;
    private WaypointZone currentZone;
    private bool isWaiting = false;
    private Coroutine waitRoutine;
    private AudioSource audioSource;
    private bool playerInRange = false;
    private PlayerController currentPlayer;

    private static Dictionary<(NPCGroup, int), int> groupPopulation = new Dictionary<(NPCGroup, int), int>();

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (agent != null)
            agent.speed = moveSpeed;

        if (tradePrompt != null)
        {
            promptCanvasGroup.alpha = 0f;
            tradePrompt.SetActive(false);
        }
    }

    void Start()
    {
        roundManager = FindFirstObjectByType<RoundManager>();
        if (roundManager == null) return;

        RegisterNPC();
        MoveToNewZone();
    }

    void OnDestroy()
    {
        UnregisterNPC();

        if (tradePrompt != null && tradePrompt.activeSelf)
            tradePrompt.SetActive(false);
    }

    void Update()
    {
        if (agent == null || !agent.isActiveAndEnabled) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f && !isWaiting)
            StartWaiting();

        if (playerInRange && tradePrompt != null && promptCanvasGroup.alpha < 1f)
        {
            promptCanvasGroup.alpha += Time.deltaTime * 5f;
            UpdateTradePrompt();
        }
        else if (!playerInRange && tradePrompt != null && promptCanvasGroup.alpha > 0f)
        {
            promptCanvasGroup.alpha -= Time.deltaTime * 5f;

            if (promptCanvasGroup.alpha <= 0f && tradePrompt.activeSelf)
                tradePrompt.SetActive(false);
        }
    }

    void UpdateTradePrompt()
    {
        if (promptText == null || currentPlayer == null) return;

        ItemType playerItem = currentPlayer.GetHeldItem();

        if (playerItem == ItemType.None)
        {
            promptText.text = $"{npcName}: Bring me something to trade!";
            promptText.color = Color.white;
        }
        else if (IsDesiredItem(playerItem))
        {
            int value = GetItemTradeValue(playerItem);
            float repMultiplier = 1f;

            if (ReputationManager.Instance != null)
                repMultiplier = ReputationManager.Instance.GetVoteMultiplier(currentPlayer.GetPlayerNumber(), group);

            int totalValue = Mathf.RoundToInt(value * repMultiplier);

            promptText.text = $"{npcName}: I'll give you {totalValue} votes for that!";
            promptText.color = Color.green;
        }
        else
        {
            promptText.text = $"{npcName}: I don't want that...";
            promptText.color = Color.red;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            playerInRange = true;
            currentPlayer = other.GetComponent<PlayerController>();

            if (tradePrompt != null)
                tradePrompt.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            playerInRange = false;
            currentPlayer = null;
        }
    }

    void RegisterNPC()
    {
        int round = roundManager != null ? roundManager.currentRound : 1;
        var key = (group, round);

        if (groupPopulation.ContainsKey(key))
            groupPopulation[key]++;
        else
            groupPopulation[key] = 1;
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

    public bool IsDesiredItem(ItemType item)
    {
        return item switch
        {
            ItemType.Protractor => group == NPCGroup.Nerd,
            ItemType.Basketball => group == NPCGroup.Athlete,
            ItemType.Paintbrush => group == NPCGroup.Artist,
            ItemType.Apple => group == NPCGroup.Teacher,
            ItemType.PrankKit => group == NPCGroup.Grade8,
            _ => false
        };
    }

    public int GetItemTradeValue(ItemType item)
    {
        return item switch
        {
            ItemType.Protractor => 8,
            ItemType.Basketball => 8,
            ItemType.Paintbrush => 8,
            ItemType.Apple => 10,
            ItemType.PrankKit => 5,
            ItemType.Food => 3,
            ItemType.Book => 5,
            ItemType.Pen => 2,
            _ => 1
        };
    }

    public void TryTrade(PlayerController player)
    {
        if (player == null) return;

        ItemType offeredItem = player.GetHeldItem();

        if (offeredItem == ItemType.None)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowAnnouncement("You have nothing to trade!", Color.yellow);
            return;
        }

        if (IsDesiredItem(offeredItem))
            AcceptTrade(player, offeredItem);
        else
            RejectTrade(player);
    }

    void AcceptTrade(PlayerController player, ItemType item)
    {
        int baseVotes = GetItemTradeValue(item);

        float repMultiplier = 1f;
        if (ReputationManager.Instance != null)
            repMultiplier = ReputationManager.Instance.GetVoteMultiplier(player.GetPlayerNumber(), group);

        int votesAwarded = Mathf.RoundToInt(baseVotes * repMultiplier);

        player.AddVotes(votesAwarded);
        player.ClearHeldItem();

        PlayerPoints playerPoints = player.GetComponent<PlayerPoints>();
        if (playerPoints != null)
            playerPoints.AddRoundPoints(votesAwarded);

        if (ReputationManager.Instance != null)
            ReputationManager.Instance.ModifyReputation(player.GetPlayerNumber(), group, 0.05f);

        if (UIManager.Instance != null)
            UIManager.Instance.ShowTradeResult(votesAwarded, group.ToString());

        if (acceptEffect != null)
            acceptEffect.Play();

        if (acceptSound != null && audioSource != null)
            audioSource.PlayOneShot(acceptSound);

        UpdateTradePrompt();
    }

    void RejectTrade(PlayerController player)
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowAnnouncement($"{group} doesn't want that item!", Color.red);

        if (rejectEffect != null)
            rejectEffect.Play();

        if (rejectSound != null && audioSource != null)
            audioSource.PlayOneShot(rejectSound);
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

    public NPCGroup GetGroup() => group;

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
                agent.SetDestination(nextWaypoint.position);
        }

        isWaiting = false;
        waitRoutine = null;
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
}