using UnityEngine;
using UnityEngine.UI; // For UI
using System.Collections;

public class RoundManager : MonoBehaviour
{
    public int currentRound = 1;
    private int lastRound;

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
    public GameObject roundUIPanel;  // Assign a UI panel in the Inspector
    public Text roundUIText;         // Assign the Text component inside the panel
    public float roundUIDuration = 2f; // How long the UI is visible

    private bool isRoundTransitioning = false;

    void Start()
    {
        lastRound = currentRound;
        NotifyNPCs();
    }

    void Update()
    {
        if (currentRound != lastRound && !isRoundTransitioning)
        {
            lastRound = currentRound;
            StartCoroutine(RoundTransition());
        }
    }

    void NotifyNPCs()
    {
        // This will tell all NPCs to move to new zones
        NPCMovement[] npcs = FindObjectsByType<NPCMovement>(FindObjectsSortMode.None);
        foreach (NPCMovement npc in npcs)
        {
            npc.MoveToNewZone();
        }
    }

    IEnumerator RoundTransition()
    {
        isRoundTransitioning = true;

        // Pause gameplay
        Time.timeScale = 0f;

        // Show round UI
        roundUIPanel.SetActive(true);
        roundUIText.text = $"Round {currentRound}";

        // Wait in real-time (not affected by Time.timeScale)
        yield return new WaitForSecondsRealtime(roundUIDuration);

        // Hide UI
        roundUIPanel.SetActive(false);

        // Resume gameplay
        Time.timeScale = 1f;

        // Notify NPCs
        NotifyNPCs();

        isRoundTransitioning = false;
    }
}