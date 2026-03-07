using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

public class StudentController : MonoBehaviour
{

    [Header("Components")]
    private NavMeshAgent agent;


    [Header("Student info")]
    public GroupType1 groupType;
    public int studentID;
    public Pack currentPack;


    [Header("Movement Settings")]
    public float wanderRadius = 5f;
    public float wanderTimer = 5f;
    private float timer;

    [Header("Fire Alarm Settings")]
    public float normalSpeed = 3.5f;
    public float panickedSpeed = 7f;
    public float waitAtAssemblyTime = 3f;

    [Header("Fire Alarm State")]
    public bool isInPanicMode = false;
    public bool isAtAssembly = false;
    public FireAssemblyPoint targetAssemblyPoint;
    public HangoutZone lastHangoutZone;
    public float timeToLeaveAssembly = 0f;


    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;
        normalSpeed = agent.speed;
    }

    private void Update()
    {
        if (isInPanicMode)
        {
            HandlePanicBehavior();
            return;
        }
        //wander behavior
        timer += Time.deltaTime;

        if(timer >= wanderTimer && !agent.pathPending)
        {
            Vector3 newPos = GetRandomPointInHangout();
            agent.SetDestination(newPos);
            timer = 0;
        }
    }

    private void HandlePanicBehavior()
    {
        if(isAtAssembly)
        {
            if(Time.time >= timeToLeaveAssembly)
            {
                ReturnToLastHangout();
            }

            return;
        }

        if(targetAssemblyPoint == null)
        {
            Debug.LogError($"Student {studentID} has no assembly point!");
            return;
        }

        if(!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            isAtAssembly= true;
            timeToLeaveAssembly = Time.time + waitAtAssemblyTime;
            Debug.Log($"Student {studentID} reached assembly point, waiting...");
        }
    }

    private Vector3 GetRandomPointInHangout()
    {
        if(currentPack != null && currentPack.currentHangout != null)
        {
            Vector3 randomDir = Random.insideUnitSphere * currentPack.currentHangout.zoneRadius;
            randomDir.y = 0;
            return currentPack.currentHangout.transform.position + randomDir;
        }

        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1);
        return hit.position;
    }

    public void MoveToHangout(HangoutZone hangout)
    {
        if(currentPack != null)
        {
            currentPack.currentHangout = hangout;

            Vector3 targetPos = hangout.transform.position + (Random.insideUnitSphere * hangout.zoneRadius);
            targetPos.y = 0;
            agent.SetDestination(targetPos);
        }
    }

    public void TriggerFireAlarm(FireAssemblyPoint assemblyPoint)
    {
        if (isInPanicMode) return;

        Debug.Log($"Student {studentID} panicking! Running to assembly point");

        lastHangoutZone = currentPack?.currentHangout;

        isInPanicMode = true;
        isAtAssembly = false;
        targetAssemblyPoint = assemblyPoint;

        agent.speed = panickedSpeed;

        agent.ResetPath();
        agent.SetDestination(assemblyPoint.transform.position);

    }

    public void ReturnToLastHangout()
    {
        if(lastHangoutZone != null)
        {
            Debug.Log($"Student {studentID} returning to {lastHangoutZone.name}");

            isInPanicMode = false;
            isAtAssembly= false;    

            agent.speed = normalSpeed;

            MoveToHangout(lastHangoutZone);

            if(targetAssemblyPoint != null)
            {
                targetAssemblyPoint = null;
            }
        }

        else
        {
            ExitPanicMode();
        }
    }

    public void ExitPanicMode()
    {
        isInPanicMode=false;
        isAtAssembly= false;
        agent.speed=normalSpeed;
        targetAssemblyPoint=null;
    }
}
