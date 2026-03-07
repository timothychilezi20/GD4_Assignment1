using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class DumpZone : MonoBehaviour
{
    [Header("Dump Zone Settings")]
    public float multiplier = 1.0f;
    public NPCMovement.NPCGroup assignedGroup;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem depositEffect;
    [SerializeField] private Light zoneLight;
    [SerializeField] private MeshRenderer zoneRenderer;
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject ballotVisualPrefab; 

    [Header("Audio")]
    [SerializeField] private AudioClip depositSound;
    [SerializeField] private AudioClip activeSound;
    private AudioSource audioSource;

    [Header("Interaction")]
    [SerializeField] private float depositCooldown = 0.5f;
    [SerializeField] private GameObject highlightEffect;
    [SerializeField] private Color highlightColor = Color.white;

    private float lastDepositTime;
    private int currentPlayerInZone = 0;
    private Material originalMaterial;
    private Color originalLightColor;
    private bool isHighlighted = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (zoneRenderer != null)
            originalMaterial = zoneRenderer.material;

        if (zoneLight != null)
            originalLightColor = zoneLight.color;

        if (multiplierText != null)
            multiplierText.text = $"{multiplier:F1}x";

        if (highlightEffect != null)
            highlightEffect.SetActive(false);

        SetZoneColor();
    }

    void Update()
    {
        // Rotate multiplier text to face camera
        if (multiplierText != null && Camera.main != null)
        {
            multiplierText.transform.rotation = Quaternion.LookRotation(
                multiplierText.transform.position - Camera.main.transform.position
            );
        }
    }

    void SetZoneColor()
    {
        Color zoneColor = assignedGroup switch
        {
            NPCMovement.NPCGroup.Nerd => Color.cyan,
            NPCMovement.NPCGroup.Athlete => Color.green,
            NPCMovement.NPCGroup.Artist => Color.magenta,
            NPCMovement.NPCGroup.Teacher => Color.yellow,
            NPCMovement.NPCGroup.Grade8 => Color.gray,
            _ => Color.white
        };

        if (zoneLight != null)
        {
            zoneLight.color = zoneColor;
            originalLightColor = zoneColor;
        }

        if (zoneRenderer != null)
        {
            zoneRenderer.material.color = zoneColor;
            zoneRenderer.material.SetColor("_EmissionColor", zoneColor * 0.5f);
        }

        if (multiplierText != null)
            multiplierText.color = zoneColor;
    }

    // ========== TRIGGER METHODS ==========

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            currentPlayerInZone++;
            Debug.Log($"Player entered {assignedGroup} zone. Players in zone: {currentPlayerInZone}");

            // Enable highlight
            if (highlightEffect != null)
                highlightEffect.SetActive(true);

            // Pulse light
            if (zoneLight != null)
                StartCoroutine(PulseLight());
        }
    }

    void OnTriggerStay(Collider other)
    {
       
        // Only process if it's a player
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null) return;

            PlayerInput playerInput = other.GetComponent<PlayerInput>();
            if (playerInput == null) return;


            // Check if the player pressed the Interact button (E)
            if (playerInput.actions["Interact"].WasPressedThisFrame())
            {
                Debug.Log($"Player {player.GetPlayerNumber()} pressed E in {assignedGroup} zone");

                // Check if player has votes
                int heldVotes = player.GetHeldVotes();
                Debug.Log("Player votes: " + heldVotes);
                if (heldVotes > 0)
                {
                    Debug.Log($"Player has {heldVotes} votes. Processing deposit...");
                    ProcessDeposit(player);
                }
                else
                {
                    Debug.Log("Player has no votes to deposit");
                    // Optional: Show a notification
                    if (NotificationManager.Instance != null)
                        NotificationManager.Instance.SpawnNotification("+10 Votes!", Color.green, transform.position);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            currentPlayerInZone--;
            Debug.Log($"Player left {assignedGroup} zone. Players in zone: {currentPlayerInZone}");

            if (currentPlayerInZone <= 0 && highlightEffect != null)
                highlightEffect.SetActive(false);
        }
    }

    IEnumerator PulseLight()
    {
        if (zoneLight == null) yield break;

        float duration = 1f;
        float elapsed = 0;
        float originalIntensity = zoneLight.intensity;

        while (elapsed < duration && currentPlayerInZone > 0)
        {
            elapsed += Time.deltaTime;
            float pulse = Mathf.Sin(Time.time * 5f) * 0.5f + 0.5f;
            zoneLight.intensity = originalIntensity + pulse;
            yield return null;
        }

        zoneLight.intensity = originalIntensity;
    }

    // ========== DEPOSIT LOGIC ==========

    public void ProcessDeposit(PlayerController player)
    {
        Debug.Log($"DUMP ZONE PROCESSING: {assignedGroup} zone");

        int heldVotes = player.GetHeldVotes();
        if (heldVotes <= 0)
        {
            Debug.Log("No votes to deposit");
            return;
        }

        lastDepositTime = Time.time;

        // Calculate points with multipliers
        int pointsEarned = Mathf.RoundToInt(heldVotes * multiplier);

        // Apply reputation multiplier
        float repMultiplier = player.GetVoteMultiplierForNPC(assignedGroup);
        pointsEarned = Mathf.RoundToInt(pointsEarned * repMultiplier);

        // Add to GameManager
        //if (TwoPlayerGameManager.Instance != null)
        //{
        //    TwoPlayerGameManager.Instance.AddPlayerVotes(pointsEarned, player.GetPlayerNumber());
        //}
        //else
        //{
        //    Debug.LogWarning("No TwoPlayerGameManager found - points not added");
        //}

        // Clear player's held votes
        player.ClearHeldVotes();

        // --- UI Notification ---
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.SpawnNotification($"+{pointsEarned} Votes!", Color.green, transform.position);
        }

        // Play effects
        PlayDepositEffects(pointsEarned);

        // Optional: remove 3D floating text
        // SpawnFloatingPoints(pointsEarned);

        for (int i = 0; i < heldVotes; i++)
        {
            Vector3 spawnPos = player.transform.position + Random.insideUnitSphere * 0.5f; 

            GameObject ballot = Instantiate(ballotVisualPrefab, spawnPos, Quaternion.identity);

            StartCoroutine(FlyToZone(ballot)); 
        }
    }

    IEnumerator FlyToZone(GameObject ballot)
    {
        Vector3 start = ballot.transform.position;
        Vector3 target = transform.position + Vector3.up * 1f;

        float time = 0;
        float duration = 0.4f; 

        while (time < duration)
        {
            time += Time.deltaTime;
            ballot.transform.position = Vector3.Lerp(start, target, time / duration);
            yield return null;
        }

        Destroy( ballot );
    }

    void PlayDepositEffects(int points)
    {
        // Particle effect
        if (depositEffect != null)
            depositEffect.Play();

        // Light flash
        if (zoneLight != null)
            StartCoroutine(FlashLight());

        // Sound
        if (depositSound != null && audioSource != null)
            audioSource.PlayOneShot(depositSound);

        // Animation
        if (animator != null)
            animator.SetTrigger("Deposit");
    }

    IEnumerator FlashLight()
    {
        if (zoneLight == null) yield break;

        float originalIntensity = zoneLight.intensity;
        zoneLight.intensity = 5f;

        yield return new WaitForSeconds(0.2f);

        zoneLight.intensity = originalIntensity;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = assignedGroup switch
        {
            NPCMovement.NPCGroup.Nerd => Color.cyan,
            NPCMovement.NPCGroup.Athlete => Color.green,
            NPCMovement.NPCGroup.Artist => Color.magenta,
            NPCMovement.NPCGroup.Teacher => Color.yellow,
            NPCMovement.NPCGroup.Grade8 => Color.gray,
            _ => Color.white
        };

        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(collider.center, collider.size);
        }
    }
}