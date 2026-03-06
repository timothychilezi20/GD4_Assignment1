using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private int heldBallots = 0;

    [Header("Item System")]
    private Item heldItem;
    //[SerializeField] private Transform itemHoldPoint;
    [SerializeField] private float itemPickupRange = 2f;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayer =1 ;
    [SerializeField] private Transform itemHoldPoint;
    [SerializeField] private LayerMask pickupLayer = 1;

    
   

    // Components
    private Rigidbody rb;
    private Renderer playerRenderer;
    private Coroutine dashCoroutine;
    private PlayerInput playerInput;
    private CapsuleCollider playerCollider;
    private PlayerInventory inventory;

   
    
    private GroupType heldBallotType;

    // State
    private Vector2 movementInput;
    private bool isDashing = false;
    private bool canDash = true;
    private GameObject nearbyInteractable;

    // Events
    public System.Action<int, int> OnVotesChanged; // playerNumber, votes
    public System.Action<int, ItemType> OnItemChanged;

    private PlayerPoints playerPoints;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();
        playerInput = GetComponent<PlayerInput>();
        playerCollider = GetComponent<CapsuleCollider>();
        inventory = GetComponent<PlayerInventory>();

        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        if (itemHoldPoint == null) itemHoldPoint = transform;
    }

    void Start()
    {
        // Configure Rigidbody
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.mass = 1f;
        rb.linearDamping = 5f;
        rb.angularDamping = 0.5f;

        // Set player color
        if (playerRenderer != null)
        {
            playerRenderer.material.color = playerColor;
        }

        // Set tag based on player number
        gameObject.tag = playerNumber == 1 ? "Player1" : "Player2";
        gameObject.name = playerName;

        if(interactableLayer == 0)
        {
            Debug.LogError($"Player {playerNumber}: interactableLayer is 0! Set it in inspector to include Item/GroupReceiver/DumpingStation layers", this);

            interactableLayer = -1;
        }

        Debug.Log($"Player {playerNumber} started with color: {playerColor}");
    }

    void Update()
    {
        if (!isDashing)
        {
            HandleMovement();
        }

        UpdateInteractionUI();

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

    // ========== INPUT METHODS ==========
    // These are called automatically by the Player Input component with "Send Messages" behavior

    public void Move(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();
        // Debug log to verify input is working
        if (movementInput != Vector2.zero)
            Debug.Log($"Player {playerNumber} moving: {movementInput}");
    }

    public void Dash(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Debug.Log($"Player {playerNumber} dash performed");
            if (canDash && !isDashing)
            {
                if (dashCoroutine != null)
                    StopCoroutine(dashCoroutine);
                dashCoroutine = StartCoroutine(Dash());
            }
        }
    }

    //INTERACTION SYSTEM

    void HandleInteractionDetection()
    {
        nearbyInteractable = null;
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer);

        Debug.Log($"Player {playerNumber} scanning {hits.Length} objects in range");

        foreach(Collider hit in hits)
        {
            if(hit.gameObject == gameObject) continue;

            if (hit.GetComponent<DumpingStation>())
            {
                nearbyInteractable = hit.gameObject;
                ShowInteractUI("Dump Ballots");
                return;
            }

            if (hit.GetComponent<GroupReceiver>())
            {
                nearbyInteractable = hit.gameObject;
                if (inventory.HasItem())
                {
                    ShowInteractUI("Give Item");
                }

                else
                {
                    ShowInteractUI("No Item");
                    return;
                }

                if (hit.GetComponent<Item>())
                {
                    nearbyInteractable = hit.gameObject;
                    if (!inventory.HasItem())

                        ShowInteractUI("Pick Up");
                    

                    else

                        ShowInteractUI("Inventory Full");
                    return ;

                }
                Debug.Log("item thing");
            }

            
        }

        HideInteractUI();
    }

    private void OnTriggerEnter(Collider other)
    {


      
    }
    void TryInteract()
    {
        if(nearbyInteractable == null)
        {
            Debug.Log($"Player {playerNumber}: Nothing to interact with");
            return;
        }

        DumpingStation station = nearbyInteractable.GetComponent<DumpingStation>();
        if(station != null)
        {
            inventory.DumpBallots(station);
            return;
        }

        GroupReceiver group = nearbyInteractable.GetComponent<GroupReceiver>();
        if(group != null)
        {
            inventory.GiveItemToGroup(group);
            return;
        }

        Item item = nearbyInteractable.GetComponent<Item>();

        if(item != null)
        {
            inventory.PickUp(item);
            return; 
        }   
    }


    void ShowInteractUI(string action)
    {
        Debug.Log($"[INTERACT]: {action}");
    }

    void HideInteractUI()
    {

    }

    void UpdateInteractionUI()
    {

    }
   

    // ========== DASH MECHANIC ==========

    IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;

        Vector3 dashDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;
        if (dashDirection.magnitude < 0.1f)
        {
            dashDirection = transform.forward;
        }

        // Visual feedback - flash white
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
                    //otherPlayer.TakeDashHit();
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
                        //If you have a GameManager, uncomment this
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

   

    // ========== INTERACTION SYSTEM ==========

    void HandleInteraction()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer);

        nearbyInteractable = null;

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue;

            nearbyInteractable = hit.gameObject;
            break;
        }
    }

    void PickUpItem(Item item)
    {
        if(heldItem != null)
        {
            Debug.Log("Already holding Item");
            return;
        }

        heldItem = item;
        item.OnPickedUp(itemHoldPoint);

        Debug.Log($"Player picked up item for {item.groupType}");
    }


    
    // ========== VOTE SYSTEM ==========







    // ========== ITEM SYSTEM ==========



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

    // ========== SETTER METHODS (Called by Spawner) ==========

    public void SetPlayerNumber(int number)
    {
        playerNumber = number;
        Debug.Log($"Player number set to: {playerNumber}");
    }

    public void SetPlayerColor(Color color)
    {
        playerColor = color;
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
        Debug.Log($"Player color set to: {color}");
    }

    public void SetPlayerName(string name)
    {
        playerName = name;
        gameObject.name = name;
        Debug.Log($"Player name set to: {name}");
    }

    // ========== PUBLIC GETTERS ==========

    public int GetPlayerNumber() => playerNumber;
   
   
    public bool IsDashing() => isDashing;

    // ========== DEBUG VISUALIZATION ==========

    void OnDrawGizmosSelected()
    {
        // Interaction range
        Gizmos.color = playerNumber == 1 ? Color.blue : Color.red;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Dash range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dashRange);

        // Held votes indicator
        
    }

    // Clean up events if needed
    void OnDestroy()
    {
        // Unsubscribe from any events here if needed
    }
}