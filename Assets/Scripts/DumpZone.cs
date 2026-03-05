using UnityEngine;
using TMPro;
using System.Collections;

public class DumpZone : MonoBehaviour, IInteractable
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

    public void OnInteract(GameObject interactor)
    {
        PlayerController player = interactor.GetComponent<PlayerController>();
        if (player != null && Time.time - lastDepositTime >= depositCooldown)
        {
            ProcessDeposit(player);
        }
    }

    public string GetInteractionPrompt()
    {
        return $"Press E to deposit votes\n{multiplier:F1}x multiplier";
    }

    public bool CanInteract(GameObject interactor)
    {
        PlayerController player = interactor.GetComponent<PlayerController>();
        return player != null &&
               player.GetHeldVotes() > 0 &&
               Time.time - lastDepositTime >= depositCooldown;
    }

    public Transform GetTransform() => transform;

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

    void ProcessDeposit(PlayerController player)
    {
        int heldVotes = player.GetHeldVotes();
        if (heldVotes <= 0) return;

        lastDepositTime = Time.time;

        // Calculate points with multipliers
        int pointsEarned = Mathf.RoundToInt(heldVotes * multiplier);

        // Apply reputation multiplier
        float repMultiplier = player.GetVoteMultiplierForNPC(assignedGroup);
        pointsEarned = Mathf.RoundToInt(pointsEarned * repMultiplier);

        // Add to player's score
        if (TwoPlayerGameManager.Instance != null)
        {
            TwoPlayerGameManager.Instance.AddPlayerVotes(pointsEarned, player.GetPlayerNumber());
        }

        // Clear player's held votes
        player.ClearHeldVotes();

        // Play deposit effects
        PlayDepositEffects(pointsEarned);

        Debug.Log($"Player {player.GetPlayerNumber()} deposited at {assignedGroup} zone: {heldVotes} votes → {pointsEarned} points (Multiplier: {multiplier}x, Rep: {repMultiplier:F2}x)");
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

        // Spawn floating points
        SpawnFloatingPoints(points);
    }

    IEnumerator FlashLight()
    {
        if (zoneLight == null) yield break;

        float originalIntensity = zoneLight.intensity;
        zoneLight.intensity = 5f;

        yield return new WaitForSeconds(0.2f);

        zoneLight.intensity = originalIntensity;
    }

    void SpawnFloatingPoints(int points)
    {
        if (multiplierText == null) return;

        GameObject floatingObj = new GameObject("FloatingPoints");
        floatingObj.transform.position = transform.position + Vector3.up * 3;

        TextMeshPro tmp = floatingObj.AddComponent<TextMeshPro>();
        tmp.text = $"+{points}";
        tmp.fontSize = points >= 20 ? 48 : 36;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = points >= 20 ? Color.yellow : Color.white;
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = Color.black;

        StartCoroutine(FloatText(floatingObj));
    }

    IEnumerator FloatText(GameObject textObj)
    {
        float duration = 1.5f;
        float elapsed = 0;
        Vector3 startPos = textObj.transform.position;
        TextMeshPro tmp = textObj.GetComponent<TextMeshPro>();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            textObj.transform.position = startPos + Vector3.up * (t * 2f);

            if (tmp != null)
                tmp.alpha = 1 - t;

            yield return null;
        }

        Destroy(textObj);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            currentPlayerInZone++;

            // Enable highlight
            if (highlightEffect != null)
                highlightEffect.SetActive(true);

            // Pulse light
            if (zoneLight != null)
                StartCoroutine(PulseLight());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            currentPlayerInZone--;

            if (currentPlayerInZone <= 0)
            {
                // Disable highlight
                if (highlightEffect != null)
                    highlightEffect.SetActive(false);
            }
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