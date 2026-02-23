using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Player Identification")]
    [SerializeField] private int playerNumber = 1; // 1 or 2
    [SerializeField] private Color playerColor = Color.blue;
    [SerializeField] private string playerName = "Player 1"; 

    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float dashCooldown = 1.5f;
    [SerializeField] private float dashRange = 2f;

    [Header("Vote System")]
    [SerializeField] private int heldVotes = 0;
    [SerializeField] private int votesLostOnHit = 5;
    [SerializeField] private GameObject votePickupPrefab;

    [Header("Item System")]
    [SerializeField] private ItemType heldItem = ItemType.None;
    [SerializeField] private Transform itemHoldPoint;
    [SerializeField] private float itemPickupRange = 2f;

    //[Header("Reputation System")]
    //[SerializeField] private float teacherRep = 1.0f;
    //[SerializeField] private float athleteRep = 1.0f;
    //[SerializeField] private float artistRep = 1.0f;
    //[SerializeField] private float nerdRep = 1.0f;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayer;

    // Components
    private Rigidbody rb;
    private Renderer playerRenderer;
    private Coroutine dashCoroutine;
    private PlayerInput playerInput; 

    // State
    private Vector2 movementInput;
    private bool isDashing = false;
    private bool canDash = true;
    private GameObject nearbyInteractable;

    // Events
    public System.Action<int, int> OnVotesChanged; // playerNumber, votes
    public System.Action<int, ItemType> OnItemChanged;
    public System.Action<int, float> OnReputationChanged; 

    // References to other player
    private GameObject otherPlayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();
        playerInput = GetComponent<PlayerInput>();

        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
    }

    void Start()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.mass = 1f;
        rb.linearDamping = 5f;

        // Set player color
        if (playerRenderer != null)
        {
            playerRenderer.material.color = playerColor;
        }

        // Set tag based on player number
        gameObject.tag = playerNumber == 1 ? "Player1" : "Player2";

        gameObject.name = playerName; 
    }

    //void FindOtherPlayer()
    //{
    //    if (playerNumber == 1)
    //    {
    //        otherPlayer = GameObject.FindGameObjectWithTag("Player2");
    //    }
    //    else
    //    {
    //        otherPlayer = GameObject.FindGameObjectWithTag("Player1");
    //    }
    //}

    void Update()
    {
        if (!isDashing)
        {
            HandleMovement();
        }

        HandleInteraction();
    }

    void HandleMovement()
    {
        Vector3 moveDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;

        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 targetVelocity = moveDirection * speed;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    // Called by Input System
    public void OnMove(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();
    }

    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && canDash && !isDashing)
        {
            if (dashCoroutine != null)
                StopCoroutine(dashCoroutine);

            dashCoroutine = StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;

        Vector3 dashDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;
        if (dashDirection.magnitude < 0.1f)
        {
            dashDirection = transform.forward;
        }

        // Visual feedback
        if (playerRenderer != null)
            playerRenderer.material.color = Color.white;

        float dashTimer = 0f;
        while (dashTimer < dashDuration)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            CheckDashCollisions();

            dashTimer += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        isDashing = false;

        // Restore color
        if (playerRenderer != null)
            playerRenderer.material.color = playerColor;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void CheckDashCollisions()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, dashRange);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject != gameObject)
            {
                // Check if it's the OTHER player (competitive!)
                PlayerController otherPlayer = hit.GetComponent<PlayerController>();
                if (otherPlayer != null && otherPlayer.GetPlayerNumber() != playerNumber)
                {
                    otherPlayer.TakeDashHit();
                    Debug.Log($"Player {playerNumber} dashed into Player {otherPlayer.GetPlayerNumber()}!"); 
                }

                // Check if it's an NPC
                NPCMovement npc = hit.GetComponent<NPCMovement>();
                if (npc != null)
                {
                    int votesDropped = npc.GetHeldVotes();
                    npc.DropVotes();

                    Debug.Log($"Player {playerNumber} dashed into {npc.GetGroup()}, dropped {votesDropped} votes");

                    // Reputation loss for hitting teacher
                    if (npc.GetGroup() == NPCMovement.NPCGroup.Teacher)
                    {
                        TwoPlayerGameManager.Instance.ModifyReputation(
                            playerNumber,
                            NPCMovement.NPCGroup.Teacher,
                            -0.1f
                        );
                    }
                }
            }
        }
    }

    public void TakeDashHit()
    {
        int votesToDrop = Mathf.Min(votesLostOnHit, heldVotes);
        if (votesToDrop > 0)
        {
            DropVotes(votesToDrop);
        }
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            TryInteract();
        }
    }

    void HandleInteraction()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer);

        if (hits.Length > 0)
        {
            nearbyInteractable = hits[0].gameObject;
            // Show interaction prompt
        }
        else
        {
            nearbyInteractable = null;
        }
    }

    void TryInteract()
    {
        if (nearbyInteractable != null)
        {
            // Check for dropped votes
            DroppedVotes votes = nearbyInteractable.GetComponent<DroppedVotes>();
            if (votes != null)
            {
                int amount = votes.PickUp();
                AddVotes(amount);
                Debug.Log($"Player {playerNumber} picked up {amount} votes");
                return;
            }

            // Check for items
            WorldItem worldItem = nearbyInteractable.GetComponent<WorldItem>();
            if (worldItem != null)
            {
                PickUpItem(worldItem);
                return;
            }

            // Check for dump zones
            DumpZone dumpZone = nearbyInteractable.GetComponent<DumpZone>();
            if (dumpZone != null)
            {
                DumpVotes(dumpZone);
                return;
            }
        }
    }

    public void AddVotes(int amount)
    {
        heldVotes += amount;
        OnVotesChanged?.Invoke(playerNumber, heldVotes);
    }

    void DropVotes(int amount)
    {
        heldVotes -= amount;

        if (votePickupPrefab != null)
        {
            GameObject dropped = Instantiate(votePickupPrefab, transform.position + Vector3.up, Quaternion.identity);
            DroppedVotes droppedComponent = dropped.GetComponent<DroppedVotes>();
            if (droppedComponent != null)
            {
                droppedComponent.Initialize(amount, playerNumber); // Track which player dropped them
            }
        }

        OnVotesChanged?.Invoke(playerNumber, heldVotes);
    }

    void DumpVotes(DumpZone dumpZone)
    {
        if (heldVotes > 0)
        {
            int dumpedVotes = Mathf.RoundToInt(heldVotes * dumpZone.multiplier);

            // Add to team score
            TwoPlayerGameManager.Instance.AddPlayerVotes(dumpedVotes, playerNumber);

            heldVotes = 0;
            OnVotesChanged?.Invoke(playerNumber, heldVotes);

            Debug.Log($"Player {playerNumber} dumped {dumpedVotes} votes at {dumpZone.assignedGroup} zone!");
        }
    }

    void PickUpItem(WorldItem item)
    {
        if (heldItem == ItemType.None)
        {
            heldItem = item.itemType;

            // Visual feedback
            if (itemHoldPoint != null)
            {
                // Clear any existing item visual
                foreach (Transform child in itemHoldPoint)
                {
                    Destroy(child.gameObject);
                }

                // Instantiate new item visual if available
                if (item.itemVisualPrefab != null)
                {
                    GameObject visual = Instantiate(item.itemVisualPrefab, itemHoldPoint);
                    visual.transform.localPosition = Vector3.zero;
                    visual.transform.localRotation = Quaternion.identity;

                    // Scale down if too big
                    visual.transform.localScale = Vector3.one * 0.5f;
                }
            }

            Destroy(item.gameObject);
            OnItemChanged?.Invoke(playerNumber, heldItem);

            // Reputation effects for rare items
            if (item.isRareItem)
            {
                // Teacher loses reputation
                TwoPlayerGameManager.Instance.ModifyReputation(
                    playerNumber,
                    NPCMovement.NPCGroup.Teacher,
                    -0.15f
                );

                // Corresponding group gains reputation
                NPCMovement.NPCGroup itemGroup = GetItemGroup(item.itemType);
                if (itemGroup != NPCMovement.NPCGroup.Teacher)
                {
                    TwoPlayerGameManager.Instance.ModifyReputation(
                        playerNumber,
                        itemGroup,
                        0.1f
                    );
                }

                Debug.Log($"Player {playerNumber} picked up rare item: {item.itemType}");
            }
        }
        else
        {
            Debug.Log($"Player {playerNumber} already holding an item!");
        }
    }

    NPCMovement.NPCGroup GetItemGroup(ItemType item)
    {
        return item switch
        {
            ItemType.Protractor => NPCMovement.NPCGroup.Nerd,
            ItemType.Basketball => NPCMovement.NPCGroup.Athlete,
            ItemType.Paintbrush => NPCMovement.NPCGroup.Artist,
            ItemType.Apple => NPCMovement.NPCGroup.Teacher,
            ItemType.PrankKit => NPCMovement.NPCGroup.Grade8,
            _ => NPCMovement.NPCGroup.Nerd
        };
    }

    // Public getters
    public int GetPlayerNumber() => playerNumber;
    public int GetHeldVotes() => heldVotes;
    public ItemType GetHeldItem() => heldItem;
    public bool IsDashing() => isDashing;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = playerNumber == 1 ? Color.blue : Color.red;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dashRange);

        if (heldVotes > 0)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2, 0.2f + (heldVotes * 0.01f)); 
        }
    }
    public void SetPlayerNumber(int number)
    {
        playerNumber = number;
    }

    public void SetPlayerColor(Color color)
    {
        playerColor = color;
        if (playerRenderer != null)
        {
            playerRenderer.material.color = color;
        }
    }

    public void SetPlayerName(string name)
    {
        playerName = name;
        gameObject.name = name;
    }
}