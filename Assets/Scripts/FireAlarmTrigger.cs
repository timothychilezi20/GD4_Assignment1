using UnityEngine;
using System.Collections;

public class FireAlarmTrigger : MonoBehaviour
{
    [Header("Alarm")]
    [SerializeField] private int activationCost = 20;
    [SerializeField] private float alarmDuration = 15f;
    [SerializeField] private float cooldownTime = 60f;

    [Header("References")]
    [SerializeField] private FireAlarmSystem alarmSystem;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private Light alarmLight;
    [SerializeField] private AudioClip pullSound;
    [SerializeField] private AudioClip errorSound;

    [Header("Visuals")]
    [SerializeField] private Material readyMaterial;
    [SerializeField] private Material cooldownMaterial;
    [SerializeField] private MeshRenderer alarmRenderer;

    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (alarmSystem == null)
            alarmSystem = FindFirstObjectByType<FireAlarmSystem>();

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        UpdateAlarmVisual();
    }

    private void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                cooldownTimer = 0f;
                UpdateAlarmVisual();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player1") && !other.CompareTag("Player2"))
            return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(true);

        if (isOnCooldown)
        {
            ShowPrompt($"Fire Alarm Cooling Down ({Mathf.CeilToInt(cooldownTimer)}s)", Color.yellow);
            return;
        }

        if (player.GetHeldVotes() >= activationCost)
        {
            ShowPrompt($"Press E to trigger Fire Alarm ({activationCost} votes)", Color.green);
        }
        else
        {
            ShowPrompt($"Need {activationCost} votes! (You have {player.GetHeldVotes()})", Color.red);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }

    public void TryActivateAlarm(PlayerController player)
    {
        if (player == null) return;

        if (isOnCooldown)
        {
            UIManager.Instance?.ShowPlayerMessage(
                player.GetPlayerNumber(),
                $"Fire alarm cooling down! {Mathf.CeilToInt(cooldownTimer)}s left",
                Color.yellow
            );

            if (errorSound != null && audioSource != null)
                audioSource.PlayOneShot(errorSound);

            return;
        }

        if (player.GetHeldVotes() < activationCost)
        {
            UIManager.Instance?.ShowPlayerMessage(
                player.GetPlayerNumber(),
                $"Not enough votes! Need {activationCost}",
                Color.red
            );

            if (errorSound != null && audioSource != null)
                audioSource.PlayOneShot(errorSound);

            return;
        }

        // Spend votes
        int newVotes = player.GetHeldVotes() - activationCost;
        player.ClearHeldVotes();
        if (newVotes > 0)
            player.AddVotes(newVotes);

        if (alarmSystem != null)
        {
            alarmSystem.TriggerAlarm(player.GetPlayerNumber(), alarmDuration);
        }

        if (pullSound != null && audioSource != null)
            audioSource.PlayOneShot(pullSound);

        if (alarmLight != null)
            StartCoroutine(AlarmFlashLight());

        isOnCooldown = true;
        cooldownTimer = cooldownTime;
        UpdateAlarmVisual();

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        UIManager.Instance?.ShowPlayerMessage(
            player.GetPlayerNumber(),
            "You triggered the fire alarm!",
            Color.red
        );

        Debug.Log($"Player {player.GetPlayerNumber()} triggered fire alarm! Cost: {activationCost} votes");
    }

    private void ShowPrompt(string message, Color color)
    {
        if (interactionPrompt != null)
        {
            TextMesh promptText = interactionPrompt.GetComponent<TextMesh>();
            if (promptText != null)
            {
                promptText.text = message;
                promptText.color = color;
            }
        }
    }

    private IEnumerator AlarmFlashLight()
    {
        float endTime = Time.time + alarmDuration;

        while (Time.time < endTime)
        {
            if (alarmLight != null)
            {
                alarmLight.enabled = !alarmLight.enabled;
                yield return new WaitForSeconds(0.3f);
            }
        }

        if (alarmLight != null)
            alarmLight.enabled = false;
    }

    private void UpdateAlarmVisual()
    {
        if (alarmRenderer != null && readyMaterial != null && cooldownMaterial != null)
        {
            alarmRenderer.material = isOnCooldown ? cooldownMaterial : readyMaterial;
        }
    }
}