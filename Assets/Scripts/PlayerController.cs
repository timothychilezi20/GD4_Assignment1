using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float dashCooldown = 1.5f;
    [SerializeField] private float dashRange = 2f;

    [Header("Vote System")]
    [SerializeField] private int heldVotes = 0;
    [SerializeField] private int votesLostOnHit = 5;
    [SerializeField] private float votePickupRange = 2f;
    [SerializeField] private GameObject votePickupPrefab;

    [Header("Item System")]
    [SerializeField] private ItemType heldItem = ItemType.None;
    [SerializeField] private Transform itemHoldPoint;
    [SerializeField] private GameObject itemVisualPrefab;

    [Header("Reputation System")]
    [SerializeField] private float teacherReputation = 1.0f;
    [SerializeField] private float athleteReputation = 1.0f;
    [SerializeField] private float artistReputation = 1.0f;
    [SerializeField] private float nerdReputation = 1.0f;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayer;

    private Rigidbody rb;
    private Collider playerCollider;
    private Renderer playerRenderer;
    private Coroutine dashCoroutine;

    private Vector2 movementInput;
    private bool isDashing = false;
    private bool canDash = true;
    private bool isInventoryOpen = false;
    private GameObject nearbyInteractable;

    public System.Action<int> OnVotesChanged;
    public System.Action<ItemType> OnItemChanged;
    public System.Action<float[]> OnReputationChanged;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
        playerRenderer = GetComponent<Renderer>();

        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
    }

    void Start()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.mass = 1f;
        rb.linearDamping = 5f;
    }

    void Update()
    {
        if (!isDashing)
        {
            HandleMovement();
        }

        HandleInteraction();
        HandleInventory();
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

        if (playerRenderer != null)
            playerRenderer.material.color = Color.yellow;

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

        if (playerRenderer != null)
            playerRenderer.material.color = Color.white;

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
                PlayerController otherPlayer = hit.GetComponent<PlayerController>();
                if (otherPlayer != null)
                {
                    otherPlayer.TakeDashHit();
                }

                NPCMovement npc = hit.GetComponent<NPCMovement>();
                if (npc != null)
                {
                    npc.DropVotes();

                    if (npc.GetGroup() == NPCMovement.NPCGroup.Teacher)
                    {
                        ModifyReputation(NPCMovement.NPCGroup.Teacher, -0.1f);
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
            DroppedVotes votes = nearbyInteractable.GetComponent<DroppedVotes>();
            if (votes != null)
            {
                AddVotes(votes.PickUp());
                return;
            }

            WorldItem worldItem = nearbyInteractable.GetComponent<WorldItem>();
            if (worldItem != null)
            {
                PickUpItem(worldItem);
                return;
            }

            DumpZone dumpZone = nearbyInteractable.GetComponent<DumpZone>();
            if (dumpZone != null)
            {
                DumpVotes(dumpZone);
                return;
            }

            ItemTrader trader = nearbyInteractable.GetComponent<ItemTrader>();
            if (trader != null)
            {
                TradeItem(trader);
                return;
            }
        }
    }

    public void AddVotes(int amount)
    {
        heldVotes += amount;
        OnVotesChanged?.Invoke(heldVotes);
        Debug.Log($"Now holding {heldVotes} votes");
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
                droppedComponent.Initialize(amount);
            }
        }

        OnVotesChanged?.Invoke(heldVotes);
    }

    void DumpVotes(DumpZone dumpZone)
    {
        if (heldVotes > 0)
        {
            int dumpedVotes = Mathf.RoundToInt(heldVotes * dumpZone.multiplier);
            // GameManager.Instance.AddVotes(dumpedVotes);

            heldVotes = 0;
            OnVotesChanged?.Invoke(heldVotes);

            Debug.Log($"Dumped votes with {dumpZone.multiplier}x multiplier!");
        }
    }

    void PickUpItem(WorldItem item)
    {
        if (heldItem == ItemType.None)
        {
            heldItem = item.itemType;

            if (itemVisualPrefab != null && itemHoldPoint != null)
            {
                GameObject visual = Instantiate(itemVisualPrefab, itemHoldPoint);
            }

            Destroy(item.gameObject);
            OnItemChanged?.Invoke(heldItem);

            if (item.isRareItem)
            {
                ModifyReputation(NPCMovement.NPCGroup.Teacher, -0.15f);

                NPCMovement.NPCGroup itemGroup = GetItemGroup(item.itemType);
                if (itemGroup != NPCMovement.NPCGroup.Teacher)
                {
                    ModifyReputation(itemGroup, 0.1f);
                }
            }
        }
    }

    void TradeItem(ItemTrader trader)
    {
        if (heldItem != ItemType.None && trader != null)
        {
            int votesReceived = trader.GetTradeValue(heldItem);
            AddVotes(votesReceived);

            heldItem = ItemType.None;
            if (itemHoldPoint != null)
            {
                Destroy(itemHoldPoint.GetChild(0)?.gameObject);
            }

            OnItemChanged?.Invoke(heldItem);
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

    public void ModifyReputation(NPCMovement.NPCGroup group, float delta)
    {
        switch (group)
        {
            case NPCMovement.NPCGroup.Nerd:
                nerdReputation = Mathf.Clamp(nerdReputation + delta, 0.5f, 1.5f);
                break;
            case NPCMovement.NPCGroup.Athlete:
                athleteReputation = Mathf.Clamp(athleteReputation + delta, 0.5f, 1.5f);
                break;
            case NPCMovement.NPCGroup.Artist:
                artistReputation = Mathf.Clamp(artistReputation + delta, 0.5f, 1.5f);
                break;
            case NPCMovement.NPCGroup.Teacher:
                teacherReputation = Mathf.Clamp(teacherReputation + delta, 0.3f, 1.2f);
                break;
        }

        OnReputationChanged?.Invoke(new float[] { nerdReputation, athleteReputation, artistReputation, teacherReputation });
    }

    public float GetReputationForGroup(NPCMovement.NPCGroup group)
    {
        return group switch
        {
            NPCMovement.NPCGroup.Nerd => nerdReputation,
            NPCMovement.NPCGroup.Athlete => athleteReputation,
            NPCMovement.NPCGroup.Artist => artistReputation,
            NPCMovement.NPCGroup.Teacher => teacherReputation,
            _ => 1f
        };
    }

    void HandleInventory()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            isInventoryOpen = !isInventoryOpen;
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();
    }

    public void OnInventory(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            isInventoryOpen = !isInventoryOpen;
        }
    }

    public bool IsDashing() => isDashing;
    public int GetHeldVotes() => heldVotes;
    public ItemType GetHeldItem() => heldItem;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dashRange);
    }
}