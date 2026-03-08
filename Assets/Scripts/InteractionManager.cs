//using UnityEngine;
//using UnityEngine.InputSystem;
//using TMPro;
//using UnityEngine.UI; 

//public class InteractionManager : MonoBehaviour
//{
//    [Header("Settings")]
//    [SerializeField] private float interactionRange = 3f;
//    [SerializeField] private LayerMask interactableLayer;
//    [SerializeField] private Transform interactionPoint;
//    [SerializeField] private NotificationManager notificationManager;

//    [Header("UI")]
//    [SerializeField] private GameObject interactionPrompt;
//    [SerializeField] private TextMeshProUGUI promptText;
//    [SerializeField] private RawImage promptIcon;
//    [SerializeField] private CanvasGroup promptCanvasGroup;

//    [Header("Visual")]
//    [SerializeField] private LineRenderer lineRenderer;
//    [SerializeField] private Material validMaterial;
//    [SerializeField] private Material invalidMaterial;

//    private PlayerController player;
//    private Camera playerCamera;
//    private IInteractable currentInteractable;
//    private GameObject currentInteractableObject;
//    private float promptFadeSpeed = 5f;

//    void Start()
//    {
//        player = GetComponent<PlayerController>();
//        playerCamera = GetComponentInChildren<Camera>();

//        if (playerCamera == null)
//            playerCamera = Camera.main;

//        if (interactionPrompt != null)
//        {
//            promptCanvasGroup.alpha = 0;
//        }

//        if (lineRenderer != null)
//        {
//            lineRenderer.enabled = false;
//        }

//        if (notificationManager == null)
//        {
//            if (notificationManager == null)
//            {
//                notificationManager = FindFirstObjectByType<NotificationManager>();
//            }
//        }
//    }

//    void Update()
//    {
//        FindInteractable();
//        UpdatePrompt();
//        UpdateVisuals();
//    }

//    void FindInteractable()
//    {
//        Collider[] hits = Physics.OverlapSphere(
//            interactionPoint != null ? interactionPoint.position : transform.position,
//            interactionRange,
//            interactableLayer
//        );

//        IInteractable closestInteractable = null;
//        GameObject closestObject = null;
//        float closestDistance = float.MaxValue;

//        foreach (Collider hit in hits)
//        {
//            IInteractable interactable = hit.GetComponent<IInteractable>();
//            if (interactable != null && interactable.CanInteract(gameObject))
//            {
//                float distance = Vector3.Distance(
//                    interactionPoint != null ? interactionPoint.position : transform.position,
//                    hit.transform.position
//                );

//                if (distance < closestDistance)
//                {
//                    closestDistance = distance;
//                    closestInteractable = interactable;
//                    closestObject = hit.gameObject;
//                }
//            }
//        }

//        // Changed interactable?
//        if (closestInteractable != currentInteractable)
//        {
//            currentInteractable = closestInteractable;
//            currentInteractableObject = closestObject;

//            // Update prompt
//            if (currentInteractable != null)
//            {
//                ShowPrompt(currentInteractable.GetInteractionPrompt());
//            }
//        }
//    }

//    void UpdatePrompt()
//    {
//        if (promptCanvasGroup == null) return;

//        float targetAlpha = currentInteractable != null ? 1f : 0f;
//        promptCanvasGroup.alpha = Mathf.Lerp(
//            promptCanvasGroup.alpha,
//            targetAlpha,
//            Time.deltaTime * promptFadeSpeed
//        );
//    }

//    void UpdateVisuals()
//    {
//        if (lineRenderer != null)
//        {
//            if (currentInteractable != null && currentInteractableObject != null)
//            {
//                lineRenderer.enabled = true;

//                // Draw line from player to interactable
//                Vector3 startPos = interactionPoint != null ? interactionPoint.position : transform.position;
//                Vector3 endPos = currentInteractableObject.transform.position;

//                lineRenderer.SetPosition(0, startPos);
//                lineRenderer.SetPosition(1, endPos);

//                // Change color based on validity
//                bool canInteract = currentInteractable.CanInteract(gameObject);
//                lineRenderer.material = canInteract ? validMaterial : invalidMaterial;
//            }
//            else
//            {
//                lineRenderer.enabled = false;
//            }
//        }
//    }

//    void ShowPrompt(string text)
//    {
//        if (promptText != null)
//        {
//            promptText.text = text;
//        }
//    }

//    // Called by Player Input
//    public void OnInteract(InputAction.CallbackContext context)
//    {
//        if (context.performed && currentInteractable != null)
//        {
//            if (currentInteractable.CanInteract(gameObject))
//            {
//                currentInteractable.OnInteract(gameObject);

//                // Optional: haptic feedback
//                if (player != null)
//                {
//                    // Add controller rumble if using gamepad
//                }
//            }
//        }
//    }

//    // Public helper so other input handlers (like PlayerController) can trigger the selected interactable.
//    public void PerformInteract()
//    {
//        if (currentInteractable == null)
//        {
//            Debug.Log("InteractionManager.PerformInteract: no current interactable.");
//            return;
//        }

//        if (currentInteractable.CanInteract(gameObject))
//        {
//            currentInteractable.OnInteract(gameObject);
//        }
//        else
//        {
//            Debug.Log("InteractionManager.PerformInteract: CanInteract returned false.");
//        }
//    }

//    void OnDrawGizmosSelected()
//    {
//        Gizmos.color = Color.cyan;
//        Gizmos.DrawWireSphere(
//            interactionPoint != null ? interactionPoint.position : transform.position,
//            interactionRange
//        );
//    }
//}