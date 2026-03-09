using UnityEngine;
using TMPro;
using System.Collections;

public class ItemPickupAnnouncementManager : MonoBehaviour
{
    public static ItemPickupAnnouncementManager Instance { get; private set; }

    [System.Serializable]
    public class PlayerAnnouncementUI
    {
        public GameObject panel;
        public TextMeshProUGUI text;
        public CanvasGroup canvasGroup;
    }

    [Header("Player UI")]
    [SerializeField] private PlayerAnnouncementUI player1UI;
    [SerializeField] private PlayerAnnouncementUI player2UI;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float displayDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    [Header("Colors")]
    [SerializeField] private Color lowValueColor = Color.white;
    [SerializeField] private Color mediumValueColor = Color.cyan;
    [SerializeField] private Color highValueColor = Color.yellow;
    [SerializeField] private Color rareItemColor = Color.magenta;

    [Header("Thresholds")]
    [SerializeField] private int mediumValueThreshold = 10;
    [SerializeField] private int highValueThreshold = 20;

    private Coroutine player1Routine;
    private Coroutine player2Routine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeUI(player1UI);
        InitializeUI(player2UI);
    }

    private void InitializeUI(PlayerAnnouncementUI ui)
    {
        if (ui.panel != null)
            ui.panel.SetActive(false);

        if (ui.canvasGroup != null)
            ui.canvasGroup.alpha = 0f;
    }

    public void ShowAnnouncement(int playerNumber, string itemName, int itemValue, bool isRare)
    {
        string rareText = isRare ? " [RARE]" : "";
        string message = $"{itemName}{rareText} ({itemValue} Votes)";

        Color messageColor = GetAnnouncementColor(itemValue, isRare);

        if (playerNumber == 1)
        {
            if (player1Routine != null)
                StopCoroutine(player1Routine);

            player1Routine = StartCoroutine(
                ShowAnnouncementRoutine(player1UI, message, messageColor)
            );
        }
        else if (playerNumber == 2)
        {
            if (player2Routine != null)
                StopCoroutine(player2Routine);

            player2Routine = StartCoroutine(
                ShowAnnouncementRoutine(player2UI, message, messageColor)
            );
        }
    }

    private Color GetAnnouncementColor(int itemValue, bool isRare)
    {
        if (isRare)
            return rareItemColor;

        if (itemValue >= highValueThreshold)
            return highValueColor;

        if (itemValue >= mediumValueThreshold)
            return mediumValueColor;

        return lowValueColor;
    }

    private IEnumerator ShowAnnouncementRoutine(PlayerAnnouncementUI ui, string message, Color messageColor)
    {
        if (ui.panel == null || ui.text == null || ui.canvasGroup == null)
            yield break;

        ui.panel.SetActive(true);
        ui.text.text = message;
        ui.text.color = messageColor;

        float time = 0f;
        while (time < fadeInDuration)
        {
            time += Time.unscaledDeltaTime;
            ui.canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / fadeInDuration);
            yield return null;
        }

        ui.canvasGroup.alpha = 1f;

        yield return new WaitForSecondsRealtime(displayDuration);

        time = 0f;
        while (time < fadeOutDuration)
        {
            time += Time.unscaledDeltaTime;
            ui.canvasGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeOutDuration);
            yield return null;
        }

        ui.canvasGroup.alpha = 0f;
        ui.panel.SetActive(false);
    }
}