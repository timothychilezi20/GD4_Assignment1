using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RoundManager : MonoBehaviour
{
    public int currentRound = 1;
    private int lastRound = 0;

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
    public GameObject roundUIPanel;
    public Image round1Image;
    public Image round2Image;
    public Image round3Image;
    public float roundUIDuration = 2f;

    private bool isRoundTransitioning = false;

    void Start()
    {
        HideAllRoundImages();

        if (roundUIPanel != null)
            roundUIPanel.SetActive(false);

        StartCoroutine(RoundTransition());
    }

    void Update()
    {
        if (currentRound != lastRound && !isRoundTransitioning)
        {
            StartCoroutine(RoundTransition());
        }
    }

    IEnumerator RoundTransition()
    {
        isRoundTransitioning = true;
        lastRound = currentRound;

        Time.timeScale = 0f;

        if (roundUIPanel != null)
            roundUIPanel.SetActive(true);

        ShowRoundImage(currentRound);

        yield return new WaitForSecondsRealtime(roundUIDuration);

        HideAllRoundImages();

        if (roundUIPanel != null)
            roundUIPanel.SetActive(false);

        Time.timeScale = 1f;

        ApplyRoundRules();
        NotifyNPCs();

        isRoundTransitioning = false;
    }

    void ApplyRoundRules()
    {
        TeacherController[] teachers = FindObjectsByType<TeacherController>(FindObjectsSortMode.None);

        switch (currentRound)
        {
            case 1:
            case 2:
                foreach (TeacherController teacher in teachers)
                {
                    teacher.ResumeNormalBehavior();
                }
                break;

            case 3:
                foreach (TeacherController teacher in teachers)
                {
                    teacher.SetAssemblyMode(true);
                }
                break;
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

    void ShowRoundImage(int round)
    {
        HideAllRoundImages();

        switch (round)
        {
            case 1:
                if (round1Image != null) round1Image.gameObject.SetActive(true);
                break;
            case 2:
                if (round2Image != null) round2Image.gameObject.SetActive(true);
                break;
            case 3:
                if (round3Image != null) round3Image.gameObject.SetActive(true);
                break;
        }
    }

    void HideAllRoundImages()
    {
        if (round1Image != null) round1Image.gameObject.SetActive(false);
        if (round2Image != null) round2Image.gameObject.SetActive(false);
        if (round3Image != null) round3Image.gameObject.SetActive(false);
    }

    public void AdvanceRound()
    {
        if (currentRound < 3 && !isRoundTransitioning)
        {
            currentRound++;
        }
    }
}