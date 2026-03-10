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

    [Header("Teacher Penalty")]
    [SerializeField] private bool applyTeacherPenalty = false;
    [SerializeField] private float teacherPenaltyMultiplier = 0.75f;
    [SerializeField] private float teacherRepThreshold = 0.8f;

    private float lastDepositTime;
    private int currentPlayerInZone = 0;
    private Material originalMaterial;
    private Color originalLightColor;

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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            currentPlayerInZone++;
            Debug.Log($"Player entered {assignedGroup} zone. Players in zone: {currentPlayerInZone}");

            if (highlightEffect != null)
                highlightEffect.SetActive(true);

            if (zoneLight != null)
                StartCoroutine(PulseLight());
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
        float elapsed = 0f;
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

    public void ProcessDeposit(PlayerController player)
    {
        if (player == null) return;

        if (Time.time < lastDepositTime + depositCooldown)
            return;

        int ballots = player.GetBallotCount();

        if (ballots <= 0)
        {
            UIManager.Instance?.ShowPlayerMessage(
                player.GetPlayerNumber(),
                "You have no ballots to dump!",
                Color.yellow
            );
            return;
        }

        lastDepositTime = Time.time;

        float totalMultiplier = multiplier;

        // Group reputation multiplier
        float repMultiplier = player.GetVoteMultiplierForNPC(assignedGroup);
        totalMultiplier *= repMultiplier;

        // Teacher reputation penalty
        if (ReputationManager.Instance != null)
        {
            float teacherRep = ReputationManager.Instance.GetReputation(
                player.GetPlayerNumber(),
                NPCMovement.NPCGroup.Teacher
            );

            if (teacherRep < 0.4f)
            {
                totalMultiplier *= 0.6f;
                UIManager.Instance?.ShowPlayerMessage(
                    player.GetPlayerNumber(),
                    "Teachers severely reduced your campaign impact!",
                    Color.red
                );
            }
            else if (teacherRep < 0.7f)
            {
                totalMultiplier *= 0.8f;
                UIManager.Instance?.ShowPlayerMessage(
                    player.GetPlayerNumber(),
                    "Teachers reduced your campaign impact!",
                    Color.yellow
                );
            }
        }

        int votesEarned = Mathf.RoundToInt(ballots * totalMultiplier);

        player.AddVotes(votesEarned);
        player.ClearBallots();

        NotificationManager.Instance?.SpawnNotification(
            $"+{votesEarned} Votes!",
            Color.green,
            transform.position
        );

        UIManager.Instance?.ShowPlayerMessage(
            player.GetPlayerNumber(),
            $"Dumped {ballots} ballots for +{votesEarned} votes!",
            Color.green
        );

        PlayDepositEffects(votesEarned);

        if (ballotVisualPrefab != null)
        {
            for (int i = 0; i < ballots; i++)
            {
                Vector3 spawnPos = player.transform.position + Random.insideUnitSphere * 0.5f;
                spawnPos.y = Mathf.Max(spawnPos.y, player.transform.position.y + 0.5f);

                GameObject ballot = Instantiate(ballotVisualPrefab, spawnPos, Quaternion.identity);
                StartCoroutine(FlyToZone(ballot));
            }
        }
    }

    IEnumerator FlyToZone(GameObject ballot)
    {
        Vector3 start = ballot.transform.position;
        Vector3 target = transform.position + Vector3.up * 1f;

        float time = 0f;
        float duration = 0.4f;

        while (time < duration && ballot != null)
        {
            time += Time.deltaTime;
            ballot.transform.position = Vector3.Lerp(start, target, time / duration);
            yield return null;
        }

        if (ballot != null)
            Destroy(ballot);
    }

    void PlayDepositEffects(int points)
    {
        if (depositEffect != null)
            depositEffect.Play();

        if (zoneLight != null)
            StartCoroutine(FlashLight());

        if (depositSound != null && audioSource != null)
            audioSource.PlayOneShot(depositSound);

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