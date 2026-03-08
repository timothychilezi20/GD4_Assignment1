using UnityEngine;
using TMPro;
using UnityEngine.UI; 

public class InteractionPrompt : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private RawImage backgroundImage;
    [SerializeField] private RawImage iconImage;

    [Header("Settings")]
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private float hoverHeight = 1f;
    [SerializeField] private float hoverSpeed = 2f;

    private Transform targetTransform;
    private Camera mainCamera;
    private float hoverOffset;

    void Start()
    {
        mainCamera = Camera.main;
        hoverOffset = Random.Range(0f, Mathf.PI * 2f);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    void Update()
    {
        if (targetTransform == null || mainCamera == null) return;

        // Position above target
        Vector3 worldPos = targetTransform.position + Vector3.up * hoverHeight;
        worldPos.y += Mathf.Sin((Time.time * hoverSpeed) + hoverOffset) * 0.2f;

        // Convert to screen position
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
        transform.position = screenPos;

        // Face camera (billboard)
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                        mainCamera.transform.rotation * Vector3.up);
    }

    public void Show(Transform target, string text, Color color)
    {
        targetTransform = target;

        if (promptText != null)
        {
            promptText.text = text;
            promptText.color = color;
        }

        if (backgroundImage != null)
            backgroundImage.color = new Color(color.r, color.g, color.b, 0.3f);

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    public void Hide()
    {
        targetTransform = null;

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }
}