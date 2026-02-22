using UnityEngine;

public class npcSpawner : MonoBehaviour
{
    public GameObject[] NPCPrefabs; 
    public Transform[] spawnPoints;

    public float spawnRadius = 0.5f;

    public int totalNPCsToSpawn = 10;
    public int minNPCsToSpawn = 2;
    public int maxNPCsToSpawn = 5;

    void Start()
    {
        int spawnedSoFar = 0;

        while (spawnedSoFar < totalNPCsToSpawn)
        {
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            int NPCsToSpawn = Random.Range(minNPCsToSpawn, maxNPCsToSpawn + 1);
            NPCsToSpawn = Mathf.Min(NPCsToSpawn, totalNPCsToSpawn - spawnedSoFar);

            for (int i = 0; i < NPCsToSpawn; i++)
            {
                int randomIndex = Random.Range(0, NPCPrefabs.Length);

                Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
                randomOffset.y = 0;

                Vector3 spawnPosition = point.position + randomOffset;

                Instantiate(NPCPrefabs[randomIndex], spawnPosition, Quaternion.identity);
                spawnedSoFar++;
            }
        }
    }
}
