using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Player HUDs")]
    public PlayerHUD player1HUD;
    public PlayerHUD player2HUD;

    [Header("World Prompt")]
    public GameObject interactPromptUI;       // The panel / canvas for the prompt
    public TextMeshProUGUI interactPromptText; // The text inside the panel

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Hide prompt initially
        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);
    }

    // --- Player HUD Updates ---
    public void UpdatePlayerVotes(int playerNumber, int votes)
    {
        PlayerHUD hud = GetPlayerHUD(playerNumber);
        hud?.UpdateVotes(votes);
    }

    public void UpdatePlayerBallots(int playerNumber, int ballots)
    {
        PlayerHUD hud = GetPlayerHUD(playerNumber);
        hud?.UpdateBallots(ballots);
    }

    public void UpdatePlayerItem(int playerNumber, ItemType item)
    {
        PlayerHUD hud = GetPlayerHUD(playerNumber);
        hud?.UpdateItem(item);
    }

    // --- Temporary popups / announcements ---
    public void ShowStealResult(int votesLost, int playerNumber)
    {
        PlayerHUD hud = GetPlayerHUD(playerNumber);
        hud?.ShowTemporaryMessage($"-{votesLost} Votes!", Color.red);
    }

    public void ShowTradeResult(string message, int playerNumber, Color? color = null)
    {
        PlayerHUD hud = GetPlayerHUD(playerNumber);
        hud?.ShowTemporaryMessage(message, color ?? Color.white);
    }

    public void ShowPlayerMessage(int playerNumber, string message, Color color)
    {
        PlayerHUD hud = GetPlayerHUD(playerNumber);
        hud?.ShowTemporaryMessage(message, color);
    }

    // --- Interactable prompts ---
    public void ShowInteractPrompt(string text)
    {
        if (interactPromptUI != null && interactPromptText != null)
        {
            interactPromptText.text = text;
            interactPromptUI.SetActive(true);
        }
    }

    public void HideInteractPrompt()
    {
        if (interactPromptUI != null)
        {
            interactPromptUI.SetActive(false);
        }
    }

    // --- General announcement (optional central notifications) ---
    public void ShowAnnouncement(string message, Color color)
    {
        // Logs to console for debugging
        Debug.Log($"Announcement: {message}");

        // Show on both player HUDs temporarily
        player1HUD?.ShowTemporaryMessage(message, color);
        player2HUD?.ShowTemporaryMessage(message, color);
    }

    // --- Helper ---
    private PlayerHUD GetPlayerHUD(int playerNumber)
    {
        if (playerNumber == 1) return player1HUD;
        if (playerNumber == 2) return player2HUD;
        return null;
    }
}