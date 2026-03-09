using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class TeacherController : MonoBehaviour
{
    [Header("Components")]
    private NavMeshAgent agent;
    private Animator animator;


    [Header("Teacher Info")]
    public TeacherType teacherType;
    public string teacherName;

    [Header("Patrol Routes")]
    public List<PatrolRoute> availableRoutes = new List<PatrolRoute>();
    private PatrolRoute currentRoute;
    private int currentWaypointIndex = 0;
    private bool isReversing = false;

    [Header("Teacher Spots")]
    public List<TeacherSpot> assignedSpots = new List<TeacherSpot>();
    private TeacherSpot currentSpot;

    [Header("Behavior Settings")]
    public float behaviorChangeInterval = 15f; //Time between behavior changes
    private float behaviorTimer;

    [Header("Current State")]
    public TeacherBehavior currentBehavior;
    public float minglingDuration = 20f; //How long they mingle for

    public enum TeacherBehavior
    {
        Mingling,
        Patrolling,
        MovingToSpot,
        Idle
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        ChooseNewBehavior();
        behaviorTimer = behaviorChangeInterval;
        animator = GetComponent<Animator>();
        animator.SetBool("Walk", true);

    }

    private void Update()
    {
        behaviorTimer -= Time.deltaTime;

        if(behaviorTimer <= 0 )
        {
            ChooseNewBehavior();
            behaviorTimer = behaviorChangeInterval;
        }

        switch(currentBehavior )
        {
            case TeacherBehavior.Patrolling:
                HandlePatrolling();
                break;
            case TeacherBehavior.Mingling:
                HandleMingling();
                break;
            case TeacherBehavior.MovingToSpot:
                //Check if arrived from movement
                if(!agent.pathPending && agent.remainingDistance < 0.5)
                {
                    currentBehavior = TeacherBehavior.Mingling;

                    currentSpot = GetCurrentSpot();
                }
                break;
            case TeacherBehavior.Idle:
                agent.ResetPath();
                break;
        }
    }

    private void ChooseNewBehavior()
    {
        int random = Random.Range( 0, 100 );

        if(random < 40)
        {
            StartPatrolling();
        }

        else if( random < 75)
        {
            MoveToRandomSpot();
        }

        else if(random < 90)
        {
            MoveToRandomSpot();
        }
        else
        {
            currentBehavior = TeacherBehavior.Idle;
        }
    }

    private void StartPatrolling()
    {
        if(availableRoutes.Count == 0)
        {
            Debug.LogWarning($"No patrol routes assigned to {teacherName}");
            return;
        }

        currentRoute = availableRoutes[Random.Range(0,availableRoutes.Count)];
        currentWaypointIndex = 0;
        isReversing = false;    

        if(currentRoute.waypoints.Count > 0) 
        {
            agent.SetDestination(currentRoute.waypoints[0].position);
            currentBehavior = TeacherBehavior.Patrolling;
        }
    }

    private void HandlePatrolling()
    {
        if(currentRoute == null || currentRoute.waypoints.Count == 0)
        {
            StartPatrolling();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (currentRoute.reverseRoute)
            {
                HandleReversePatrol();
            }
            else if (currentRoute.loopRoute)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % currentRoute.waypoints.Count;
            }
            else
            {
                if (currentWaypointIndex < currentRoute.waypoints.Count - 1)
                {
                    currentWaypointIndex++;
                }
                else
                {
                    ChooseNewBehavior();
                    return;
                }
            }

            agent.SetDestination(currentRoute.waypoints[currentWaypointIndex].position);
            animator.SetBool("Walk", true );

            Invoke("ContinuePatrol", Random.Range(0.5f, 2f));
        }
    }

    private void HandleReversePatrol()
    {
        if(!isReversing)
        {
            if(currentWaypointIndex < currentRoute.waypoints.Count - 1)
            {
                currentWaypointIndex++;
            }

            else
            {
                isReversing = true;
                currentWaypointIndex--;
            }
        }

        else
        {
            if(currentWaypointIndex > 0)
            {
                currentWaypointIndex--;
            }

            else
            {
                isReversing = false;
                currentWaypointIndex++;
            }
        }
    }

    private void ContinuePatrol()
    {
        if (currentBehavior == TeacherBehavior.Patrolling && currentRoute != null)
        {
            agent.SetDestination(currentRoute.waypoints[currentWaypointIndex].position);
        }
    }

    private void HandleMingling()
    {
        if(currentSpot != null && agent.remainingDistance < 1f)
        {
            Vector3 randomPoint = currentSpot.transform.position + (Random.insideUnitSphere * currentSpot.minglingRadius);
            randomPoint.y = 0f;

            NavMeshHit hit;
            if(NavMesh.SamplePosition(randomPoint, out hit, currentSpot.minglingRadius, 1))
            {
                agent.SetDestination(hit.position);
            }

            Invoke("HandleMingling", Random.Range(3f, 8f));
        }
    }

    private void MoveToRandomSpot()
    {
        if (assignedSpots.Count <= 1) return;

        List<TeacherSpot> otherSpots = new List<TeacherSpot>();

        foreach(TeacherSpot spot in assignedSpots)
        {
            if(spot != currentSpot)
            {
                otherSpots.Add(spot);
            }
        }

        if(otherSpots.Count > 0)
        {
            TeacherSpot targetSpot = otherSpots[Random.Range(0, otherSpots.Count)];

            agent.SetDestination(targetSpot.transform.position);

            currentBehavior = TeacherBehavior.MovingToSpot;
        }
    }

    private TeacherSpot GetCurrentSpot()
    {
        TeacherSpot closest = null;

        float closestDistance = float.MaxValue;

        foreach(TeacherSpot spot in assignedSpots )
        {
            float distance = Vector3.Distance(transform.position, spot.transform.position);
            if(distance < closestDistance)
            {
                closestDistance = distance;
                closest = spot;
            }
        }

        return closest;
    }
}
