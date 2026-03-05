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

    [Header("Reputation Values")]
    [SerializeField] private float teacherRepP1 = 1.0f;
    [SerializeField] private float athleteRepP1 = 1.0f;
    [SerializeField] private float artistRepP1 = 1.0f;
    [SerializeField] private float nerdRepP1 = 1.0f;
    [SerializeField] private float grade8RepP1 = 1.0f;

    [SerializeField] private float teacherRepP2 = 1.0f;
    [SerializeField] private float athleteRepP2 = 1.0f;
    [SerializeField] private float artistRepP2 = 1.0f;
    [SerializeField] private float nerdRepP2 = 1.0f;
    [SerializeField] private float grade8RepP2 = 1.0f;

    [Header("Reputation Effects")]
    [SerializeField] private float voteMultiplierMin = 0.5f;
    [SerializeField] private float voteMultiplierMax = 2.0f;

    // Events for UI updates
    public System.Action<int, NPCMovement.NPCGroup, float> OnReputationChanged; // playerNumber, group, newValue
    public System.Action<int, NPCMovement.NPCGroup, float> OnReputationChangedWithChange; // playerNumber, group, changeAmount

    private Dictionary<(int, NPCMovement.NPCGroup), float> previousReputations = new Dictionary<(int, NPCMovement.NPCGroup), float>();

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
        }
    }

    void Start()
    {
        // Initialize previous values
        SaveAllCurrentReputations();
    }

    void SaveAllCurrentReputations()
    {
        foreach (NPCMovement.NPCGroup group in System.Enum.GetValues(typeof(NPCMovement.NPCGroup)))
        {
            previousReputations[(1, group)] = GetReputation(1, group);
            previousReputations[(2, group)] = GetReputation(2, group);
        }
    }

    // Modify reputation for a player with a specific NPC group
    public void ModifyReputation(int playerNumber, NPCMovement.NPCGroup group, float delta)
    {
        Debug.Log($"ReputationManager.ModifyReputation - Player: {playerNumber}, Group: {group}, Delta: {delta}");

        float currentValue = GetReputation(playerNumber, group);
        float newValue = currentValue + delta;
        newValue = Mathf.Clamp(newValue, minReputation, maxReputation);

        float actualChange = newValue - currentValue;

        SetReputation(playerNumber, group, newValue);

        // Fire events
        OnReputationChanged?.Invoke(playerNumber, group, newValue);
        OnReputationChangedWithChange?.Invoke(playerNumber, group, actualChange);

        // Show popup if enabled
        if (showReputationPopups && Mathf.Abs(actualChange) >= minChangeToShowPopup)
        {
            ShowReputationPopup(playerNumber, group, actualChange);
        }
    }

    // Set reputation directly
    public void SetReputation(int playerNumber, NPCMovement.NPCGroup group, float value)
    {
        value = Mathf.Clamp(value, minReputation, maxReputation);

        switch (group)
        {
            case NPCMovement.NPCGroup.Teacher:
                if (playerNumber == 1) teacherRepP1 = value;
                else teacherRepP2 = value;
                break;
            case NPCMovement.NPCGroup.Athlete:
                if (playerNumber == 1) athleteRepP1 = value;
                else athleteRepP2 = value;
                break;
            case NPCMovement.NPCGroup.Artist:
                if (playerNumber == 1) artistRepP1 = value;
                else artistRepP2 = value;
                break;
            case NPCMovement.NPCGroup.Nerd:
                if (playerNumber == 1) nerdRepP1 = value;
                else nerdRepP2 = value;
                break;
            case NPCMovement.NPCGroup.Grade8:
                if (playerNumber == 1) grade8RepP1 = value;
                else grade8RepP2 = value;
                break;
        }

        OnReputationChanged?.Invoke(playerNumber, group, value);
    }

    // Get reputation for a player with a specific NPC group
    public float GetReputation(int playerNumber, NPCMovement.NPCGroup group)
    {
        return (playerNumber, group) switch
        {
            (1, NPCMovement.NPCGroup.Teacher) => teacherRepP1,
            (1, NPCMovement.NPCGroup.Athlete) => athleteRepP1,
            (1, NPCMovement.NPCGroup.Artist) => artistRepP1,
            (1, NPCMovement.NPCGroup.Nerd) => nerdRepP1,
            (1, NPCMovement.NPCGroup.Grade8) => grade8RepP1,
            (2, NPCMovement.NPCGroup.Teacher) => teacherRepP2,
            (2, NPCMovement.NPCGroup.Athlete) => athleteRepP2,
            (2, NPCMovement.NPCGroup.Artist) => artistRepP2,
            (2, NPCMovement.NPCGroup.Nerd) => nerdRepP2,
            (2, NPCMovement.NPCGroup.Grade8) => grade8RepP2,
            _ => defaultReputation
        };
    }

    // Get the change amount for a player/group since last check
    public float GetReputationChange(int playerNumber, NPCMovement.NPCGroup group)
    {
        var key = (playerNumber, group);
        if (previousReputations.ContainsKey(key))
        {
            float current = GetReputation(playerNumber, group);
            float previous = previousReputations[key];
            return current - previous;
        }
        return 0f;
    }

    // Method to show popup
    private void ShowReputationPopup(int playerNumber, NPCMovement.NPCGroup group, float changeAmount)
    {
        Debug.Log($"=== SHOW POPUP ===");
        Debug.Log($"10. ShowReputationPopup called for Player {playerNumber}, Group {group}, Change {changeAmount}");

        // Find the player GameObject
        GameObject player = GameObject.FindGameObjectWithTag(playerNumber == 1 ? "Player1" : "Player2");
        Debug.Log($"11. Player GameObject found: {player != null}");
        if (player != null)
        {
            Debug.Log($"11a. Player position: {player.transform.position}");
            Debug.Log($"11b. Player tag: {player.tag}");
        }

        if (player == null) return;

        // Get popup manager
        if (ReputationPopupManager.Instance != null)
        {
            Debug.Log($"12. ReputationPopupManager.Instance FOUND");
            Debug.Log($"12a. ReputationPopupManager.Instance.GetInstanceID(): {ReputationPopupManager.Instance.GetInstanceID()}");

            Vector3 spawnPos = player.transform.position + Vector3.up * 2f;
            Debug.Log($"13. Calling ShowPopup with position: {spawnPos}");

            ReputationPopupManager.Instance.ShowPopup(
                playerNumber,
                group,
                changeAmount,
                spawnPos
            );
        }
        else
        {
            Debug.LogError($"12. ReputationPopupManager.Instance is NULL!");

            // Try to find it
            ReputationPopupManager found = FindFirstObjectByType<ReputationPopupManager>();
            Debug.Log($"12b. Found by Find: {found != null}");
        }
    }
    // Get vote multiplier based on reputation
    public float GetVoteMultiplier(int playerNumber, NPCMovement.NPCGroup group)
    {
        float rep = GetReputation(playerNumber, group);
        return Mathf.Lerp(voteMultiplierMin, voteMultiplierMax,
                         Mathf.InverseLerp(minReputation, maxReputation, rep));
    }

    // Get reputation level description
    public string GetReputationLevel(float rep)
    {
        if (rep >= 1.3f) return "Beloved";
        if (rep >= 1.1f) return "Liked";
        if (rep >= 0.9f) return "Neutral";
        if (rep >= 0.6f) return "Disliked";
        return "Hated";
    }

    // Get color for reputation display
    public Color GetReputationColor(float rep)
    {
        if (rep >= 1.2f) return Color.green;
        if (rep >= 0.8f) return Color.white;
        return Color.red;
    }
}