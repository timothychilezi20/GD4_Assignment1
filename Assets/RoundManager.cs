using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RoundManager : MonoBehaviour
{
    public int currentRound = 1;

    [Header("Waypoint Zones")]
    public WaypointZone mathCore;
    public WaypointZone gymClass;
    public WaypointZone artClass;
    public WaypointZone staffLounge;
    public WaypointZone zuluClass;
    public WaypointZone mathLit;
    public WaypointZone afrClass;
    public WaypointZone assemblyHall;
    public WaypointZone tuckShop;

    [Header("UI")]
    [SerializeField] private GameObject roundUIPanel;
    [SerializeField] private CanvasGroup roundCanvasGroup;

    [SerializeField] private Image round1Image;
    [SerializeField] private Image round2Image;
    [SerializeField] private Image round3Image;

    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.4f;

    private int lastRound = 0;
    private bool isRoundTransitioning = false;

    void Start()
    {
        if (roundUIPanel != null)
            roundUIPanel.SetActive(true);

        if (roundCanvasGroup != null)
            roundCanvasGroup.alpha = 0f;

        HideAllRoundImages();

        // Show Round 1 immediately at game start
        StartCoroutine(RoundTransition());
    }

    void Update()
    {
        if (currentRound != lastRound && !isRoundTransitioning)
        {
            StartCoroutine(RoundTransition());
        }
    }

    void NotifyNPCs()
    {
        NPCMovement[] npcs = FindObjectsByType<NPCMovement>(FindObjectsSortMode.None);
        foreach (NPCMovement npc in npcs)
        {
            npc.MoveToNewZone();
        }
    }

    IEnumerator RoundTransition()
    {
        isRoundTransitioning = true;
        lastRound = currentRound;

        Debug.Log("Starting round transition for round: " + currentRound);

        Time.timeScale = 0f;

        if (roundUIPanel != null)
            roundUIPanel.SetActive(true);

        ShowCurrentRoundImage();

        yield return StartCoroutine(FadeCanvas(0f, 1f, fadeInDuration));
        yield return new WaitForSecondsRealtime(holdDuration);
        yield return StartCoroutine(FadeCanvas(1f, 0f, fadeOutDuration));

        HideAllRoundImages();

        if (roundUIPanel != null)
            roundUIPanel.SetActive(false);

        Time.timeScale = 1f;

        NotifyNPCs();

        isRoundTransitioning = false;
    }

    void ShowCurrentRoundImage()
    {
        HideAllRoundImages();

        switch (currentRound)
        {
            case 1:
                if (round1Image != null)
                    round1Image.gameObject.SetActive(true);
                break;

            case 2:
                if (round2Image != null)
                    round2Image.gameObject.SetActive(true);
                break;

            case 3:
                if (round3Image != null)
                    round3Image.gameObject.SetActive(true);
                break;

            default:
                Debug.LogWarning("No image assigned for round: " + currentRound);
                break;
        }
    }

    void HideAllRoundImages()
    {
        if (round1Image != null)
            round1Image.gameObject.SetActive(false);

        if (round2Image != null)
            round2Image.gameObject.SetActive(false);

        if (round3Image != null)
            round3Image.gameObject.SetActive(false);
    }

    IEnumerator FadeCanvas(float start, float end, float duration)
    {
        if (roundCanvasGroup == null)
        {
            Debug.LogWarning("Round CanvasGroup is missing!");
            yield break;
        }

        float elapsed = 0f;
        roundCanvasGroup.alpha = start;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            roundCanvasGroup.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }

        roundCanvasGroup.alpha = end;
    }

    public void AdvanceRound()
    {
        if (currentRound < 3)
        {
            currentRound++;
        }
    }
}