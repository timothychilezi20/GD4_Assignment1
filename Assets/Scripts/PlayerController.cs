using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Player Identification")]
    [SerializeField] private int playerNumber = 1;
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
    [SerializeField] private int ballotCount = 0;

    [Header("Item System")]
    [SerializeField] private ItemType heldItem = ItemType.None;
    [SerializeField] public Transform itemHoldPoint;
    private WorldItem heldItemObject;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayer;

    private Rigidbody rb;
    private Renderer playerRenderer;
    private Coroutine dashCoroutine;
    private PlayerInput playerInput;
    private CapsuleCollider playerCollider;
    private PlayerPoints playerPoints;

    private Vector2 movementInput;
    private bool isDashing = false;
    private bool canDash = true;
    private GameObject nearbyInteractable;

    public System.Action<int, int> OnVotesChanged;
    public System.Action<int, ItemType> OnItemChanged;
    private string currentInteractText = "";

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();
        playerInput = GetComponent<PlayerInput>();
        playerCollider = GetComponent<CapsuleCollider>();
        playerPoints = GetComponent<PlayerPoints>();

        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
    }

    void Start()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.mass = 1f;
        rb.linearDamping = 5f;
        rb.angularDamping = 0.5f;

        if (playerRenderer != null)
            playerRenderer.material.color = playerColor;

        gameObject.tag = playerNumber == 1 ? "Player1" : "Player2";
        gameObject.name = playerName;
    }

    void Update()
    {
        if (!isDashing)
            HandleMovement();

        HandleInteraction();

        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            AddVotes(10);
        }
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

    public void Move(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();
    }

    public void Dash(InputAction.CallbackContext ctx)
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
            dashDirection = transform.forward;

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
                PlayerController otherPlayer = hit.GetComponent<PlayerController>();
                if (otherPlayer != null && otherPlayer.GetPlayerNumber() != playerNumber)
                {
                    otherPlayer.TakeDashHit();

                    if (UIManager.Instance != null)
                        UIManager.Instance.ShowTradeResult("Trade successful!", playerNumber);
                }
            }
        }
    }

    public void TakeDashHit()
    {
        int votesToDrop = Mathf.Min(votesLostOnHit, heldVotes);
        if (votesToDrop > 0)
            DropVotes(votesToDrop);

        DropHeldItem();

        // Update UI
        UIManager.Instance.UpdatePlayerVotes(playerNumber, heldVotes);
    }

    void HandleInteraction()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer);

        nearbyInteractable = null;
        float closestDistance = Mathf.Infinity;
        currentInteractText = "";

        foreach (Collider hit in hits)
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);

            if (distance < closestDistance)
            {
                // Check for interactable types
                if (hit.GetComponent<Ballot>() != null)
                {
                    closestDistance = distance;
                    nearbyInteractable = hit.gameObject;
                    currentInteractText = "Press E to pick up Ballot";
                }
                else if (hit.GetComponent<DroppedVotes>() != null)
                {
                    closestDistance = distance;
                    nearbyInteractable = hit.gameObject;
                    currentInteractText = "Press E to pick up Votes";
                }
                else if (hit.GetComponent<WorldItem>() != null)
                {
                    closestDistance = distance;
                    nearbyInteractable = hit.gameObject;
                    WorldItem item = hit.GetComponent<WorldItem>();
                    currentInteractText = $"Press E to pick up {item.ItemName}";
                }
                else if (hit.GetComponent<StudentController>() != null)
                {
                    closestDistance = distance;
                    nearbyInteractable = hit.gameObject;

                    StudentController student = hit.GetComponent<StudentController>();
                    currentInteractText = $"Press E to trade with {student.groupType} student";
                }
            }
        }

        // Show or hide UI prompt via UIManager
        if (!string.IsNullOrEmpty(currentInteractText))
        {
            UIManager.Instance?.ShowInteractPrompt(currentInteractText);
        }
        else
        {
            UIManager.Instance?.HideInteractPrompt();
        }
    }

    public void Interact(InputAction.CallbackContext ctx)
    {
        Debug.Log($"Interact method called, performed={ctx.performed}");
        if (ctx.performed)
            TryInteract();
    }

    void TryInteract()
    {
        Debug.Log($"TryInteract called. nearbyInteractable = {nearbyInteractable}");

        if (nearbyInteractable != null)
        {
            Debug.Log($"Checking interactable: {nearbyInteractable.name}");

            Ballot ballot = nearbyInteractable.GetComponent<Ballot>();
            if (ballot != null)
            {
                Debug.Log("Found Ballot");

                int amount = ballot.PickUp();

                AddVotes(amount);

                // Increase ballot count
                ballotCount++;

                // Update HUD
                UIManager.Instance?.UpdatePlayerBallots(playerNumber, ballotCount);

                UIManager.Instance?.ShowPlayerMessage(playerNumber, "+10 Votes!", Color.green);

                return;
            }

            // --- Dropped Votes ---
            DroppedVotes votes = nearbyInteractable.GetComponent<DroppedVotes>();
            if (votes != null)
            {
                Debug.Log("Found DroppedVotes");
                int amount = votes.PickUp();
                AddVotes(amount);

                // UI
                UIManager.Instance?.ShowPlayerMessage(playerNumber, "+10 Votes!", Color.green);
                return;
            }

            // --- World Items ---
            WorldItem worldItem = nearbyInteractable.GetComponent<WorldItem>();
            if (worldItem != null)
            {
                Debug.Log("Found WorldItem, calling TryPickUp");
                worldItem.TryPickUp(this);

                // Optional: show prompt for item pickup
                UIManager.Instance?.ShowPlayerMessage(playerNumber, "+10 Votes!", Color.green);
                return;
            }

            // --- NPC Trade ---
            NPCMovement npc = nearbyInteractable.GetComponent<NPCMovement>();
            if (npc != null)
            {
                Debug.Log("Found NPCMovement, calling TryTrade");
                npc.TryTrade(this);

                // Optional: show prompt for trading
                UIManager.Instance?.ShowPlayerMessage(playerNumber, "+10 Votes!", Color.green);
                return;
            }

            StudentController student = nearbyInteractable.GetComponent<StudentController>();

            if (student != null)
            {
                student.TryTrade(this);
                return;
            }

            Debug.Log($"No interactable component found on {nearbyInteractable.name}");
        }
        else
        {
            Debug.Log("No nearbyInteractable found");
            UIManager.Instance?.HideInteractPrompt();
        }

        UIManager.Instance?.HideInteractPrompt();
    }

    public void AddVotes(int amount)
    {
        heldVotes += amount;
        UIManager.Instance.UpdatePlayerVotes(playerNumber, heldVotes);

        if (playerPoints != null)
            playerPoints.AddHeldVotes(amount);

        if (UIManager.Instance != null)
            UIManager.Instance?.ShowPlayerMessage(playerNumber, "+10 Votes!", Color.green);
    }

    void DropVotes(int amount)
    {
        heldVotes -= amount;

        //if (votePickupPrefab != null)
        //{
        //    // Use the player's forward direction for the dropped votes
        //    GameObject dropped = Instantiate(votePickupPrefab, transform.position + Vector3.up, Quaternion.identity);
        //    DroppedVotes droppedComponent = dropped.GetComponent<DroppedVotes>();
        //    if (droppedComponent != null)
        //        droppedComponent.Initialize(amount, transform.forward);
        //}

        OnVotesChanged?.Invoke(playerNumber, heldVotes);

        if (playerPoints != null)
            playerPoints.RemoveHeldVotes(amount);

        for (int i = 0; i < amount; i++)
        {
            Instantiate(votePickupPrefab, transform.position + Random.insideUnitSphere, Quaternion.identity);
        }
    }

    public void ClearHeldVotes()
    {
        heldVotes = 0;
        OnVotesChanged?.Invoke(playerNumber, heldVotes);

        if (playerPoints != null)
            playerPoints.RemoveHeldVotes(heldVotes);
    }

    public int GetHeldVotes()
    {
        return heldVotes;
    }

    public void PickUpItem(WorldItem item)
    {
        if (heldItem == ItemType.None)
        {
            heldItem = item.ItemType;
            heldItemObject = item;

            if (itemHoldPoint != null)
            {
                item.transform.SetParent(itemHoldPoint);
                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.identity;
            }

            OnItemChanged?.Invoke(playerNumber, heldItem);
        }
    }

    public void ClearHeldItem()
    {
        heldItem = ItemType.None;

        if (itemHoldPoint != null)
        {
            foreach (Transform child in itemHoldPoint)
                Destroy(child.gameObject);
        }

        UIManager.Instance?.UpdatePlayerItem(playerNumber, heldItem);
        OnItemChanged?.Invoke(playerNumber, heldItem);
    }

    public ItemType GetHeldItem()
    {
        return heldItem;
    }

    public float GetVoteMultiplierForNPC(NPCMovement.NPCGroup npcGroup)
    {
        if (ReputationManager.Instance != null)
        {
            return ReputationManager.Instance.GetVoteMultiplier(playerNumber, npcGroup);
        }
        return 1f;
    }

    public void SetPlayerNumber(int number)
    {
        playerNumber = number;
    }

    public void SetPlayerColor(Color color)
    {
        playerColor = color;
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
            renderer.material.color = color;
    }

    public void SetPlayerName(string name)
    {
        playerName = name;
        gameObject.name = name;
    }

    public int GetPlayerNumber() => playerNumber;
    public bool IsDashing() => isDashing;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = playerNumber == 1 ? Color.blue : Color.red;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dashRange);
    }

    public void DropHeldItem()
    {
        if (heldItemObject == null)
            return;

        Debug.Log($"{playerName} dropped {heldItem}");

        Vector3 dropPosition = transform.position + transform.forward + Vector3.up * 0.5f;

        heldItemObject.DropItem(dropPosition);

        Rigidbody rb = heldItemObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce((transform.forward + Vector3.up) * 3f, ForceMode.Impulse);
        }

        heldItem = ItemType.None;
        heldItemObject = null;

        if (itemHoldPoint != null)
        {
            foreach (Transform child in itemHoldPoint)
                Destroy(child.gameObject);
        }

        OnItemChanged?.Invoke(playerNumber, heldItem);
    }

    public int GetBallotCount()
    {
        return ballotCount;
    }

    public void ClearBallots()
    {
        ballotCount = 0;
        UIManager.Instance?.UpdatePlayerBallots(playerNumber, ballotCount);
    }
}