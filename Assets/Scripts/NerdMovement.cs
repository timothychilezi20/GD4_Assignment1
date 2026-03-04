using UnityEngine;
using UnityEngine.AI;

public class NerdMovement : MonoBehaviour
{
    public Transform[] waypoints;
    private UnityEngine.AI.NavMeshAgent agent;
    private int currentWaypointIndex;
    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        MoveToNextWaypoint();
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            MoveToNextWaypoint();
        }
    }

    void MoveToNextWaypoint()
    {
        if (waypoints.Length == 0)
            return;

        currentWaypointIndex = Random.Range(0, waypoints.Length);
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }
}
