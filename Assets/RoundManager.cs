using UnityEngine;
using System.Collections.Generic;

public class RoundManager : MonoBehaviour
{
    public int currentRound = 1;

    [Header("Zones")]
    public WaypointZone dramaClass;
    public WaypointZone staffLounge;
    public WaypointZone afrClass;
    public WaypointZone artClass;
    public WaypointZone gymClass;
    public WaypointZone mathCore;
    public WaypointZone zuluClass;
    public WaypointZone mathLit;
    public WaypointZone tuckShop;
    public WaypointZone assemblyHall; 

    public void AssignWaypoints(NPCMovement npc)
    {
        List<WaypointZone> allowedZones = new List<WaypointZone>();

        switch (currentRound)
        {
            case 1:
                SetupRound1(npc, allowedZones);
                break;
            case 2:
                SetupRound2(npc, allowedZones);
                break;
            case 3:
                SetupRound3(npc, allowedZones);
                break;
        }

        npc.SetZones(allowedZones);
    }

    void SetupRound1(NPCMovement npc, List<WaypointZone> zones)
    {
        if (npc.groupType == GroupType.Nerd)
        {
            zones.Add(mathCore);
            zones.Add(afrClass);
            zones.Add(zuluClass);
        }

        if (npc.groupType == GroupType.Athlete)
        {
            zones.Add(gymClass);
            zones.Add(mathLit);
        }

        if (npc.groupType == GroupType.Artist)
        {
            zones.Add(artClass);
            zones.Add(dramaClass);
            zones.Add(tuckShop);
        }

        if (npc.groupType == GroupType.Teacher)
        {
            zones.Add(staffLounge);
            zones.Add(mathCore);
            zones.Add(afrClass);
            zones.Add(zuluClass);
        }

        if (npc.groupType == GroupType.Grade8)
        {
            zones.Add(tuckShop);
            zones.Add(gymClass);
        }
    }

    void SetupRound2(NPCMovement npc, List<WaypointZone> zones)
    {
        if (npc.groupType == GroupType.Nerd)
        {
            zones.Add(staffLounge);
            zones.Add(gymClass);
            zones.Add(tuckShop);
            zones.Add(mathCore);
        }

        if (npc.groupType == GroupType.Athlete)
        {
            zones.Add(gymClass);
            zones.Add(tuckShop);
            zones.Add(mathLit);
        }

        if (npc.groupType == GroupType.Artist)
        {
            zones.Add(artClass);
            zones.Add(dramaClass);
            zones.Add(gymClass);
            zones.Add(tuckShop);
        }

        if (npc.groupType == GroupType.Teacher)
        {
            zones.Add(gymClass);
            zones.Add(staffLounge);
            zones.Add(mathCore);
            zones.Add(afrClass);
        }

        if (npc.groupType == GroupType.Grade8)
        {
            zones.Add(gymClass);
            zones.Add(tuckShop);
            zones.Add(dramaClass);
        }
    }

    void SetupRound3(NPCMovement npc, List<WaypointZone> zones)
    {
        zones.Add(assemblyHall);
    }

    public void NextRound()
    {
        currentRound++;

        NPCMovement[] allNPCs = Object.FindObjectsByType<NPCMovement>(FindObjectsSortMode.None);

        foreach (NPCMovement npc in allNPCs)
        {
            AssignWaypoints(npc);
        }
    }
}