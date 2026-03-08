using UnityEngine;

public class EmergencyExit : MonoBehaviour
{
    [Header("Exit Settings")]
    [SerializeField] private bool isEmergencyExit = true;
    [SerializeField] private float openAngle = 90f;

    [Header("Visuals")]
    [SerializeField] private Light exitLight;
    [SerializeField] private Material emergencyMaterial;
    [SerializeField] private MeshRenderer exitSign;

    private DoorController doorController;
    private Material originalMaterial;

    void Start()
    {
        doorController = GetComponent<DoorController>();

        if (exitSign != null && emergencyMaterial != null)
        {
            originalMaterial = exitSign.material;
        }

        if (exitLight != null)
            exitLight.color = Color.green;
    }

    public void SetEmergencyMode(bool active)
    {
        if (doorController != null)
        {
            doorController.SetEmergencyForced(active);

            if (active)
            {
                doorController.OpenDoor();

                // Visual feedback
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
                doorController.CloseDoor();

                // Reset visuals
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
}