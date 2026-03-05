using UnityEngine;
using TMPro;
using System.Collections;

public class DroppedVotes : MonoBehaviour
{
    [Header("Vote Data")]
    private int voteAmount;
    private NPCMovement.NPCGroup sourceGroup; // This is the group
    private int droppedByPlayer; // This is the player number (keep this separate)
    private float lifetime = 8f;
    private float timeRemaining;

    [Header("Visual")]
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private MeshRenderer voteRenderer;
    [SerializeField] private Light voteLight;
    [SerializeField] private ParticleSystem collectEffect;

    [Header("Animation")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float pulseSpeed = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    private AudioSource audioSource;

    private Vector3 startPosition;
    private float bobOffset;
    private bool isCollected = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        startPosition = transform.position;
        bobOffset = Random.Range(0f, Mathf.PI * 2f);
        timeRemaining = lifetime;

        UpdateVisuals();

        // Start fade out
        StartCoroutine(FadeOut());
    }

    void Update()
    {
        if (isCollected) return;

        // Bobbing animation
        Vector3 newPosition = startPosition;
        newPosition.y += Mathf.Sin((Time.time * bobSpeed) + bobOffset) * bobHeight;
        transform.position = newPosition;

        // Rotation animation
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Pulse scale based on value
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.05f;
        transform.localScale = Vector3.one * pulse;

        // Update timer
        timeRemaining -= Time.deltaTime;
    }

    // ✅ FIXED: Now takes NPCGroup, not int for the second parameter
    public void Initialize(int amount, NPCMovement.NPCGroup group, int playerNumber = 0)
    {
        voteAmount = amount;
        sourceGroup = group;
        droppedByPlayer = playerNumber;

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        // Update text
        if (valueText != null)
        {
            valueText.text = voteAmount.ToString();
            valueText.color = GetTextColor();
            valueText.fontSize = 24 + (voteAmount * 2);
        }

        // Update material color based on NPC group
        if (voteRenderer != null)
        {
            voteRenderer.material.color = GetGroupColor();

            // Make it glow based on value
            float emissionIntensity = Mathf.Lerp(0.5f, 2f, (float)voteAmount / 20f);
            voteRenderer.material.SetColor("_EmissionColor", GetGroupColor() * emissionIntensity);
        }

        // Update light
        if (voteLight != null)
        {
            voteLight.color = GetGroupColor();
            voteLight.intensity = Mathf.Lerp(0.5f, 3f, (float)voteAmount / 20f);
        }

        // Set name for debugging
        string playerInfo = droppedByPlayer > 0 ? $"_P{droppedByPlayer}" : "";
        gameObject.name = $"Votes_{voteAmount}_{sourceGroup}{playerInfo}";
    }

    Color GetGroupColor()
    {
        return sourceGroup switch
        {
            NPCMovement.NPCGroup.Nerd => Color.cyan,
            NPCMovement.NPCGroup.Athlete => Color.green,
            NPCMovement.NPCGroup.Artist => Color.magenta,
            NPCMovement.NPCGroup.Teacher => Color.yellow,
            NPCMovement.NPCGroup.Grade8 => Color.gray,
            _ => Color.white
        };
    }

    Color GetTextColor()
    {
        if (voteAmount >= 15) return Color.yellow;
        if (voteAmount >= 8) return Color.green;
        return Color.white;
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0;
        Color originalColor = voteRenderer.material.color;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / lifetime);

            if (voteRenderer != null)
            {
                Color newColor = originalColor;
                newColor.a = alpha;
                voteRenderer.material.color = newColor;
            }

            if (valueText != null)
            {
                Color textColor = valueText.color;
                textColor.a = alpha;
                valueText.color = textColor;
            }

            yield return null;
        }

        if (!isCollected)
            Destroy(gameObject);
    }

    public int PickUp()
    {
        if (isCollected) return 0;

        isCollected = true;

        // Play effects
        if (collectEffect != null)
            Instantiate(collectEffect, transform.position, Quaternion.identity);

        if (pickupSound != null && audioSource != null)
            audioSource.PlayOneShot(pickupSound);

        Destroy(gameObject, 0.2f);
        return voteAmount;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                int amount = PickUp();
                player.AddVotes(amount);
                Debug.Log($"Player {player.GetPlayerNumber()} picked up {amount} votes from {sourceGroup}");
            }
        }
    }
}