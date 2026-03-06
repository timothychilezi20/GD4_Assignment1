using UnityEngine;
using TMPro;

public class WorldItem : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemType itemType;
    [SerializeField] private bool isRareItem = false;
    [SerializeField] private string itemName;
    [SerializeField] private int itemValue = 5;

    [Header("Interaction")]
    [SerializeField] private float hoverHeight = 0.3f;
    [SerializeField] private float hoverSpeed = 2f;
    [SerializeField] private float rotationSpeed = 50f;

    [Header("UI")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private CanvasGroup promptCanvasGroup;

    private Collider itemCollider;
    private bool isCollected = false;
    private bool playerInRange = false;
    private PlayerController currentPlayer;

    private Vector3 startPosition;
    private float hoverOffset;
    private AudioSource audioSource;

    public ItemType ItemType => itemType;
    public bool IsRareItem => isRareItem;
    public string ItemName => itemName;
    public int ItemValue => itemValue;

    void Awake()
    {
        itemCollider = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (itemCollider != null)
            itemCollider.isTrigger = true;

        startPosition = transform.position;
        hoverOffset = Random.Range(0f, Mathf.PI * 2f);

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
            if (promptCanvasGroup != null)
                promptCanvasGroup.alpha = 0f;
        }
    }

    void Update()
    {
        if (isCollected) return;

        // Hover animation
        float yOffset = Mathf.Sin((Time.time * hoverSpeed) + hoverOffset) * hoverHeight;
        transform.position = startPosition + Vector3.up * yOffset;

        // Rotation
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Fade prompt
        if (interactionPrompt != null && promptCanvasGroup != null)
        {
            float targetAlpha = playerInRange ? 1f : 0f;
            promptCanvasGroup.alpha = Mathf.Lerp(promptCanvasGroup.alpha, targetAlpha, Time.deltaTime * 5f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            playerInRange = true;
            currentPlayer = other.GetComponent<PlayerController>();

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
                if (promptText != null)
                {
                    string rarityText = isRareItem ? "<color=yellow>RARE</color> " : "";
                    promptText.text = $"Press Interact to pick up {rarityText}{itemName}";
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            playerInRange = false;
            currentPlayer = null;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
                if (promptCanvasGroup != null)
                    promptCanvasGroup.alpha = 0f;
            }
        }
    }

    public void TryPickUp(PlayerController player)
    {
        if (isCollected || player == null) return;

        if (player.GetHeldItem() == ItemType.None)
        {
            player.PickUpItem(this);
            isCollected = true;

            // Disable visuals
            if (itemCollider != null) itemCollider.enabled = false;
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = false;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);

            // Play pickup sound
            if (audioSource != null && audioSource.clip != null)
                audioSource.Play();

            // Spawn floating notification above the item
            if (NotificationManager.Instance != null)
            {
                string rarity = isRareItem ? " (RARE!)" : "";
                NotificationManager.Instance.SpawnNotification($"Picked up {itemName}{rarity}", Color.magenta, transform.position + Vector3.up * 2f);
            }

            Destroy(gameObject, 0.5f);
        }
        else
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowAnnouncement("Already holding an item!", Color.red);
        }
    }

    public bool IsPlayerInRange(PlayerController player)
    {
        return playerInRange && currentPlayer == player;
    }
}