using UnityEngine;
using System;

public class PlayerVotes : MonoBehaviour
{
    [Header("Player Settings")]
    public int PlayerNumber = 1;

    [Header("Vote Settings")]
    public int heldVotes = 0;
    public GameObject votePickupPrefab; // prefab to spawn when dropping votes

    // Event for HUD updates
    public Action<int, int> OnVotesChanged; // PlayerNumber, heldVotes

    /// <summary>
    /// Adds votes to this player and updates UI.
    /// </summary>
    public void AddVotes(int amount)
    {
        if (amount <= 0) return;

        heldVotes += amount;
        heldVotes = Mathf.Max(heldVotes, 0); // Clamp to 0 minimum

        // Update HUD
        UIManager.Instance?.UpdatePlayerVotes(PlayerNumber, heldVotes);

        // Temporary message for this player only
        UIManager.Instance?.ShowTradeResult($"+{amount} Votes!", PlayerNumber, Color.green);

        // Invoke event for other systems (like PlayerPoints)
        OnVotesChanged?.Invoke(PlayerNumber, heldVotes);
    }

    /// <summary>
    /// Drops votes at the specified position, spawns pickups, and updates UI.
    /// </summary>
    public void DropVotes(Vector3 position, int amount)
    {
        if (amount <= 0) return;

        int dropAmount = Mathf.Min(amount, heldVotes);
        heldVotes -= dropAmount;
        heldVotes = Mathf.Max(heldVotes, 0);

        // Update HUD
        UIManager.Instance?.UpdatePlayerVotes(PlayerNumber, heldVotes);

        // Temporary message for this player only
        UIManager.Instance?.ShowTradeResult($"-{dropAmount} Votes!", PlayerNumber, Color.red);

        // Spawn vote pickup prefabs in world
        if (votePickupPrefab != null)
        {
            for (int i = 0; i < dropAmount; i++)
            {
                Vector3 spawnPos = position + UnityEngine.Random.insideUnitSphere * 0.5f + Vector3.up * 0.5f;
                Instantiate(votePickupPrefab, spawnPos, Quaternion.identity);
            }
        }

        // Invoke event
        OnVotesChanged?.Invoke(PlayerNumber, heldVotes);
    }

    /// <summary>
    /// Clears all held votes and updates UI.
    /// </summary>
    public void ClearVotes()
    {
        heldVotes = 0;

        UIManager.Instance?.UpdatePlayerVotes(PlayerNumber, heldVotes);
        OnVotesChanged?.Invoke(PlayerNumber, heldVotes);
    }

    /// <summary>
    /// Returns the current number of votes held by this player.
    /// </summary>
    public int GetVotes()
    {
        return heldVotes;
    }
}