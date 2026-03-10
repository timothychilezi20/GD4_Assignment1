using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class TeacherController : MonoBehaviour
{
    [Header("Components")]
    private NavMeshAgent agent;

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
    private TeacherSpot targetSpot;

    [Header("Behavior Settings")]
    public float behaviorChangeInterval = 15f;
    private float behaviorTimer;

    [Header("Mingling Settings")]
    public float minglingMoveInterval = 4f;
    private float minglingTimer;

    [Header("Special States")]
    [SerializeField] private bool forceAssemblyMode = false;
    [SerializeField] private bool forceIdle = false;

    [Header("Current State")]
    public TeacherBehavior currentBehavior;

    [Header("Animator")]
    [SerializeField] Animator animator;

    private bool fireAlarmActive = false;

    public enum TeacherBehavior
    {
        Mingling,
        Patrolling,
        MovingToSpot,
        MovingToAssembly,
        Idle
    }

    public enum TeacherType
    {
        Math,
        Art,
        Sports,
        Staff,
        Principal
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError($"{teacherName} is missing a NavMeshAgent!");
            enabled = false;
            return;
        }

        behaviorTimer = behaviorChangeInterval;
        minglingTimer = minglingMoveInterval;
        animator = GetComponent<Animator>();

        ChooseNewBehavior();
    }

    private void Update()
    {
        if (forceIdle)
        {
            currentBehavior = TeacherBehavior.Idle;
            HandleIdle();
            return;
        }

        if (fireAlarmActive)
        {
            HandleAssemblyMode();
            return;
        }

        if (forceAssemblyMode)
        {
            HandleAssemblyMode();
            return;
        }

        behaviorTimer -= Time.deltaTime;

        if (behaviorTimer <= 0f)
        {
            ChooseNewBehavior();
            behaviorTimer = behaviorChangeInterval;
        }

        switch (currentBehavior)
        {
            case TeacherBehavior.Patrolling:
               
                HandlePatrolling();
                break;

            case TeacherBehavior.Mingling:

                HandleMingling();
                break;

            case TeacherBehavior.MovingToSpot:
                HandleMovingToSpot();
                break;

            case TeacherBehavior.Idle:
                HandleIdle();
                break;
        }
    }

    private void ChooseNewBehavior()
    {
        int random = Random.Range(0, 100);

        if (random < 40)
        {
            StartPatrolling();
        }
        else if (random < 80)
        {
            MoveToRandomSpot();
        }
        else
        {
            currentBehavior = TeacherBehavior.Idle;
            agent.ResetPath();
        }
    }

    private void StartPatrolling()
    {
        if (availableRoutes.Count == 0)
        {
            Debug.LogWarning($"No patrol routes assigned to {teacherName}");
            currentBehavior = TeacherBehavior.Idle;
            return;
        }

        currentRoute = availableRoutes[Random.Range(0, availableRoutes.Count)];
        currentWaypointIndex = 0;
        isReversing = false;

        if (currentRoute.waypoints != null && currentRoute.waypoints.Count > 0)
        {
            currentBehavior = TeacherBehavior.Patrolling;
            agent.SetDestination(currentRoute.waypoints[currentWaypointIndex].position);
            animator.SetBool("Walk", true);
        }
        else
        {
            Debug.LogWarning($"{teacherName} selected a patrol route with no waypoints.");
            currentBehavior = TeacherBehavior.Idle;
        }
    }

    private void HandlePatrolling()
    {
        if (currentRoute == null || currentRoute.waypoints == null || currentRoute.waypoints.Count == 0)
        {
            StartPatrolling();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
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
            animator.SetBool("Walk", true);
        }
    }

    private void HandleReversePatrol()
    {
        if (!isReversing)
        {
            if (currentWaypointIndex < currentRoute.waypoints.Count - 1)
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
            if (currentWaypointIndex > 0)
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

    private void HandleMovingToSpot()
    {
        if (targetSpot == null)
        {
            currentBehavior = TeacherBehavior.Idle;
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentSpot = targetSpot;
            targetSpot = null;
            currentBehavior = TeacherBehavior.Mingling;
            animator.SetBool("Walk", true);
            minglingTimer = minglingMoveInterval;
        }
    }

    private void HandleMingling()
    {
        if (currentSpot == null)
        {
            currentSpot = GetCurrentSpot();

            if (currentSpot == null)
            {
                currentBehavior = TeacherBehavior.Idle;
                animator.SetBool("Walk", false);
                return;
            }
        }

        minglingTimer -= Time.deltaTime;

        if (minglingTimer <= 0f && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            Vector3 randomPoint = currentSpot.transform.position + (Random.insideUnitSphere * currentSpot.minglingRadius);
            randomPoint.y = currentSpot.transform.position.y;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, currentSpot.minglingRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                animator.SetBool("Walk", false);
            }

            minglingTimer = minglingMoveInterval;
        }
    }

    private void HandleIdle()
    {
        agent.ResetPath();
        animator.SetBool("Walk",false);
    }

    private void MoveToRandomSpot()
    {
        if (assignedSpots == null || assignedSpots.Count == 0)
        {
            currentBehavior = TeacherBehavior.Idle;
            return;
        }

        List<TeacherSpot> otherSpots = new List<TeacherSpot>();

        foreach (TeacherSpot spot in assignedSpots)
        {
            if (spot != null && spot != currentSpot)
            {
                otherSpots.Add(spot);
            }
        }

        if (otherSpots.Count == 0)
        {
            currentBehavior = TeacherBehavior.Mingling;
            return;
        }

        targetSpot = otherSpots[Random.Range(0, otherSpots.Count)];
        agent.SetDestination(targetSpot.transform.position);
        currentBehavior = TeacherBehavior.MovingToSpot;
    }

    private TeacherSpot GetCurrentSpot()
    {
        TeacherSpot closest = null;
        float closestDistance = float.MaxValue;

        foreach (TeacherSpot spot in assignedSpots)
        {
            if (spot == null) continue;

            float distance = Vector3.Distance(transform.position, spot.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = spot;
            }
        }

        return closest;
    }

    private TeacherSpot GetAssemblySpot()
    {
        TeacherSpot closestAssemblySpot = null;
        float closestDistance = float.MaxValue;

        foreach (TeacherSpot spot in assignedSpots)
        {
            if (spot == null || !spot.isAssemblySpot) continue;

            float distance = Vector3.Distance(transform.position, spot.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestAssemblySpot = spot;
            }
        }

        return closestAssemblySpot;
    }

    private void HandleAssemblyMode()
    {
        TeacherSpot assemblySpot = GetAssemblySpot();

        if (assemblySpot == null)
        {
            Debug.LogWarning($"{teacherName} has no assembly spot assigned.");
            currentBehavior = TeacherBehavior.Idle;
            HandleIdle();
            return;
        }

        currentBehavior = TeacherBehavior.MovingToAssembly;
        agent.SetDestination(assemblySpot.transform.position);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentSpot = assemblySpot;
            currentBehavior = TeacherBehavior.Mingling;
            minglingTimer = minglingMoveInterval;
        }
    }

    public void SetAssemblyMode(bool enabled)
    {
        forceAssemblyMode = enabled;

        if (enabled)
        {
            targetSpot = null;
            currentRoute = null;
            behaviorTimer = behaviorChangeInterval;
        }
    }

    public void SetIdleMode(bool enabled)
    {
        forceIdle = enabled;

        if (enabled)
        {
            forceAssemblyMode = false;
            fireAlarmActive = false;
            agent.ResetPath();
            animator.SetBool("Walk",false);
            currentBehavior = TeacherBehavior.Idle;
        }
    }

    public void ResumeNormalBehavior()
    {
        forceIdle = false;
        forceAssemblyMode = false;
        fireAlarmActive = false;
        behaviorTimer = behaviorChangeInterval;
        ChooseNewBehavior();
    }

    public void SetFireAlarmMode(bool active)
    {
        fireAlarmActive = active;

        if (fireAlarmActive)
        {
            Debug.Log($"{teacherName} responding to fire alarm!");
            targetSpot = null;
            currentRoute = null;
            agent.ResetPath();

            TeacherSpot assemblySpot = GetAssemblySpot();
            if (assemblySpot != null)
            {
                agent.SetDestination(assemblySpot.transform.position);
                currentBehavior = TeacherBehavior.MovingToAssembly;
            }
            else
            {
                currentBehavior = TeacherBehavior.Idle;
            }
        }
        else
        {
            Debug.Log($"{teacherName} returning to normal behavior.");
            ResumeNormalBehavior();
        }
    }
}