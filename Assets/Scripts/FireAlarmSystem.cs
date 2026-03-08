using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class FireAlarmSystem : MonoBehaviour
{
    public static FireAlarmSystem Instance { get; private set; }

    [Header("Alarm State")]
    [SerializeField] private bool isAlarmActive = false;
    [SerializeField] private float alarmEndTime = 0f;
    [SerializeField] private int triggeredByPlayer = 0;

    [Header("Door System")]
    [SerializeField] private List<DoorController> allDoors = new List<DoorController>();
    [SerializeField] private List<EmergencyExit> emergencyExits = new List<EmergencyExit>();

    [Header("Visual Effects")]
    [SerializeField] private Light[] alarmLights;
    [SerializeField] private GameObject alarmSirenPrefab;
    [SerializeField] private float lightFlashRate = 0.3f;

    [Header("Audio")]
    [SerializeField] private AudioClip alarmSound;
    [SerializeField][Range(0f, 1f)] private float alarmVolume = 0.7f;

    [Header("UI")]
    [SerializeField] private GameObject alarmUIPanel;
    [SerializeField] private TextMesh alarmTimerText;

    private AudioSource audioSource;
    private Coroutine flashCoroutine;
    private Coroutine timerCoroutine;

    public System.Action<int> OnAlarmTriggered;
    public System.Action OnAlarmEnded;

    private void Awake()
    {
       if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (allDoors.Count == 0)
        {
            allDoors.AddRange(FindObjectsByType<DoorController>(FindObjectsSortMode.None));
        }

        if (emergencyExits.Count == 0)
        {
            emergencyExits.AddRange(FindObjectsByType<EmergencyExit>(FindObjectsSortMode.None));
        }

        if (alarmUIPanel != null)
        {
            alarmUIPanel.SetActive(false);
        }
        Debug.Log($"Fire Alarm System initialized with {allDoors.Count} doors and {emergencyExits.Count} emergency exits");
    }

    public void TriggerAlarm(int playerNumber, float duration)
    {
        if (isAlarmActive) return; // Can't trigger while already active

        isAlarmActive = true;
        alarmEndTime = Time.time + duration;
        triggeredByPlayer = playerNumber;

        // Close all regular doors
        foreach (DoorController door in allDoors)
        {
            door.CloseDoor(true); // Force close, ignoring normal operation
            door.SetLocked(true); // Lock doors during alarm
        }

        // Ensure emergency exits are open
        foreach (EmergencyExit exit in emergencyExits)
        {
            exit.SetEmergencyMode(true);
        }

        // Visual effects
        if (alarmLights.Length > 0)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashLights());
        }

        // Audio
        if (alarmSound != null && audioSource != null)
        {
            audioSource.clip = alarmSound;
            audioSource.loop = true;
            audioSource.volume = alarmVolume;
            audioSource.Play();
        }

        // UI
        if (alarmUIPanel != null)
            alarmUIPanel.SetActive(true);

        // Start timer
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(AlarmTimer(duration));

        // Notify other systems
        OnAlarmTriggered?.Invoke(playerNumber);

        Debug.Log($"🚨 FIRE ALARM triggered by Player {playerNumber}! Duration: {duration}s");
    }

    IEnumerator AlarmTimer(float duration)
    {
        float timeRemaining = duration;

        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            if (alarmTimerText != null)
            {
                int seconds = Mathf.CeilToInt(timeRemaining);
                alarmTimerText.text = $"ALARM: {seconds}s";
            }

            yield return null;
        }

        EndAlarm();
    }

    void EndAlarm()
    {
        isAlarmActive = false;

        // Open all regular doors
        foreach (DoorController door in allDoors)
        {
            door.SetLocked(false);
            door.OpenDoor();
        }

        // Reset emergency exits
        foreach (EmergencyExit exit in emergencyExits)
        {
            exit.SetEmergencyMode(false);
        }

        // Stop visual effects
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        // Reset lights
        foreach (Light light in alarmLights)
        {
            if (light != null)
                light.enabled = false;
        }

        // Stop audio
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        // Hide UI
        if (alarmUIPanel != null)
            alarmUIPanel.SetActive(false);

        // Notify
        OnAlarmEnded?.Invoke();

        Debug.Log("Fire alarm ended");
    }

    IEnumerator FlashLights()
    {
        while (isAlarmActive)
        {
            foreach (Light light in alarmLights)
            {
                if (light != null)
                    light.enabled = !light.enabled;
            }
            yield return new WaitForSeconds(lightFlashRate);
        }
    }

    public bool IsAlarmActive() => isAlarmActive;
    public int GetTriggeredByPlayer() => triggeredByPlayer;
    public float GetTimeRemaining() => Mathf.Max(0, alarmEndTime - Time.time);
}