using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Player HUDs")]
    public List<PlayerHUD> playerHUDs = new List<PlayerHUD>(); // Automatically handles any number of players

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
        GetPlayerHUD(playerNumber)?.UpdateVotes(votes);
    }

    public void UpdatePlayerBallots(int playerNumber, int ballots)
    {
        GetPlayerHUD(playerNumber)?.UpdateBallots(ballots);
    }

    public void UpdatePlayerItem(int playerNumber, ItemType item)
    {
        GetPlayerHUD(playerNumber)?.UpdateItem(item);
    }

    // --- Temporary popups / announcements ---
    public void ShowPlayerMessage(int playerNumber, string message, Color color)
    {
        var hud = GetPlayerHUD(playerNumber);
        if (hud != null)
        {
            hud.ShowTemporaryMessage(message, color);
        }
        else
        {
            Debug.LogWarning($"No HUD found for player {playerNumber}, sending to all players instead.");
            ShowAnnouncement(message, color);
        }
    }

    // --- Steal / trade results (auto send to correct HUD if possible) ---
    public void ShowStealResult(int votesLost, int playerNumber)
    {
        ShowPlayerMessage(playerNumber, $"-{votesLost} Votes!", Color.red);
    }

    public void ShowTradeResult(string message, int playerNumber, Color? color = null)
    {
        ShowPlayerMessage(playerNumber, message, color ?? Color.white);
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
        Debug.Log($"Announcement: {message}");
        foreach (var hud in playerHUDs)
        {
            hud?.ShowTemporaryMessage(message, color);
        }
    }

    // --- Helper ---
    private PlayerHUD GetPlayerHUD(int playerNumber)
    {
        // playerNumber assumed to start at 1
        int index = playerNumber - 1;
        if (index >= 0 && index < playerHUDs.Count)
            return playerHUDs[index];
        return null;
    }
}