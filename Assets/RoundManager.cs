using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public int currentRound = 1;

    private int lastRound;

    public WaypointZone mathCore;
    public WaypointZone gymClass;
    public WaypointZone artClass;
    public WaypointZone staffLounge;
    public WaypointZone theatre;
    public WaypointZone field;
    public WaypointZone assemblyHall;

    void Start()
    {
        lastRound = currentRound;
        NotifyNPCs();
    }

    void Update()
    {
        if (currentRound != lastRound)
        {
            lastRound = currentRound;
            NotifyNPCs();
        }
    }

    void NotifyNPCs()
    {
        NPCMovement[] npcs = Object.FindObjectsByType<NPCMovement>(FindObjectsSortMode.None);

        foreach (NPCMovement npc in npcs)
        {
            npc.MoveToNewZone();
        }
    }
}