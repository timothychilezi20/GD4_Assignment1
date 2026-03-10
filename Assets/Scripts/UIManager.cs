using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [System.Serializable]
    public class PlayerInteractPrompt
    {
        public GameObject promptUI;
        public TextMeshProUGUI promptText;
    }

    [Header("Player HUDs")]
    public List<PlayerHUD> playerHUDs = new List<PlayerHUD>();

    [Header("Player Interact Prompts")]
    public PlayerInteractPrompt player1Prompt;
    public PlayerInteractPrompt player2Prompt;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        HideInteractPrompt(1);
        HideInteractPrompt(2);
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
            Debug.LogWarning($"No HUD found for player {playerNumber}");
        }
    }

    public void ShowStealResult(int votesLost, int playerNumber)
    {
        ShowPlayerMessage(playerNumber, $"-{votesLost} Votes!", Color.red);
    }

    public void ShowTradeResult(string message, int playerNumber, Color? color = null)
    {
        ShowPlayerMessage(playerNumber, message, color ?? Color.white);
    }

    // --- Interact prompts ---
    public void ShowInteractPrompt(int playerNumber, string text)
    {
        PlayerInteractPrompt prompt = GetPlayerPrompt(playerNumber);

        if (prompt != null && prompt.promptUI != null && prompt.promptText != null)
        {
            prompt.promptText.text = text;
            prompt.promptUI.SetActive(true);
        }
    }

    public void HideInteractPrompt(int playerNumber)
    {
        PlayerInteractPrompt prompt = GetPlayerPrompt(playerNumber);

        if (prompt != null && prompt.promptUI != null)
        {
            prompt.promptUI.SetActive(false);
        }
    }

    // --- Helpers ---
    private PlayerHUD GetPlayerHUD(int playerNumber)
    {
        int index = playerNumber - 1;

        if (index >= 0 && index < playerHUDs.Count)
            return playerHUDs[index];

        return null;
    }

    private PlayerInteractPrompt GetPlayerPrompt(int playerNumber)
    {
        switch (playerNumber)
        {
            case 1:
                return player1Prompt;

            case 2:
                return player2Prompt;

            default:
                return null;
        }
    }

    public void ShowAnnouncement(string message, Color color)
    {
        Debug.Log($"Announcement: {message}");

        foreach (var hud in playerHUDs)
        {
            if (hud != null)
            {
                hud.ShowTemporaryMessage(message, color);
            }
        }
    }

}