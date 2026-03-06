using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Announcements")]
    [SerializeField] private GameObject announcementPanel;
    [SerializeField] private TextMeshProUGUI announcementText;
    [SerializeField] private float announcementDuration = 2f;

    [Header("Interaction Prompts")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private CanvasGroup promptCanvasGroup;

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
        if (announcementPanel != null)
            announcementPanel.SetActive(false);

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    public void ShowAnnouncement(string message, Color color)
    {
        if (announcementPanel == null || announcementText == null) return;

        StopAllCoroutines();
        StartCoroutine(DisplayAnnouncement(message, color));
    }

    IEnumerator DisplayAnnouncement(string message, Color color)
    {
        announcementPanel.SetActive(true);
        announcementText.text = message;
        announcementText.color = color;

        yield return new WaitForSeconds(announcementDuration);

        announcementPanel.SetActive(false);
    }

    public void ShowTradeResult(int votes, string groupName)
    {
        ShowAnnouncement($"+{votes} VOTES from {groupName}!", Color.green);
    }

    public void ShowStealResult(int votes, int targetPlayer)
    {
        ShowAnnouncement($"STOLE {votes} VOTES from Player {targetPlayer}!", Color.yellow);
    }

    public void ShowInteractionPrompt(string message, Color color)
    {
        if (interactionPrompt == null || interactionText == null) return;

        interactionPrompt.SetActive(true);
        interactionText.text = message;
        interactionText.color = color;

        if (promptCanvasGroup != null)
            promptCanvasGroup.alpha = 1f;
    }

    public void HideInteractionPrompt()
    {
        if (interactionPrompt == null) return;

        if (promptCanvasGroup != null)
            promptCanvasGroup.alpha = 0f;

        interactionPrompt.SetActive(false);
    }
}