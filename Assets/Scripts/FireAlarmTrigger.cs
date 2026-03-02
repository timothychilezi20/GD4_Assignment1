using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class FireAlarmTrigger : MonoBehaviour
{
    [Header("Alarm")]
    [SerializeField] private float activationCost = 20f;
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
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (alarmSystem == null)
        {
            alarmSystem = FindFirstObjectByType<FireAlarmSystem>(); 
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        UpdateAlarmVisual(); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isOnCooldown && (other.CompareTag("Player1") || other.CompareTag("Player2")))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            PlayerPoints points = other.GetComponent<PlayerPoints>();

            if (player != null && points != null)
            {
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(true);
                }

                if (points.GetHeldVotes() >= activationCost)
                {
                    ShowPrompt($"Press E to trigger Fire Alarm ({activationCost} votes)", Color.green);
                }
                else
                {
                    ShowPrompt($"Need {activationCost} votes! (You have {points.GetHeldVotes()})", Color.red);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isOnCooldown)
        {
            return; 
        }

        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            PlayerController player = other.GetComponent<PlayerController> ();
            PlayerPoints points = other.GetComponent<PlayerPoints>();

            if (player != null && points != null && player.GetPlayerNumber() > 0)
            {
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Slash)) //need to find for new input system
                {
                    TryActivateAlarm(other.gameObject, points); 
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
       if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    void TryActivateAlarm(GameObject player, PlayerPoints points)
    {
        if (points.GetHeldVotes() >= activationCost)
        {
            points.RemoveHeldVotes(Mathf.RoundToInt(activationCost));

            if (alarmSystem != null)
            {
                int playerNumber = player.GetComponent<PlayerController>().GetPlayerNumber();
                alarmSystem.TriggerAlarm(playerNumber, alarmDuration);
            }

            if (pullSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(pullSound);
            }

            if (alarmLight != null)
            {
                StartCoroutine(AlarmFlashLight());
            }

            isOnCooldown = true;
            cooldownTimer = cooldownTime;
            UpdateAlarmVisual();

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            Debug.Log($"Player {player.GetComponent<PlayerController>().GetPlayerNumber()} triggered fire alarm! Cost: {activationCost} votes");
        }
        else
        {
            if (errorSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(errorSound);
            }

            Debug.Log($"Not enough votes! Need {activationCost} "); 
        }
    }

    void ShowPrompt(string message, Color color)
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

    IEnumerator AlarmFlashLight()
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
        {
            alarmLight.enabled = false; 
        }
    }

    private void UpdateAlarmVisual()
    {
       if (alarmRenderer != null && readyMaterial != null && cooldownMaterial != null)
        {
            alarmRenderer.material = isOnCooldown ? cooldownMaterial : readyMaterial;
        }
    }
}
