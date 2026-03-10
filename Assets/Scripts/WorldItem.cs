using UnityEngine;
using TMPro;

public class WorldItem : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemType itemType;
    [SerializeField] private bool isRareItem = false;
    [SerializeField] private string itemName;
    [SerializeField] private int itemValue = 5;

    [Header("Hover & Rotation")]
    [SerializeField] private float hoverHeight = 0.3f;
    [SerializeField] private float hoverSpeed = 2f;
    [SerializeField] private float rotationSpeed = 50f;

    [Header("UI Prompt")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private CanvasGroup promptCanvasGroup;

    private Collider itemCollider;
    private bool isCollected = false;
    private PlayerController currentPlayer;
    private Vector3 startPosition;
    private float hoverOffset;
    private AudioSource audioSource;

    public ItemSpawnPoint spawnPoint;
    public ItemSpawner spawner;
    private RareItemRule rareItemRule;

    public ItemType ItemType => itemType;
    public bool IsRareItem => isRareItem;
    public string ItemName => itemName;
    public int ItemValue => itemValue;

    void Awake()
    {
        itemCollider = GetComponent<Collider>();
        if (itemCollider != null) itemCollider.isTrigger = true;

        rareItemRule = GetComponent<RareItemRule>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        startPosition = transform.position;
        hoverOffset = Random.Range(0f, Mathf.PI * 2f);

        if (interactionPrompt != null)
        {
            if (promptCanvasGroup != null)
                promptCanvasGroup.alpha = 0f;

            interactionPrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (isCollected) return;

        float yOffset = Mathf.Sin((Time.time * hoverSpeed) + hoverOffset) * hoverHeight;
        transform.position = startPosition + Vector3.up * yOffset;
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        if (currentPlayer != null && interactionPrompt != null && promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = Mathf.Min(promptCanvasGroup.alpha + Time.deltaTime * 5f, 1f);
        }
        else if (interactionPrompt != null && promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = Mathf.Max(promptCanvasGroup.alpha - Time.deltaTime * 5f, 0f);
            if (promptCanvasGroup.alpha <= 0f)
                interactionPrompt.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            currentPlayer = player;
            ShowPromptForPlayer(player);
        }
    }

    void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && player == currentPlayer)
        {
            currentPlayer = null;
        }
    }

    private void ShowPromptForPlayer(PlayerController player)
    {
        if (interactionPrompt != null && promptText != null)
        {
            interactionPrompt.SetActive(true);
            string rarityText = isRareItem ? "<color=yellow>RARE</color> " : "";
            promptText.text = $"Press E to pick up {rarityText}{itemName}";
        }
    }

    public void TryPickUp(PlayerController player)
    {
        if (isCollected || player == null) return;

        if (player.GetHeldItem() != ItemType.None)
        {
            UIManager.Instance?.ShowAnnouncement("Already holding an item!", Color.red);
            return;
        }

        isCollected = true;
        player.PickUpItem(this);
        currentPlayer = null;

        if (itemCollider != null)
            itemCollider.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (audioSource != null && audioSource.clip != null)
            audioSource.Play();

        if (spawnPoint != null)
            spawnPoint.ClearItem();

        if (spawner != null)
            spawner.Respawn(spawnPoint);

        if (ItemPickupAnnouncementManager.Instance != null)
        {
            ItemPickupAnnouncementManager.Instance.ShowAnnouncement(
                player.GetPlayerNumber(),
                itemName,
                itemValue,
                isRareItem
            );
        }
    }

    public void DropItem(Vector3 dropPosition)
    {
        isCollected = false;

        transform.SetParent(null);
        transform.position = dropPosition;

        if (itemCollider != null)
            itemCollider.enabled = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        startPosition = transform.position;
    }

    public bool HasRareRule()
    {
        return rareItemRule != null && rareItemRule.IsRare;
    }

    public NPCMovement.NPCGroup GetRareTargetGroup()
    {
        return rareItemRule != null ? rareItemRule.TargetGroup : NPCMovement.NPCGroup.Athlete;
    }

    public float GetRareReputationChange()
    {
        return rareItemRule != null ? rareItemRule.ReputationChange : 0f;
    }
}