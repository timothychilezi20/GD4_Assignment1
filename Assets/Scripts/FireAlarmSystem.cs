using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.UI;
using TMPro; 

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
    [SerializeField] private List<FireAssemblyPoint> assemblyPoints = new List<FireAssemblyPoint>();

    [Header("Visual Effects")]
    [SerializeField] private Light[] alarmLights;
    [SerializeField] private float lightFlashRate = 0.3f;

    [Header("Audio")]
    [SerializeField] private AudioClip alarmSound;
    [SerializeField, Range(0f, 1f)] private float alarmVolume = 0.7f;

    [Header("UI")]
    [SerializeField] private GameObject alarmUIPanel;
    [SerializeField] private TextMeshProUGUI alarmTimerText;

    private AudioSource audioSource;
    private Coroutine flashCoroutine;
    private Coroutine timerCoroutine;

    public System.Action<int> OnAlarmTriggered;
    public System.Action OnAlarmEnded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (allDoors.Count == 0)
            allDoors.AddRange(FindObjectsByType<DoorController>(FindObjectsSortMode.None));

        if (emergencyExits.Count == 0)
            emergencyExits.AddRange(FindObjectsByType<EmergencyExit>(FindObjectsSortMode.None));

        if (assemblyPoints.Count == 0)
            assemblyPoints.AddRange(FindObjectsByType<FireAssemblyPoint>(FindObjectsSortMode.None));

        if (alarmUIPanel != null)
            alarmUIPanel.SetActive(false);

        Debug.Log($"Fire Alarm System initialized with {allDoors.Count} doors, {emergencyExits.Count} emergency exits, and {assemblyPoints.Count} assembly points.");
    }

    public void TriggerAlarm(int playerNumber, float duration)
    {
        if (isAlarmActive) return;

        isAlarmActive = true;
        alarmEndTime = Time.time + duration;
        triggeredByPlayer = playerNumber;

        foreach (DoorController door in allDoors)
        {
            if (door == null) continue;
            door.CloseDoor(true);
            door.SetLocked(true);
        }

        foreach (EmergencyExit exit in emergencyExits)
        {
            if (exit == null) continue;
            exit.SetEmergencyMode(true);
        }

        foreach (FireAssemblyPoint point in assemblyPoints)
        {
            if (point == null) continue;
            point.Activate();
        }

        TeacherController[] teachers = FindObjectsByType<TeacherController>(FindObjectsSortMode.None);
        foreach (TeacherController teacher in teachers)
        {
            if (teacher == null) continue;
            teacher.SetFireAlarmMode(true);
        }

        if (alarmLights.Length > 0)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(FlashLights());
        }

        if (alarmSound != null && audioSource != null)
        {
            audioSource.clip = alarmSound;
            audioSource.loop = true;
            audioSource.volume = alarmVolume;
            audioSource.Play();
        }

        if (alarmUIPanel != null)
            alarmUIPanel.SetActive(true);

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        timerCoroutine = StartCoroutine(AlarmTimer(duration));

        OnAlarmTriggered?.Invoke(playerNumber);

        Debug.Log($"🚨 FIRE ALARM triggered by Player {playerNumber}! Duration: {duration}s");
    }

    private IEnumerator AlarmTimer(float duration)
    {
        float timeRemaining = duration;

        while (timeRemaining > 0f)
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

    private void EndAlarm()
    {
        isAlarmActive = false;

        foreach (DoorController door in allDoors)
        {
            if (door == null) continue;
            door.SetLocked(false);
            door.OpenDoor();
        }

        foreach (EmergencyExit exit in emergencyExits)
        {
            if (exit == null) continue;
            exit.SetEmergencyMode(false);
        }

        foreach (FireAssemblyPoint point in assemblyPoints)
        {
            if (point == null) continue;
            point.Deactivate();
        }

        TeacherController[] teachers = FindObjectsByType<TeacherController>(FindObjectsSortMode.None);
        foreach (TeacherController teacher in teachers)
        {
            if (teacher == null) continue;
            teacher.SetFireAlarmMode(false);
        }

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        foreach (Light light in alarmLights)
        {
            if (light != null)
                light.enabled = false;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        if (alarmUIPanel != null)
            alarmUIPanel.SetActive(false);

        OnAlarmEnded?.Invoke();

        Debug.Log("Fire alarm ended");
    }

    private IEnumerator FlashLights()
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
    public float GetTimeRemaining() => Mathf.Max(0f, alarmEndTime - Time.time);
}