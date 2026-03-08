using UnityEngine;
using System.Collections.Generic;

public class ReputationManager : MonoBehaviour
{
    public static ReputationManager Instance { get; private set; }

    [Header("Reputation Settings")]
    [SerializeField] public float minReputation = 0.3f;
    [SerializeField] public float maxReputation = 1.5f;
    [SerializeField] private float defaultReputation = 1.0f;

    [Header("Popup Settings")]
    [SerializeField] private bool showReputationPopups = true;
    [SerializeField] private float minChangeToShowPopup = 0.05f; // Only show for changes > 0.05

    [Header("Reputation Effects")]
    [SerializeField] private float voteMultiplierMin = 0.5f;
    [SerializeField] private float voteMultiplierMax = 2.0f;

    // Events for UI updates
    public System.Action<int, NPCMovement.NPCGroup, float> OnReputationChanged; // playerNumber, group, newValue
    public System.Action<int, NPCMovement.NPCGroup, float> OnReputationChangedWithChange; // playerNumber, group, changeAmount

    // Dictionaries to store reputations
    private Dictionary<int, Dictionary<NPCMovement.NPCGroup, float>> reputations =
        new Dictionary<int, Dictionary<NPCMovement.NPCGroup, float>>();

    private Dictionary<int, Dictionary<NPCMovement.NPCGroup, float>> previousReputations =
        new Dictionary<int, Dictionary<NPCMovement.NPCGroup, float>>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("ReputationManager Instance created");
        }
        else
        {
            Debug.Log("ReputationManager Instance already exists, destroying duplicate");
            Destroy(gameObject);
            return;
        }

        // Initialize dictionaries for players 1 and 2
        for (int player = 1; player <= 2; player++)
        {
            reputations[player] = new Dictionary<NPCMovement.NPCGroup, float>();
            previousReputations[player] = new Dictionary<NPCMovement.NPCGroup, float>();

            foreach (NPCMovement.NPCGroup group in System.Enum.GetValues(typeof(NPCMovement.NPCGroup)))
            {
                reputations[player][group] = defaultReputation;
                previousReputations[player][group] = defaultReputation;
            }
        }
    }

    // Modify reputation for a player/group
    public void ModifyReputation(int playerNumber, NPCMovement.NPCGroup group, float delta)
    {
        if (!ValidatePlayerNumber(playerNumber)) return;

        float currentValue = reputations[playerNumber][group];
        float newValue = Mathf.Clamp(currentValue + delta, minReputation, maxReputation);
        float actualChange = newValue - currentValue;

        reputations[playerNumber][group] = newValue;
        previousReputations[playerNumber][group] = newValue;

        // Fire events
        OnReputationChanged?.Invoke(playerNumber, group, newValue);
        OnReputationChangedWithChange?.Invoke(playerNumber, group, actualChange);

        // Show popup if enabled
        if (showReputationPopups && Mathf.Abs(actualChange) >= minChangeToShowPopup)
        {
            ShowReputationPopup(playerNumber, group, actualChange);
        }
    }

    // Directly set reputation
    public void SetReputation(int playerNumber, NPCMovement.NPCGroup group, float value)
    {
        if (!ValidatePlayerNumber(playerNumber)) return;

        value = Mathf.Clamp(value, minReputation, maxReputation);
        reputations[playerNumber][group] = value;
        previousReputations[playerNumber][group] = value;

        OnReputationChanged?.Invoke(playerNumber, group, value);
    }

    // Get current reputation
    public float GetReputation(int playerNumber, NPCMovement.NPCGroup group)
    {
        if (!ValidatePlayerNumber(playerNumber)) return defaultReputation;
        return reputations[playerNumber][group];
    }

    // Get reputation change since last update
    public float GetReputationChange(int playerNumber, NPCMovement.NPCGroup group)
    {
        if (!ValidatePlayerNumber(playerNumber)) return 0f;
        return reputations[playerNumber][group] - previousReputations[playerNumber][group];
    }

    // Show reputation popup
    private void ShowReputationPopup(int playerNumber, NPCMovement.NPCGroup group, float changeAmount)
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerNumber == 1 ? "Player1" : "Player2");
        if (player == null) return;

        if (ReputationPopupManager.Instance != null)
        {
            Vector3 spawnPos = player.transform.position + Vector3.up * 2f;
            ReputationPopupManager.Instance.ShowPopup(playerNumber, group, changeAmount, spawnPos);
        }
        else
        {
            Debug.LogError("ReputationPopupManager.Instance is NULL!");
        }
    }

    // Vote multiplier based on reputation
    public float GetVoteMultiplier(int playerNumber, NPCMovement.NPCGroup group)
    {
        float rep = GetReputation(playerNumber, group);
        return Mathf.Lerp(voteMultiplierMin, voteMultiplierMax,
                          Mathf.InverseLerp(minReputation, maxReputation, rep));
    }

    // Reputation level description
    public string GetReputationLevel(float rep)
    {
        if (rep >= 1.3f) return "Beloved";
        if (rep >= 1.1f) return "Liked";
        if (rep >= 0.9f) return "Neutral";
        if (rep >= 0.6f) return "Disliked";
        return "Hated";
    }

    // Reputation color for UI
    public Color GetReputationColor(float rep)
    {
        if (rep >= 1.2f) return Color.green;
        if (rep >= 0.8f) return Color.white;
        return Color.red;
    }

    // Validate player number
    private bool ValidatePlayerNumber(int playerNumber)
    {
        if (playerNumber < 1 || playerNumber > 2)
        {
            Debug.LogWarning($"Invalid player number: {playerNumber}");
            return false;
        }
        return true;
    }
}