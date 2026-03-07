using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Text voteText;

    private Dictionary<GroupType, int> groupVotes = new Dictionary<GroupType, int>();

    void Awake()
    {
        Instance = this;

        foreach (GroupType type in System.Enum.GetValues(typeof(GroupType)))
        {
            groupVotes[type] = 0;
        }

        UpdateUI();
    }

    public void AddVotes(GroupType type, int amount)
    {
        groupVotes[type] += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        int total = 0;

        foreach (var votes in groupVotes.Values)
            total += votes;

        voteText.text = "Votes: " + total.ToString();
    }
}