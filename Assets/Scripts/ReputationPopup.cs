using UnityEngine;
using TMPro;
using System.Collections;

public class ReputationPopup : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 50f; // Screen space movement (pixels)
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float lifetime = 2f;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private CanvasGroup canvasGroup;

    private Vector2 startPosition;
    private float startTime;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (popupText == null)
            popupText = GetComponentInChildren<TextMeshProUGUI>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        startPosition = rectTransform.position;
        startTime = Time.time;

        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (rectTransform == null) return;

        // Move upward in screen space
        float progress = (Time.time - startTime) / lifetime;
        rectTransform.position = startPosition + Vector2.up * moveSpeed * progress;

        // Fade out
        if (canvasGroup != null)
        {
            float alpha = 1f - progress;
            canvasGroup.alpha = Mathf.Clamp01(alpha);
        }
    }

    public void SetPopup(float changeAmount, NPCMovement.NPCGroup group, int playerNumber)
    {
        if (popupText == null) return;

        // Format the message
        string symbol = changeAmount > 0 ? "+" : "";
        string message = $"{symbol}{changeAmount:F2} {group}";

        // Add player indicator
        string playerColorHex = playerNumber == 1 ? "#3498db" : "#e74c3c";
        message = $"<color={playerColorHex}>P{playerNumber}</color> {message}";

        // Set color based on positive/negative
        Color color = changeAmount > 0 ? Color.green : Color.red;

        popupText.text = message;
        popupText.color = color;
        popupText.fontSize = 36;
        popupText.outlineWidth = 0.2f;
        popupText.outlineColor = Color.black;
    }
}