using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReputationPopupManager : MonoBehaviour
{
    public static ReputationPopupManager Instance { get; private set; }

    [Header("Popup Prefab")]
    public GameObject reputationPopupPrefab;
    public Canvas screenSpaceCanvas;

    [Header("Settings")]
    public float popupOffsetY = 100f;
    public float randomOffsetX = 50f;
    public int maxPopupsPerPlayer = 5;
    public bool debugMode = true;

    [Header("Camera")]
    public Camera mainCamera;

    private Dictionary<int, List<GameObject>> playerPopups;
    private bool isQuitting = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep manager across scenes
            Debug.Log("ReputationPopupManager Instance created (persistent)");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeManager();
    }

    void InitializeManager()
    {
        playerPopups = new Dictionary<int, List<GameObject>>
        {
            { 1, new List<GameObject>() },
            { 2, new List<GameObject>() }
        };

        // Find camera if not assigned
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Create canvas if not assigned
        if (screenSpaceCanvas == null)
        {
            CreateScreenSpaceCanvas();
        }

        if (debugMode)
        {
            Debug.Log($"ReputationPopupManager Initialized:");
            Debug.Log($"- Prefab assigned: {reputationPopupPrefab != null}");
            Debug.Log($"- Canvas assigned: {screenSpaceCanvas != null}");
            Debug.Log($"- Camera assigned: {mainCamera != null}");
        }
    }

    void CreateScreenSpaceCanvas()
    {
        // Check if canvas already exists in scene
        screenSpaceCanvas = FindFirstObjectByType<Canvas>();
        if (screenSpaceCanvas != null)
        {
            Debug.Log("Found existing Canvas in scene");
            return;
        }

        GameObject canvasObj = new GameObject("ReputationPopupCanvas");
        screenSpaceCanvas = canvasObj.AddComponent<Canvas>();
        screenSpaceCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        Debug.Log("Created new Screen Space Canvas for popups");
    }

    void Update()
    {
        // Recover if canvas was destroyed
        if (screenSpaceCanvas == null && !isQuitting)
        {
            Debug.LogWarning("Canvas was destroyed! Recreating...");
            CreateScreenSpaceCanvas();
        }

        // Clean up destroyed popups periodically
        if (Time.frameCount % 120 == 0) // Every 2 seconds (assuming 60fps)
        {
            CleanupPopups();
        }
    }

    void CleanupPopups()
    {
        foreach (int playerNum in new List<int>(playerPopups.Keys))
        {
            if (playerPopups.ContainsKey(playerNum))
            {
                playerPopups[playerNum].RemoveAll(p => p == null);
            }
        }
    }

    public void ShowPopup(int playerNumber, NPCMovement.NPCGroup group, float changeAmount, Vector3 worldPosition)
    {
        Debug.Log($"=== POPUP ATTEMPT #{Time.frameCount} ===");
        Debug.Log($"Manager Instance: {Instance != null}");
        Debug.Log($"Canvas: {screenSpaceCanvas != null}");
        Debug.Log($"Prefab: {reputationPopupPrefab != null}");
        Debug.Log($"Camera: {mainCamera != null}");



        if (isQuitting) return;

        // Validate setup
        if (reputationPopupPrefab == null)
        {
            Debug.LogError("reputationPopupPrefab is NULL!");
            return;
        }

        if (screenSpaceCanvas == null)
        {
            Debug.Log("Canvas was null, recreating...");
            CreateScreenSpaceCanvas();
            if (screenSpaceCanvas == null) return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No camera found!");
                return;
            }
        }

        // Convert world position to screen position
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);

        // Add random offset
        Vector2 randomOffset = new Vector2(
            Random.Range(-randomOffsetX, randomOffsetX),
            popupOffsetY
        );

        // Create popup
        GameObject popupObj = Instantiate(reputationPopupPrefab, screenSpaceCanvas.transform);

        // Position it
        RectTransform rectTransform = popupObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.position = screenPos + (Vector3)randomOffset;
        }

        // Name it for debugging
        popupObj.name = $"RepPopup_P{playerNumber}_{group}_{changeAmount}_{Time.time}";

        // Track it
        if (!playerPopups.ContainsKey(playerNumber))
        {
            playerPopups[playerNumber] = new List<GameObject>();
        }
        playerPopups[playerNumber].Add(popupObj);

        // Limit popups
        playerPopups[playerNumber].RemoveAll(p => p == null);
        while (playerPopups[playerNumber].Count > maxPopupsPerPlayer)
        {
            GameObject oldest = playerPopups[playerNumber][0];
            if (oldest != null) Destroy(oldest);
            playerPopups[playerNumber].RemoveAt(0);
        }

        // Configure popup
        ReputationPopup popup = popupObj.GetComponent<ReputationPopup>();
        if (popup != null)
        {
            popup.SetPopup(changeAmount, group, playerNumber);
            if (debugMode) Debug.Log($"✓ Popup created for Player {playerNumber}");
        }
        else
        {
            Debug.LogError("ReputationPopup component missing on prefab!");
        }

        Debug.Log($"Popup created: {popupObj != null}, Parent: {popupObj.transform.parent != null}");
    }

    void OnDestroy()
    {
        isQuitting = true;
    }

    void OnApplicationQuit()
    {
        isQuitting = true;
    }
}