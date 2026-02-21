using UnityEngine;
using UnityEngine.AI;

public class ArtistsAI : MonoBehaviour
{
    public Transform[] waypoints;
    public NavMeshAgent agent;
    private int currentWaypoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToNextWaypoint(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextWaypoint();
        }
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0)
            return;

        currentWaypoint = Random.Range(0, waypoints.Length);
        agent.SetDestination(waypoints[currentWaypoint].position);
    }
}
