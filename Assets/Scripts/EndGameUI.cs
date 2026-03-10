using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndGameUI : MonoBehaviour
{
    [Header("End Game Panel")]
    [SerializeField] private GameObject endGamePanel;

    [Header("Winner Elements")]
    [SerializeField] private Text winnerText;
    [SerializeField] private Text winnerBallotsText;
    [SerializeField] private Text winnerItemsText;

    [Header("Loser Elements")]
    [SerializeField] private Text loserText;
    [SerializeField] private Text loserBallotsText;
    [SerializeField] private Text loserItemsText;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private string mainMenuSceneName = "StartScene";

    private PlayerController player1;
    private PlayerController player2;
    private bool isGameEnding = false;

    void Start()
    {
        if (endGamePanel != null)
            endGamePanel.SetActive(false);
    }

    public void ShowEndGameScreen()
    {
        if (isGameEnding) return;
        isGameEnding = true;

        // Find both players
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (PlayerController player in players)
        {
            if (player.GetPlayerNumber() == 1)
                player1 = player;
            else if (player.GetPlayerNumber() == 2)
                player2 = player;
        }

        // Determine winner based on ballot count
        int player1Ballots = player1 != null ? player1.GetBallotCount() : 0;
        int player2Ballots = player2 != null ? player2.GetBallotCount() : 0;

        PlayerController winner = player1Ballots > player2Ballots ? player1 : player2;
        PlayerController loser = winner == player1 ? player2 : player1;

        // Get the winning player number
        int winnerNumber = winner != null ? winner.GetPlayerNumber() : 1;
        int loserNumber = loser != null ? loser.GetPlayerNumber() : 2;

        // Get ballot counts
        int winnerBallots = winner != null ? winner.GetBallotCount() : 0;
        int loserBallots = loser != null ? loser.GetBallotCount() : 0;

        // Get items collected counts
        int winnerItems = winner != null ? winner.GetItemsCollected() : 0;
        int loserItems = loser != null ? loser.GetItemsCollected() : 0;

        // Update UI elements
        if (winnerText != null)
            winnerText.text = $"PLAYER {winnerNumber} WINS!!";

        if (loserText != null)
            loserText.text = $"PLAYER {loserNumber} LOSES!!";

        if (winnerBallotsText != null)
            winnerBallotsText.text = $"BALLOTS COLLECTED: {winnerBallots}";

        if (loserBallotsText != null)
            loserBallotsText.text = $"BALLOTS COLLECTED: {loserBallots}";

        if (winnerItemsText != null)
            winnerItemsText.text = $"ITEMS COLLECTED: {winnerItems}";

        if (loserItemsText != null)
            loserItemsText.text = $"ITEMS COLLECTED: {loserItems}";

        // Show the panel
        if (endGamePanel != null)
        {
            endGamePanel.SetActive(true);

            // Pause the game
            Time.timeScale = 0f;

            // Start coroutine to load main menu
            StartCoroutine(LoadMainMenuAfterDelay());
        }
    }

    IEnumerator LoadMainMenuAfterDelay()
    {
        yield return new WaitForSecondsRealtime(displayDuration);

        // Load main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
