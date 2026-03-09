using UnityEngine;

public class EmergencyExit : MonoBehaviour
{
    [Header("Exit Settings")]
    [SerializeField] private bool isEmergencyExit = true;

    [Header("Visuals")]
    [SerializeField] private Light exitLight;
    [SerializeField] private Material emergencyMaterial;
    [SerializeField] private MeshRenderer exitSign;

    private DoorController doorController;
    private Material originalMaterial;

    void Start()
    {
        doorController = GetComponent<DoorController>();

        if (exitSign != null)
            originalMaterial = exitSign.material;

        if (exitLight != null)
        {
            exitLight.color = Color.green;
            exitLight.enabled = false;
        }
    }

    public void SetEmergencyMode(bool active)
    {
        if (doorController == null) return;

        if (active)
        {
            // Emergency exits should be open during fire alarm
            doorController.SetLocked(false);
            doorController.OpenDoor();

            if (exitLight != null)
            {
                exitLight.color = Color.red;
                exitLight.enabled = true;
            }

            if (exitSign != null && emergencyMaterial != null)
                exitSign.material = emergencyMaterial;
        }
        else
        {
            // After alarm ends, keep exit unlocked but close it again
            doorController.SetLocked(false);
            doorController.CloseDoor(true);

            if (exitLight != null)
            {
                exitLight.color = Color.green;
                exitLight.enabled = false;
            }

            if (exitSign != null && originalMaterial != null)
                exitSign.material = originalMaterial;
        }
    }
}