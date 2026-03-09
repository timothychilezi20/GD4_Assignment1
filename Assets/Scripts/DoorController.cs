using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door State")]
    [SerializeField] private bool isOpen = true;
    [SerializeField] private bool isLocked = false;

    [Header("References")]
    [SerializeField] private GameObject doorObject;

    void Start()
    {
        if (doorObject == null)
            doorObject = gameObject;

        UpdateDoor();
    }

    void UpdateDoor()
    {
        if (doorObject != null)
            doorObject.SetActive(!isOpen);
    }

    public void OpenDoor()
    {
        if (isLocked)
        {
            Debug.Log("Door is locked!");
            return;
        }

        isOpen = true;
        UpdateDoor();
    }

    public void CloseDoor(bool force = false)
    {
        if (isLocked && !force)
        {
            Debug.Log("Door is locked!");
            return;
        }

        isOpen = false;
        UpdateDoor();
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }
}