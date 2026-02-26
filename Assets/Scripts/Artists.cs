using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using Unity.VisualScripting;
using System.Collections;
public class Artists : MonoBehaviour
{
    [Header("Pack number")]
    public int groupID; // To identify which pack this NPC belongs to

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float turnSpeed = 120f;
    [SerializeField] private float arrivalDistance = 0.5f; //distance from their destination

    [Header("Wander Settings")]
    [SerializeField] private float minWanderTime = 5f; //min time before groups start moving to different spots 
    [SerializeField] private float maxWanderTime = 15f; // Max time before movement
    [SerializeField] private float wanderRadius = 5f; //max distance they move from their spawnned hangout spot

    [Header("Social Settings")]
    [SerializeField] private float mingleDistance = 2f; //how close other artists stand to one another 
    [SerializeField] private float mingleDuration = 10f; //how long they would mingle for

    //Components 
    private NavMeshAgent navAgent;


    private enum ArtistState 
    {
        Wandering, //Moving around hangout 
        Mingling,  //talking with group
        Traveling, //Moving to a different hangout spot
        Waiting  //waiting for original pack to assemble
    }

    private ArtistState currentState = ArtistState.Wandering;
    private float stateTimer = 0f;

    private ArtSpawns currentHangout;
    private ArtSpawns targetHangout;
    private Vector3 hangoutCenter;

    private bool isGroupMoving = false;
    private Vector3 groupDestination;



     void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();

