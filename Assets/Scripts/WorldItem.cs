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
    private bool isHeld = false;
    private Transform holdParent; 

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

        if (playerInRange && interactionPrompt != null && promptCanvasGroup.alpha < 1f)
        {
            promptCanvasGroup.alpha += Time.deltaTime * 5f;
        }
        else if (!playerInRange && interactionPrompt != null && promptCanvasGroup.alpha > 0f)
        {
            promptCanvasGroup.alpha -= Time.deltaTime * 5f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"ITEM OnTriggerEnter: {other.gameObject.name} with tag {other.tag}");

        if (isCollected) return;

        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            playerInRange = true;
            currentPlayer = other.GetComponent<PlayerController>();

            Debug.Log($"Player detected! PlayerInRange = true, CurrentPlayer = {currentPlayer}");

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
                if (promptText != null)
                {
                    string rarityText = isRareItem ? "<color=yellow>RARE</color> " : "";
                    promptText.text = $"Press E to pick up {rarityText}{itemName}";
                    Debug.Log($"Showing prompt: {promptText.text}");
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"ITEM OnTriggerExit: {other.gameObject.name}");

        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            playerInRange = false;
            currentPlayer = null;
            Debug.Log($"Player left. PlayerInRange = false");

            if (interactionPrompt != null)
            {
                promptCanvasGroup.alpha = 0f;
                interactionPrompt.SetActive(false);
            }
        }
    }

    public void TryPickUp(PlayerController player)
    {
        Debug.Log($"TryPickUp called. isCollected={isCollected}, player={player}");

        if (isCollected || player == null) return;

        if (player.GetHeldItem() == ItemType.None)
        {
            Debug.Log($"Picking up item!");
            player.PickUpItem(this);
            isCollected = true;
            isHeld = true; 

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
        }
        else
        {
            Debug.Log($"Player already holding item: {player.GetHeldItem()}");

            if (UIManager.Instance != null)
                UIManager.Instance.ShowAnnouncement("Already holding an item!", Color.red);
        }
    }

    void OnDestroy()
    {
        if (interactionPrompt != null && interactionPrompt.activeSelf)
        {
            interactionPrompt.SetActive(false);
        }
    }

    public void DropItem(Vector3 dropPosition)
    {

        Debug.Log("Item dropped");

        isCollected = false;
        isHeld = false;

        transform.SetParent(null);
        transform.position = dropPosition; 

        if (itemCollider != null)
        {
            itemCollider.enabled = true; 
        }

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = true; 
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        startPosition = transform.position;
    }
}