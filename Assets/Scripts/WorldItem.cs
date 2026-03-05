using UnityEngine;
using TMPro;
using System.Collections;

public class WorldItem : MonoBehaviour, IInteractable
{
    [Header("Item Data")]
    [SerializeField] public ItemType itemType;
    [SerializeField] public bool isRareItem = false;
    [SerializeField] private int voteValue = 5;
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private string itemName;
    [TextArea(2, 3)]
    [SerializeField] private string itemDescription;

    [Header("Visual Components")]
    [SerializeField] private MeshRenderer itemRenderer;
    [SerializeField] private Light itemLight;
    [SerializeField] private ParticleSystem floatParticles;
    [SerializeField] private ParticleSystem glowParticles;
    [SerializeField] private GameObject itemVisualPrefab; // For when held

    [Header("Animation Settings")]
    [SerializeField] private float hoverHeight = 0.5f;
    [SerializeField] private float hoverSpeed = 2f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.1f;

    [Header("Highlight")]
    [SerializeField] private GameObject highlightRing;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private float highlightPulseSpeed = 2f;

    [Header("Interaction Prompt")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI promptActionText;
    [SerializeField] private TextMeshProUGUI promptNameText;
    [SerializeField] private TextMeshProUGUI promptValueText;
    [SerializeField] private CanvasGroup promptCanvasGroup;
    [SerializeField] private float promptFadeSpeed = 5f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem pickupEffect;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip rarePickupSound;
    [SerializeField] private AudioClip cannotPickupSound;

    [Header("Respawn Settings")]
    [SerializeField] private bool canRespawn = false;
    [SerializeField] private float respawnTime = 30f;

    // Private variables
    private AudioSource audioSource;
    private Vector3 startPosition;
    private float hoverOffset;
    private Material originalMaterial;
    private Color originalLightColor;
    private bool isCollected = false;
    private bool isHighlighted = false;
    private Coroutine respawnCoroutine;
    private Collider itemCollider;

    // Properties
    public ItemType ItemType => itemType;
    public bool IsRareItem => isRareItem;
    public int VoteValue => voteValue;
    public string ItemName => itemName;
    public Sprite ItemIcon => itemIcon;

    void Awake()
    {
        // Get components
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        itemCollider = GetComponent<Collider>();

        // Store original material
        if (itemRenderer != null)
            originalMaterial = itemRenderer.material;

        if (itemLight != null)
            originalLightColor = itemLight.color;
    }

    void Start()
    {
        startPosition = transform.position;
        hoverOffset = Random.Range(0f, Mathf.PI * 2f);

        // Initialize
        if (highlightRing != null)
            highlightRing.SetActive(false);

        if (interactionPrompt != null)
        {
            promptCanvasGroup.alpha = 0f;
            UpdatePromptText();
        }

        // Set rare item visuals
        if (isRareItem)
        {
            ApplyRareItemVisuals();
        }

        // Start floating
        StartCoroutine(FloatAnimation());
    }

    void Update()
    {
        if (isCollected) return;

        // Rotation
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Pulse highlight
        if (isHighlighted && highlightRing != null)
        {
            float pulse = Mathf.Sin(Time.time * highlightPulseSpeed) * 0.2f + 0.8f;
            highlightRing.transform.localScale = Vector3.one * pulse;
        }
    }

    IEnumerator FloatAnimation()
    {
        while (!isCollected)
        {
            // Smooth hover using sine wave
            float yOffset = Mathf.Sin((Time.time * hoverSpeed) + hoverOffset) * hoverHeight;
            transform.position = startPosition + Vector3.up * yOffset;

            // Pulse scale slightly
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = Vector3.one * scale;

            yield return null;
        }
    }

    void ApplyRareItemVisuals()
    {
        // Make rare items glow more
        if (itemRenderer != null)
        {
            itemRenderer.material.EnableKeyword("_EMISSION");
            itemRenderer.material.SetColor("_EmissionColor", GetItemColor() * 2f);
        }

        if (itemLight != null)
        {
            itemLight.intensity = 3f;
            itemLight.range = 5f;
        }

        if (glowParticles != null)
            glowParticles.Play();

        voteValue *= 3; // Rare items worth more
    }

    void UpdatePromptText()
    {
        if (promptActionText != null)
            promptActionText.text = "Press E";

        if (promptNameText != null)
        {
            string rarityText = isRareItem ? "<color=yellow>RARE</color> " : "";
            promptNameText.text = $"{rarityText}{itemName}";
        }

        if (promptValueText != null)
        {
            string valueText = isRareItem ? "★ Special Item ★" : $"+{voteValue} votes";
            promptValueText.text = valueText;
            promptValueText.color = isRareItem ? Color.yellow : Color.green;
        }
    }

    Color GetItemColor()
    {
        return itemType switch
        {
            ItemType.Protractor => Color.yellow,
            ItemType.Basketball => new Color(1f, 0.5f, 0f), // Orange
            ItemType.Paintbrush => Color.magenta,
            ItemType.Apple => Color.red,
            ItemType.PrankKit => Color.green,
            ItemType.Food => Color.brown,
            ItemType.Book => Color.cyan,
            ItemType.Pen => Color.gray,
            _ => Color.white
        };
    }

    // ========== IINTERACTABLE IMPLEMENTATION ==========

    public void OnInteract(GameObject interactor)
    {
        if (isCollected) return;

        PlayerController player = interactor.GetComponent<PlayerController>();
        if (player != null)
        {
            CollectItem(player);
        }
    }

    public string GetInteractionPrompt()
    {
        string rarity = isRareItem ? " (RARE)" : "";
        return $"Press E to pick up {itemName}{rarity}\nValue: {voteValue} votes";
    }

    public bool CanInteract(GameObject interactor)
    {
        if (isCollected) return false;

        PlayerController player = interactor.GetComponent<PlayerController>();
        return player != null && player.GetHeldItem() == ItemType.None;
    }

    public Transform GetTransform() => transform;

    // ========== COLLECTION ==========

    void CollectItem(PlayerController player)
    {
        isCollected = true;

        // Stop animations
        StopAllCoroutines();

        // Give item to player
        player.PickUpItem(this);

        // Play collection effects
        PlayCollectEffects();

        // Hide visual
        if (itemRenderer != null)
            itemRenderer.enabled = false;

        if (itemLight != null)
            itemLight.enabled = false;

        if (floatParticles != null)
            floatParticles.Stop();

        if (glowParticles != null)
            glowParticles.Stop();

        if (highlightRing != null)
            highlightRing.SetActive(false);

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Disable collider
        if (itemCollider != null)
            itemCollider.enabled = false;

        // Respawn or destroy
        if (canRespawn)
        {
            if (respawnCoroutine != null)
                StopCoroutine(respawnCoroutine);
            respawnCoroutine = StartCoroutine(RespawnAfterDelay());
        }
        else
        {
            Destroy(gameObject, 2f);
        }
    }

    void PlayCollectEffects()
    {
        // Particle effect
        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        // Sound effect
        if (audioSource != null)
        {
            AudioClip sound = isRareItem ? rarePickupSound : pickupSound;
            if (sound != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(sound);
            }
        }

        // Camera shake (optional)
        // if (CameraShake.Instance != null)
        //     CameraShake.Instance.Shake(0.2f, 0.1f);
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnTime);

        // Reset item
        isCollected = false;

        if (itemRenderer != null)
            itemRenderer.enabled = true;

        if (itemLight != null)
            itemLight.enabled = true;

        if (floatParticles != null)
            floatParticles.Play();

        if (glowParticles != null && isRareItem)
            glowParticles.Play();

        if (itemCollider != null)
            itemCollider.enabled = true;

        // Reset position
        transform.position = startPosition;

        // Restart animations
        StartCoroutine(FloatAnimation());

        Debug.Log($"Item {itemName} respawned");
    }

