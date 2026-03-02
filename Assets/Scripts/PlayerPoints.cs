using UnityEngine;
using System.Collections;

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

    private PlayerUI playerUI; 
    private PlayerController playerController;

    public System.Action<int, int> OnTotalPointsChanged;
    public System.Action<int, int> OnRoundPointsChanged;
    public System.Action<int, int> OnHeldVotesChanged;

    private void Awake()
    {
       playerController = GetComponent<PlayerController>();
    }

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
                Debug.Log($"Player {playerNumber} found its UI");
                break;
            }
        }

        if (playerUI == null)
        {
            Debug.LogWarning($"No UI found for player {playerNumber}!");
        }
    }

    public void DumpVotes (int votesToDump, float zoneMultiplier)
    {
        if (votesToDump <= 0) 
            return;

        int pointsEarned = Mathf.RoundToInt(votesToDump * zoneMultiplier * dumpMultiplier);

        roundPoints += pointsEarned;
        OnRoundPointsChanged?.Invoke(playerNumber, roundPoints);

        heldVotes -= votesToDump;
        OnHeldVotesChanged?.Invoke(playerNumber, heldVotes);

        Debug.Log($"Player {playerNumber} dumped {votesToDump} votes for {pointsEarned} points!");
    }

    public void EndRound()
    {
        int roundTotal = Mathf.RoundToInt(roundPoints * roundBonusMultiplier);

        totalPoints += roundTotal;
        OnTotalPointsChanged?.Invoke(playerNumber, roundTotal);

        roundPoints = 0;
        OnRoundPointsChanged?.Invoke(playerNumber, roundPoints);

        Debug.Log($"Player {playerNumber} round ended. Total points: {totalPoints}");
    }

    public void AddHeldVotes(int amount)
    {
        heldVotes += amount;
        OnHeldVotesChanged?.Invoke(playerNumber, heldVotes);
    }

    public void RemoveHeldVotes(int amount)
    {
        heldVotes = Mathf.Max(0, heldVotes - amount);
        OnHeldVotesChanged?.Invoke(playerNumber, heldVotes);
    }

    public void SetDumpMultiplier(float multiplier, float duration)
    {
        StartCoroutine(DumpMultiplierRoutine(multiplier, duration));
    }

    IEnumerator DumpMultiplierRoutine(float multiplier, float duration)
    {
        dumpMultiplier = multiplier;
        Debug.Log($"Player {playerNumber} dump multiplier: {multiplier}x for {duration}s");

        yield return new WaitForSeconds(duration);

        dumpMultiplier = 1f;
        Debug.Log($"Player {playerNumber} dump multiplier returned to normal");
    }

    public void SetRoundBonusMultiplier(float multiplier)
    {
        roundBonusMultiplier = multiplier;
    }

    public void AddTotalPoints(int points)
    {
        totalPoints += points;
        OnTotalPointsChanged?.Invoke(playerNumber, totalPoints);
        Debug.Log($"Player {playerNumber} gained {points} total points! New total: {totalPoints}");
    }

    public void AddRoundPoints(int points)
    {
        roundPoints += points;
        OnRoundPointsChanged?.Invoke(playerNumber, roundPoints);
        Debug.Log($"Player {playerNumber} gained {points} round points! New round: {roundPoints}");
    }

    public void AddPointsWithMultiplier(int points, float multiplier)
    {
        int finalPoints = Mathf.RoundToInt(points * multiplier);
        totalPoints += finalPoints;
        OnTotalPointsChanged?.Invoke(playerNumber, totalPoints);
        Debug.Log($"Player {playerNumber} gained {points} x {multiplier} = {finalPoints} total points!");
    }

    public void AddRoundPointsWithMultiplier(int points, float multiplier)
    {
        int finalPoints = Mathf.RoundToInt(points * multiplier);
        roundPoints += finalPoints;
        OnRoundPointsChanged?.Invoke(playerNumber, roundPoints);
        Debug.Log($"Player {playerNumber} gained {points} x {multiplier} = {finalPoints} round points!");
    }

    public void SubtractPoints(int points)
    {
        totalPoints = Mathf.Max(0, totalPoints - points);
        OnTotalPointsChanged?.Invoke(playerNumber, totalPoints);
        Debug.Log($"Player {playerNumber} lost {points} points! New total: {totalPoints}");
    }

    public void ResetAllPoints()
    {
        totalPoints = 0;
        roundPoints = 0;
        heldVotes = 0;

        OnTotalPointsChanged?.Invoke(playerNumber, totalPoints);
        OnRoundPointsChanged?.Invoke(playerNumber, roundPoints);
        OnHeldVotesChanged?.Invoke(playerNumber, heldVotes);

        Debug.Log($"Player {playerNumber} points reset");
    }

    public bool SpendVotes(int amount)
    {
        if (heldVotes >= amount)
        {
            heldVotes -= amount;
            OnHeldVotesChanged?.Invoke(playerNumber, heldVotes);
            Debug.Log($"Player {playerNumber} spent {amount} votes");
        }

        return false; 
    }

    // Getters
    public int GetPlayerNumber() => playerNumber;
    public int GetTotalPoints() => totalPoints;
    public int GetRoundPoints() => roundPoints;
    public int GetHeldVotes() => heldVotes;
}

