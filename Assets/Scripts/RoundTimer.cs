using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TextMeshPro = TMPro.TextMeshProUGUI;
using UnityEngine.SceneManagement;

public class RoundTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float[] roundDurations = { 180f, 180f, 240f };
    [SerializeField] private int currentRound = 1;

    [Header("UI Elements")]
    [SerializeField] private TextMeshPro timerText;
    [SerializeField] private TextMeshPro roundText;
    [SerializeField] private Slider timerSlider;

    [Header("References")]
    [SerializeField] private RoundManager roundManager;
    //[SerializeField] private NPCSpawner npcSpawner;
    [SerializeField] private EndGameUI endGameUI;

    private float timeRemaining;
    private float totalRoundTime;
    private bool gameEnded = false;

    void Start()
    {
        if (roundManager == null)
        {
            roundManager = FindFirstObjectByType<RoundManager>();
        }

        if (endGameUI == null)
        {
            endGameUI = FindFirstObjectByType<EndGameUI>();
        }


        currentRound = roundManager != null ? roundManager.currentRound : 1;

        totalRoundTime = GetRoundDuration();

        timeRemaining = totalRoundTime;

        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameEnded) return;

        timeRemaining -= Time.deltaTime;
        UpdateUI();

        if (timeRemaining <= 0)
        {
            GameComplete(); // Changed from AdvanceToNextRound to GameComplete
        }
    }

    float GetRoundDuration()
    {
        {
            int index = Mathf.Clamp(currentRound - 1, 0, roundDurations.Length - 1);
            return roundDurations[index];
        }
    }


    void UpdateUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(Mathf.Max(0, timeRemaining) / 60);
            int seconds = Mathf.FloorToInt(Mathf.Max(0, timeRemaining) % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        if (timerSlider != null)
        {
            timerSlider.value = timeRemaining / totalRoundTime;
        }

        if (roundText != null)
        {
            roundText.text = $"Round {currentRound}";
        }
    }

    // Removed AdvanceToNextRound method entirely

    void GameComplete()
    {
        if (gameEnded) return;

        gameEnded = true;
        Debug.Log("Game Complete! Round finished.");

        if (timerText != null)
        {
            timerText.text = "GAME OVER";
        }

        // Show end game UI
        if (endGameUI != null)
        {
            endGameUI.ShowEndGameScreen();
        }
        else
        {
            // Fallback: just load main menu directly
            Debug.LogWarning("EndGameUI not found! Loading main menu directly.");
            Time.timeScale = 0f;
            StartCoroutine(LoadMainMenuAfterDelay());
        }

        enabled = false;
    }


    IEnumerator LoadMainMenuAfterDelay()
    {
        yield return new WaitForSecondsRealtime(2f);
        SceneManager.LoadScene("StartScene"); // Make sure this matches your scene name
    }

    public void AddTime(float seconds)
    {
        timeRemaining += seconds;
    }

    public int GetCurrentRound() => currentRound;
    public float GetTimeRemaining() => Mathf.Max(0, timeRemaining);
}
