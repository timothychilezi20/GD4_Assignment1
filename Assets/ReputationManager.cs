using UnityEngine;
using System.Collections.Generic;

public class ReputationManager : MonoBehaviour
{
    public static ReputationManager Instance { get; private set; }

    [Header("Reputation Settings")]
    public float minReputation = 0.3f;
    public float maxReputation = 1.5f;
    [SerializeField] private float defaultReputation = 1.0f;

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
    [SerializeField] private float interactionCooldownMin = 0.5f;
    [SerializeField] private float interactionCooldownMax = 2.0f;

    public System.Action<int, NPCMovement.NPCGroup, float> OnReputationChanged;

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

    public void ModifyReputation(int playerNumber, NPCMovement.NPCGroup group, float delta)
    {
        float newValue = GetReputation(playerNumber, group) + delta;
        newValue = Mathf.Clamp(newValue, minReputation, maxReputation);

        SetReputation(playerNumber, group, newValue);

        Debug.Log($"Player {playerNumber} reputation with {group} changed by {delta:F2} → {newValue:F2}");
        OnReputationChanged?.Invoke(playerNumber, group, newValue);
    }

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

    public float GetVoteMultiplier(int playerNumber, NPCMovement.NPCGroup group)
    {
        float rep = GetReputation(playerNumber, group);
        return Mathf.Lerp(voteMultiplierMin, voteMultiplierMax,
                         Mathf.InverseLerp(minReputation, maxReputation, rep));
    }

    public float GetInteractionCooldown(int playerNumber, NPCMovement.NPCGroup group)
    {
        float rep = GetReputation(playerNumber, group);
        return Mathf.Lerp(interactionCooldownMax, interactionCooldownMin,
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

    public Color GetReputationColor(float rep)
    {
        if (rep >= 1.2f) return Color.green;
        if (rep >= 0.8f) return Color.white;
        return Color.red;
    }
}