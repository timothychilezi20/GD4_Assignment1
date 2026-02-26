using UnityEngine;
using System.Collections.Generic;
using Unity.UI;
using TMPro; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Round Settings")]
    [SerializeField] private int currentRound = 1;
    [SerializeField] private int maxRounds = 3;

    [Header("Players")]
    [SerializeField] private PlayerPoints player1Points;
    [SerializeField] private PlayerPoints player2Points;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Events")]
    public System.Action<int> OnRoundStarted;
    public System.Action<int> OnRoundEnded;
    public System.Action<int> OnGameWon; // playerNumber of winner

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void EndRound()
    {
        Debug.Log($"Round {currentRound} ended");

        // Tell players to process round points
        if (player1Points != null)
            player1Points.EndRound();

        if (player2Points != null)
            player2Points.EndRound();

        OnRoundEnded?.Invoke(currentRound);

        currentRound++;

        if (currentRound <= maxRounds)
        {
            OnRoundStarted?.Invoke(currentRound);
            Debug.Log($"Starting Round {currentRound}");
        }
        else
        {
            DetermineWinner();
        }
    }

    void DetermineWinner()
    {
        int p1Total = player1Points != null ? player1Points.GetTotalPoints() : 0;
        int p2Total = player2Points != null ? player2Points.GetTotalPoints() : 0;

        int winner = 0;
        string message = "";

        if (p1Total > p2Total)
        {
            winner = 1;
            message = $"PLAYER 1 WINS!\n{p1Total} - {p2Total}";
        }
        else if (p2Total > p1Total)
        {
            winner = 2;
            message = $"PLAYER 2 WINS!\n{p1Total} - {p2Total}";
        }
        else
        {
            message = $"TIE!\n{p1Total} - {p2Total}";
        }

        Debug.Log($"Game Over: {message}");

        OnGameWon?.Invoke(winner);

        // Show UI
        if (gameOverPanel != null && winnerText != null)
        {
            gameOverPanel.SetActive(true);
            winnerText.text = message;
        }
    }

    public void AddPoints(int playerNumber, int points)
    {
        if (playerNumber == 1 && player1Points != null)
        {
            player1Points.AddTotalPoints(points);
        }
        else if (playerNumber == 2 && player2Points != null)
        {
            player2Points.AddTotalPoints(points);
        }
        else
        {
            Debug.LogWarning($"Can't add points to Player {playerNumber} - PlayerPoints not found!");
        }
    }

    public int GetPlayerTotalPoints(int playerNumber)
    {
        if (playerNumber == 1 && player1Points != null)
        {
            return player1Points.GetTotalPoints();
        }
     
        else if (playerNumber == 2 && player2Points != null)
        {
            return player2Points.GetTotalPoints();
        }

        return 0;
    }

    public int GetPlayerRoundPoints(int playerNumber)
    {
        if (playerNumber == 1 && player1Points != null)
        {
            return player1Points.GetRoundPoints();
        }

        else if (playerNumber == 2 && player2Points != null)
        {
            return player2Points.GetRoundPoints();
        }

        return 0;
    }

    public void AddRoundPoints(int playerNumber, int points)
    {
        if (playerNumber == 1 && player1Points != null)
        {
            player1Points.AddRoundPoints(points);
        }
        else if (playerNumber == 2 && player2Points != null)
        {
            player2Points.AddRoundPoints(points);
        }
    }

    public bool IsPlayerWinning(int playerNumber)
    {
        int p1Total = player1Points != null ? player1Points.GetTotalPoints() : 0;
        int p2Total = player2Points != null ? player2Points.GetTotalPoints() : 0;

        if (playerNumber == 1)
            return p1Total > p2Total;
        else if (playerNumber == 2)
            return p2Total > p1Total;

        return false;
    }

    // Get winning player number (0 for tie)
    public int GetWinningPlayer()
    {
        int p1Total = player1Points != null ? player1Points.GetTotalPoints() : 0;
        int p2Total = player2Points != null ? player2Points.GetTotalPoints() : 0;

        if (p1Total > p2Total) return 1;
        if (p2Total > p1Total) return 2;
        return 0; // Tie
    }
}