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

    [Header("Cameras for Split Screen")]
    public Camera player1Camera;
    public Camera player2Camera;

    private Dictionary<int, List<GameObject>> playerPopups;
    private bool isQuitting = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (debugMode) Debug.Log("ReputationPopupManager Instance created (persistent)");
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

        if (screenSpaceCanvas == null)
        {
            CreateScreenSpaceCanvas();
        }

        if (debugMode)
        {
            Debug.Log($"ReputationPopupManager Initialized:");
            Debug.Log($"- Prefab assigned: {reputationPopupPrefab != null}");
            Debug.Log($"- Canvas assigned: {screenSpaceCanvas != null}");
            Debug.Log($"- Player1 Camera assigned: {player1Camera != null}");
            Debug.Log($"- Player2 Camera assigned: {player2Camera != null}");
        }
    }

    void CreateScreenSpaceCanvas()
    {
        screenSpaceCanvas = FindFirstObjectByType<Canvas>();
        if (screenSpaceCanvas != null)
        {
            if (debugMode) Debug.Log("Found existing Canvas in scene");
            return;
        }

        GameObject canvasObj = new GameObject("ReputationPopupCanvas");
        screenSpaceCanvas = canvasObj.AddComponent<Canvas>();
        screenSpaceCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        if (debugMode) Debug.Log("Created new Screen Space Canvas for popups");
    }

    void Update()
    {
        if (screenSpaceCanvas == null && !isQuitting)
        {
            if (debugMode) Debug.LogWarning("Canvas was destroyed! Recreating...");
            CreateScreenSpaceCanvas();
        }

        if (Time.frameCount % 120 == 0)
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
        if (isQuitting) return;

        if (reputationPopupPrefab == null)
        {
            Debug.LogError("reputationPopupPrefab is NULL!");
            return;
        }

        if (screenSpaceCanvas == null)
        {
            CreateScreenSpaceCanvas();
            if (screenSpaceCanvas == null) return;
        }

        // Select camera based on player number
        Camera popupCamera = playerNumber switch
        {
            1 => player1Camera != null ? player1Camera : Camera.main,
            2 => player2Camera != null ? player2Camera : Camera.main,
            _ => Camera.main
        };

        if (popupCamera == null)
        {
            Debug.LogError("No camera found for popup!");
            return;
        }

        // Convert world position to viewport, then to canvas space
        Vector3 viewportPos = popupCamera.WorldToViewportPoint(worldPosition);
        Vector3 canvasPos = new Vector3(
            viewportPos.x * screenSpaceCanvas.pixelRect.width,
            viewportPos.y * screenSpaceCanvas.pixelRect.height,
            0
        );

        // Add random offset
        Vector2 randomOffset = new Vector2(
            Random.Range(-randomOffsetX, randomOffsetX),
            popupOffsetY
        );
        canvasPos += (Vector3)randomOffset;

        // Create popup
        GameObject popupObj = Instantiate(reputationPopupPrefab, screenSpaceCanvas.transform);
        popupObj.name = $"RepPopup_P{playerNumber}_{group}_{changeAmount}_{Time.time}";

        RectTransform rectTransform = popupObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.position = canvasPos;
        }

        // Track popup
        if (!playerPopups.ContainsKey(playerNumber))
            playerPopups[playerNumber] = new List<GameObject>();

        playerPopups[playerNumber].Add(popupObj);

        // Limit popups per player
        while (playerPopups[playerNumber].Count > maxPopupsPerPlayer)
        {
            GameObject oldest = playerPopups[playerNumber][0];
            if (oldest != null) Destroy(oldest);
            playerPopups[playerNumber].RemoveAt(0);
        }

        // Set popup content
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