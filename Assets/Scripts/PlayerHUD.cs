using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerHUD : MonoBehaviour
{
    [Header("HUD Text")]
    public TextMeshProUGUI votesText;
    public TextMeshProUGUI ballotsText;
    public TextMeshProUGUI itemText;

    [Header("Temporary Message")]
    public TextMeshProUGUI tempMessageText;

    int currentVotes;
    int currentBallots;

    public void UpdateVotes(int votes)
    {
        currentVotes = votes;

        if (votesText != null)
            votesText.text = $"Votes: {votes}";
    }

    public void UpdateBallots(int ballots)
    {
        currentBallots = ballots;

        if (ballotsText != null)
            ballotsText.text = $"Ballots: {ballots}";
    }

    public void UpdateItem(ItemType item)
    {
        if (itemText != null)
        {
            if (item == ItemType.None)
                itemText.text = "Item: None";
            else
                itemText.text = $"Item: {item}";
        }
    }

    public void ShowTemporaryMessage(string message, Color color, float duration = 1.5f)
    {
        if (tempMessageText == null) return;

        tempMessageText.text = message;
        tempMessageText.color = color;
        tempMessageText.gameObject.SetActive(true);

        StartCoroutine(HideMessage(duration));
    }

    IEnumerator HideMessage(float duration)
    {
        yield return new WaitForSeconds(duration);
        tempMessageText.gameObject.SetActive(false);
    }
}