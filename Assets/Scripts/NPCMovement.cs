using UnityEngine;
using UnityEngine.AI;

public class NPCMovement : MonoBehaviour
{
    public enum NPCGroup
    {
        Nerd,
        Athlete,
        Artist,
        Teacher,
        Grade8
    }

    public NPCGroup group;

    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void MoveToNewZone()
    {
        WaypointZone zone = GetZoneForCurrentRound();

        if (zone != null)
        {
            Transform target = zone.GetRandomWaypoint();

            if (target != null)
            {
                agent.SetDestination(target.position);
            }
        }
    }

    WaypointZone GetZoneForCurrentRound()
    {
        int round = Object.FindFirstObjectByType<RoundManager>().currentRound;

        RoundManager rm = Object.FindFirstObjectByType<RoundManager>();

        if (round == 1)
        {
            if (group == NPCGroup.Nerd)
                return rm.mathCore;

            if (group == NPCGroup.Athlete)
                return rm.gymClass;

            if (group == NPCGroup.Artist)
                return rm.artClass;

            if (group == NPCGroup.Teacher)
                return rm.staffLounge;
        }

        if (round == 2)
        {
            if (group == NPCGroup.Nerd)
                return rm.staffLounge;

            if (group == NPCGroup.Athlete)
                return rm.field;

            if (group == NPCGroup.Artist)
                return rm.theatre;

            if (group == NPCGroup.Teacher)
                return rm.mathCore;
        }

        if (round == 3)
        {
            return rm.assemblyHall;
        }

        return null;
    }
}