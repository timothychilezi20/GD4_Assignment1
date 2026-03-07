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

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;
    }

    private void Update()
    {
        //wander behavior
        timer += Time.deltaTime;

        if(timer >= wanderTimer && !agent.pathPending)
        {
            Vector3 newPos = GetRandomPointInHangout();
            agent.SetDestination(newPos);
            timer = 0;
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
}
