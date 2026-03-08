//using UnityEngine;
//using UnityEngine.AI;

//public class NPCSpawner : MonoBehaviour
//{
//    [Header("Prefab Groups")]
//    public GameObject nerdPrefab;  
//    public GameObject athletePrefab;   
//    public GameObject artistPrefab;   
//    public GameObject teacherPrefab;    
//    public GameObject grade8Prefab;     

//    [Header("Spawn Settings")]
//    public int nerdCount = 5;
//    public int athleteCount = 5;
//    public int artistCount = 5;
//    public int teacherCount = 3;
//    public int grade8Count = 10;

//    public Transform[] spawnPoints;
//    public float spawnRadius = 5f;

//    private void Start()
//    {
//        SpawnGroup(nerdPrefab, nerdCount, NPCMovement.NPCGroup.Nerd);
//        SpawnGroup(athletePrefab, athleteCount, NPCMovement.NPCGroup.Athlete);
//        SpawnGroup(artistPrefab, artistCount, NPCMovement.NPCGroup.Artist);
//        SpawnGroup(teacherPrefab, teacherCount, NPCMovement.NPCGroup.Teacher);
//        SpawnGroup(grade8Prefab, grade8Count, NPCMovement.NPCGroup.Grade8);
//    }

//    void SpawnGroup(GameObject prefab, int count, NPCMovement.NPCGroup group)
//    {
//        for (int i = 0; i < count; i++)
//        {
//            if (spawnPoints.Length == 0)
//            {
//                Debug.LogError("No spawn points assigned!");
//                return;
//            }

//            Transform randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
//            if (randomSpawn == null)
//            {
//                Debug.LogWarning("Spawn point at index is null!");
//                continue;
//            }

//            Vector3 randomPosition = randomSpawn.position + Random.insideUnitSphere * spawnRadius;
//            randomPosition.y = randomSpawn.position.y;

//            NavMeshHit hit;
//            if (NavMesh.SamplePosition(randomPosition, out hit, 5f, NavMesh.AllAreas))
//            {
//                GameObject spawnedNPC = Instantiate(prefab, hit.position, Quaternion.identity, transform);

//                NPCMovement movement = spawnedNPC.GetComponent<NPCMovement>();
//                if (movement != null && movement.GetGroup() != group)
//                {
//                    Debug.LogWarning($"Spawned {prefab.name} has group {movement.GetGroup()} but should be {group}");
//                }
//            }
//            else
//            {
//                Debug.LogWarning("No NavMesh found near spawn point: " + randomSpawn.name);
//            }
//        }
//    }
//}