        navAgent.speed = moveSpeed;
        navAgent.angularSpeed = turnSpeed;
        navAgent.stoppingDistance = arrivalDistance;
        navAgent.autoBraking = true;
    }

     void Start()
    {
        currentHangout = FindCurrentHangout();

        if(currentHangout != null )
        {
            hangoutCenter = currentHangout.transform.position;
        }

        StartCoroutine(ArtistBehavior());
    }

    IEnumerator ArtistBehavior() 
    {
        while(true)
        {
            switch(currentState)
            {
                case ArtistState.Wandering:
                    yield return StartCoroutine(WanderRoutine()); break;

                case ArtistState.Mingling:
                    yield return StartCoroutine(MingleRoutine()); break;
                case ArtistState.Traveling:
                    yield return StartCoroutine(TravelRoutine()); break;
                case ArtistState.Waiting:
                    yield return StartCoroutine(WaitRoutine()); break;
            }

            yield return null;
        }
       

    }


    IEnumerator WanderRoutine()
    {
        //picks random point in wander radius of their hangout
        Vector3 randomPoint = GetRandomPointInCircle(hangoutCenter, wanderRadius);

        if(IsValidDestination(randomPoint))
        {
            navAgent.SetDestination(randomPoint);

            float wanderTime = Random.Range(minWanderTime, maxWanderTime);
            float elapsedTime = 0f;

            while(elapsedTime < wanderTime)
            {
                if(!navAgent.pathPending && navAgent.remainingDistance <= arrivalDistance)
                {
                    randomPoint = GetRandomPointInCircle(hangoutCenter, wanderRadius);

                    if (IsValidDestination(randomPoint))
                    {
                        navAgent.SetDestination(randomPoint);
                    }
                }

                if (Random.value < 0.01f)
                {
                    if (CheckForNearbyArtists())
                    {
                        currentState = ArtistState.Mingling;
                        yield break;
                    }
                }

                elapsedTime += Time.deltaTime;
                yield return null;

            }
        }

        //Mingle a bit after wondering around
        currentState = ArtistState.Mingling; 
    }

    IEnumerator MingleRoutine()
    {

        navAgent.ResetPath();

        //face random direction 
        transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        //Mingle for a specific amount of time
        float mingleTime = mingleDuration;
        float elapsedTime = 0f;

        while(elapsedTime < mingleTime)
        {
            if (Random.value < 0.1f)
            {
                StartCoroutine(SmoothRotate(Random.Range(0, 360)));
            }

            elapsedTime = Time.deltaTime;
            yield return null;
        }


        if (ShouldGroupTravel())
        {
            currentState = ArtistState.Traveling;
        }

        else
        {
            currentState = ArtistState.Wandering;
    
         }
    }

   

    IEnumerator TravelRoutine()
    {
        if (targetHangout == null)
        {
            targetHangout = GetRandomHangoutExcept(currentHangout);
        }

        if(targetHangout != null)
        {
            Vector3 destination = targetHangout.GetRandomSpawnPoint();

            if (IsValidDestination(destination))
            {
                navAgent.SetDestination(destination);

                //Travel until arrival
                while (navAgent.pathPending || navAgent.remainingDistance > arrivalDistance)
                {
                    if(navAgent.velocity.magnitude < 0.1f && navAgent.remainingDistance > arrivalDistance)
                    {
                        Debug.LogWarning($"Artist {gameObject.name} might be stuck");

                        break;
                    }

                    yield return null;
                }

                //moved to new hangout
                currentHangout = targetHangout;
                hangoutCenter = targetHangout.transform.position;
                targetHangout = null;

                //wait for other members of pack to arrive 
                currentState = ArtistState.Waiting;
            }
            else
            {
                //wrong destination, restart wandering 

                currentState = ArtistState.Wandering;
            }
        }

        else
        {
            currentState = ArtistState.Wandering;
        }
    }

    IEnumerator WaitRoutine()
    {
        //wait for all pack members to arrive 
        float waitTime = 30f;
        float elapsedTime = 0f; 

        while(elapsedTime< waitTime)
        {
            if (AllGroupMembersArrived())
            {
                currentState = ArtistState.Wandering;
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //time over WANDER TIME
        currentState = ArtistState.Wandering;
    }

    IEnumerator SmoothRotate(float targetAngle)
    {
        Quaternion targetRotation = Quaternion.Euler(0,targetAngle,0);
        float rotateTime = 1f;
        float elapsedTime = 0f;

        while(elapsedTime< rotateTime)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, elapsedTime / rotateTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
    }


    //Helper methods, that check distance and spacing for group movements 
    Vector3 GetRandomPointInCircle(Vector3 center, float radius)
    {
        Vector2 randomCircle = Random.insideUnitSphere * radius;
        return new Vector3(center.x + randomCircle.x, center.y, center.z + randomCircle.y);
    }

    bool IsValidDestination(Vector3 destination)
    {
        NavMeshHit hit;
        return NavMesh.SamplePosition(destination, out hit, 1f, NavMesh.AllAreas);
    }

    bool CheckForNearbyArtists()
    {
        Artists[] allArtists = Object.FindObjectsOfType<Artists>();
        int nearbyCount = 0;

        foreach(Artists artist in allArtists)
        {
            if(artist != this && artist.groupID == groupID)
            {
                float distance = Vector3.Distance(transform.position,artist.transform.position);

                if(distance < mingleDistance)
                {
                    nearbyCount++;
                }
            }
        }
        

       return nearbyCount > 0;
    }

    bool AllGroupMembersArrived()
    {
        Artists[] allArtists = FindObjectsOfType<Artists>();

        foreach(Artists artist in allArtists)
        {
            if(artist.groupID == groupID)
            {
                float distanceToHangout = Vector3.Distance(artist.transform.position,hangoutCenter);

                if(distanceToHangout > arrivalDistance * 2)
                {
                    return false;
                }
            }
        }
        return true;
    }

    bool ShouldGroupTravel()
    {
        return Random.value < 0.1f;
    }

    ArtSpawns GetRandomHangoutExcept(ArtSpawns exclude)
    {
        ArtSpawns[] allHangouts = FindObjectsOfType<ArtSpawns>();
        if (allHangouts.Length <= 1) return null;

        ArtSpawns randomHangout;
        do
        {
            randomHangout = allHangouts[Random.Range(0, allHangouts.Length)];
        } while(randomHangout == exclude);

        return randomHangout;
    }

    ArtSpawns FindCurrentHangout()
    {
        ArtSpawns[] allHangouts = FindObjectsOfType<ArtSpawns>();
        ArtSpawns closest = null;
        float closestDistance = float.MaxValue;

        foreach(ArtSpawns hangout in allHangouts)
        {
            float distance = Vector3.Distance(transform.position,hangout.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = hangout;
            }
           
        }

        return closest; 
    }

    //Debugging visualizer
    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            // Draw wander radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(hangoutCenter, wanderRadius);

            // Draw current state
#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2,
                $"Group {groupID}: {currentState}");
#endif
        }
    }
}