    // ========== HIGHLIGHT ==========

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            Highlight(true);
        }

        Debug.Log("Trigger Entered"); 
    }

    void OnTriggerStay(Collider other)
    {

        if (isCollected) return;

        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            // Update prompt if needed
            if (promptCanvasGroup != null && promptCanvasGroup.alpha < 1f)
            {
                promptCanvasGroup.alpha = Mathf.Lerp(promptCanvasGroup.alpha, 1f, Time.deltaTime * promptFadeSpeed);
            }
        }

        Debug.Log("Inside Trigger"); 
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            Highlight(false);

            if (promptCanvasGroup != null)
            {
                StartCoroutine(FadePrompt(0f));
            }
        }

        Debug.Log("Exited trigger"); 
    }

    void Highlight(bool state)
    {
        isHighlighted = state;

        // Highlight ring
        if (highlightRing != null)
            highlightRing.SetActive(state);

        // Material highlight
        if (itemRenderer != null && highlightMaterial != null)
        {
            itemRenderer.material = state ? highlightMaterial : originalMaterial;
        }

        // Show/hide prompt
        if (interactionPrompt != null)
        {
            if (state)
            {
                UpdatePromptText();
                StartCoroutine(FadePrompt(1f));
            }
            else
            {
                StartCoroutine(FadePrompt(0f));
            }
        }
    }

    IEnumerator FadePrompt(float targetAlpha)
    {
        if (promptCanvasGroup == null) yield break;

        float startAlpha = promptCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * promptFadeSpeed;
            promptCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed);
            yield return null;
        }

        promptCanvasGroup.alpha = targetAlpha;
    }

    public void OnDropped()
    {

    }

    // ========== GIZMOS ==========

    void OnDrawGizmosSelected()
    {
        // Show interaction range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 2f);

        // Show hover range
        Gizmos.color = isRareItem ? Color.yellow : Color.white;
        Vector3 hoverTop = transform.position + Vector3.up * hoverHeight;
        Vector3 hoverBottom = transform.position - Vector3.up * hoverHeight;
        Gizmos.DrawLine(hoverBottom, hoverTop);

        // Draw item type
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2,
            $"{itemName}\nValue: {voteValue}",
            new GUIStyle { fontSize = 12, normal = { textColor = GetItemColor() } });
#endif
    }
}