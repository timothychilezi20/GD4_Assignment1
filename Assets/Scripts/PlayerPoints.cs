using UnityEngine;
using System;

public class PlayerPoints : MonoBehaviour
{
    [Header("Player Identification")]
    [SerializeField] private int playerNumber = 1;

    [Header("Points")]
    [SerializeField] private int totalPoints = 0;
    [SerializeField] private int roundPoints = 0;
    [SerializeField] private int heldVotes = 0;

    [Header("Multipliers")]
    [SerializeField] private float dumpMultiplier = 1f;
    [SerializeField] private float roundBonusMultiplier = 1f;

    public event Action<int, int> OnTotalPointsChanged;
    public event Action<int, int> OnRoundPointsChanged;
    public event Action<int, int> OnHeldVotesChanged;

    private PlayerUI playerUI;

    void Start()
    {
        FindPlayerUI();
    }

    void FindPlayerUI()
    {
        PlayerUI[] allUI = FindObjectsByType<PlayerUI>(FindObjectsSortMode.None);
        foreach (PlayerUI ui in allUI)
        {
            if (ui.playerNumber == playerNumber)
            {
                playerUI = ui;
                playerUI.RegisterPlayer(this);
                break;
            }
        }
    }

    // ========== VOTES ==========

    public void AddHeldVotes(int amount)
    {
        if (amount <= 0) return;

        heldVotes += amount;
        OnHeldVotesChanged?.Invoke(playerNumber, heldVotes);

        // Spawn notification
        if (NotificationManager.Instance != null)
            NotificationManager.Instance.SpawnNotification($"+{amount} Votes!", Color.green, transform.position);
    }

    public void RemoveHeldVotes(int amount)
    {
        if (amount <= 0) return;

        int actualRemoved = Mathf.Min(amount, heldVotes);
        heldVotes = Mathf.Max(0, heldVotes - amount);
        OnHeldVotesChanged?.Invoke(playerNumber, heldVotes);

        if (actualRemoved > 0 && NotificationManager.Instance != null)
            NotificationManager.Instance.SpawnNotification($"-{actualRemoved} Votes!", Color.red, transform.position);
    }

    public void ClearHeldVotes()
    {
        if (heldVotes <= 0) return;

        int lostVotes = heldVotes;
        heldVotes = 0;
        OnHeldVotesChanged?.Invoke(playerNumber, heldVotes);

        if (NotificationManager.Instance != null)
            NotificationManager.Instance.SpawnNotification($"Lost {lostVotes} Votes!", Color.red, transform.position);
    }

    // ========== POINTS ==========

    public void AddRoundPoints(int points)
    {
        if (points <= 0) return;

        roundPoints += points;
        OnRoundPointsChanged?.Invoke(playerNumber, roundPoints);

        if (NotificationManager.Instance != null)
            NotificationManager.Instance.SpawnNotification($"+{points} Round Points!", Color.yellow, transform.position);
    }

    public void AddTotalPoints(int points)
    {
        if (points <= 0) return;

        totalPoints += points;
        OnTotalPointsChanged?.Invoke(playerNumber, totalPoints);

        if (NotificationManager.Instance != null)
            NotificationManager.Instance.SpawnNotification($"+{points} Total Points!", Color.cyan, transform.position);
    }

    public void EndRound()
    {
        int roundTotal = Mathf.RoundToInt(roundPoints * roundBonusMultiplier);
        totalPoints += roundTotal;
        OnTotalPointsChanged?.Invoke(playerNumber, totalPoints);

        if (NotificationManager.Instance != null && roundTotal > 0)
            NotificationManager.Instance.SpawnNotification($"+{roundTotal} Round Bonus!", Color.magenta, transform.position);

        roundPoints = 0;
        OnRoundPointsChanged?.Invoke(playerNumber, roundPoints);
    }

    // ========== GETTERS ==========

    public int GetPlayerNumber() => playerNumber;
    public int GetTotalPoints() => totalPoints;
    public int GetRoundPoints() => roundPoints;
    public int GetHeldVotes() => heldVotes;
}