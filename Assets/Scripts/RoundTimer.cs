using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TextMeshPro = TMPro.TextMeshProUGUI;  

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

    private float timeRemaining;
    private float totalRoundTime;

    void Start()
    {
        if (roundManager == null)
        {
            roundManager = FindFirstObjectByType<RoundManager>();
        }

        currentRound = roundManager != null ? roundManager.currentRound : 1;

        totalRoundTime = GetRoundDuration(); 

        timeRemaining = totalRoundTime;

        UpdateUI(); 
    }

    // Update is called once per frame
    void Update()
    {
        timeRemaining -= Time.deltaTime;
        UpdateUI();

        if (timeRemaining <= 0)
        {
            AdvanceToNextRound(); 
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

    void AdvanceToNextRound()
    {
        currentRound++;
       
        if (currentRound > roundDurations.Length)
        {
            GameComplete();
            return; 
        }

        if (roundManager != null)
        {
            roundManager.currentRound = currentRound;

            Debug.Log($"Advancing to Round {currentRound}");
        }

        totalRoundTime = GetRoundDuration();

        timeRemaining = totalRoundTime; 

        Debug.Log($"Round {currentRound} started with duration {totalRoundTime} seconds.");
    }

    void GameComplete()
    {
        Debug.Log("Game Complete! All rounds finished.");
        if (timerText != null)
        {
            timerText.text = "GAME OVER"; 
        }

        enabled = false;
    }

    public void AddTime(float seconds)
    {
        timeRemaining += seconds;
    }

    public int GetCurrentRound() => currentRound;
    public float GetTimeRemaining() => Mathf.Max(0, timeRemaining); 
}
