using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private bool isOpen = true;
    [SerializeField] private bool isLocked = false;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float closedAngle = 0f;
    [SerializeField] private float smoothSpeed = 5f;

    [Header("References")]
    [SerializeField] private Transform doorHinge;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip lockedSound;

    private AudioSource audioSource;
    private float targetAngle;
    private bool emergencyForced = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (doorHinge == null)
            doorHinge = transform;

        targetAngle = isOpen ? openAngle : closedAngle;
    }

    void Update()
    {
        // Smoothly rotate door
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
        doorHinge.localRotation = Quaternion.Lerp(doorHinge.localRotation, targetRotation, smoothSpeed * Time.deltaTime);
    }

    public void OpenDoor()
    {
        if (isLocked && !emergencyForced)
        {
            Debug.Log("Door is locked!");
            if (lockedSound != null && audioSource != null)
                audioSource.PlayOneShot(lockedSound);
            return;
        }

        isOpen = true;
        targetAngle = openAngle;

        if (openSound != null && audioSource != null)
            audioSource.PlayOneShot(openSound);
    }

    public void CloseDoor(bool force = false)
    {
        if (isLocked && !force)
        {
            Debug.Log("Door is locked!");
            return;
        }

        isOpen = false;
        targetAngle = closedAngle;

        if (closeSound != null && audioSource != null)
            audioSource.PlayOneShot(closeSound);
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }

    public void SetEmergencyForced(bool forced)
    {
        emergencyForced = forced;
    }
}