using UnityEngine;
using UnityEngine.AI;

public class NPCSpawner : MonoBehaviour
{
    [Header("Prefab Grouops")]
    public GameObject nerdPrefab;
    public GameObject athletePrefab;
    public GameObject artistPrefab;
    public GameObject teacherPrefab;
    public GameObject grade8Prefab;

    [Header("Spawn Settings")]
    public int nerdCount = 5;
    public int athleteCount = 5;
    public int artistCount = 5;
    public int teacherCount = 3;
    public int grade8Count = 10;

    public Transform[] spawnPoints;
    public float spawnRadius = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        SpawnGroup(nerdPrefab, nerdCount);
        SpawnGroup(athletePrefab, athleteCount);
        SpawnGroup(artistPrefab, artistCount);
        SpawnGroup(teacherPrefab, teacherCount);
        SpawnGroup(grade8Prefab, grade8Count);
    }

    void SpawnGroup(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Transform randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (randomSpawn == null)
            {
                Debug.LogWarning("Spawn point at index is null!");
                continue;
            }

            Vector3 randomPosition = randomSpawn.position + Random.insideUnitSphere * spawnRadius;
            randomPosition.y = randomSpawn.position.y;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPosition, out hit, 5f, NavMesh.AllAreas))
            {
                Instantiate(prefab, hit.position, Quaternion.identity, transform);
            }
            else
            {
                Debug.LogWarning("No NavMesh found near spawn point: " + randomSpawn.name);
            }
        }
    }
}
