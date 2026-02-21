using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
public class NPCMovement : MonoBehaviour
{
    public GroupType groupType;
    public float waitTimeAtPoint = 2f;

    private NavMeshAgent agent;
    private List<WaypointZone> activeZones = new List<WaypointZone>();
    private bool isWaiting;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        RoundManager roundManager = Object.FindFirstObjectByType<RoundManager>();
        roundManager.AssignWaypoints(this);

        MoveToNextPoint();
    }

    public void SetZones(List<WaypointZone> zones)
    {
        activeZones = zones;
        MoveToNextPoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f && !isWaiting)
        {
            StartCoroutine(WaitAndMove());
        }
    }

    System.Collections.IEnumerator WaitAndMove()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTimeAtPoint);
        MoveToNextPoint();
        isWaiting = false;
    }

    void MoveToNextPoint()
    {
        if (activeZones.Count == 0)
            return;
        WaypointZone randomZone = activeZones[Random.Range(0, activeZones.Count)];

        if (randomZone.waypoints.Length == 0)
            return;

        Transform randomWaypoint = randomZone.waypoints[Random.Range(0, randomZone.waypoints.Length)];
        agent.SetDestination(randomWaypoint.position);
    }
}
