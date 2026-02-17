using UnityEngine;

public class NerdSpawner : MonoBehaviour
{

    public GameObject[] nerdPrefabs; 
    public Transform[] spawnPoint;
    public int numberOfNerdsToSpawn = 5;
    public float spawnRadius = 5f; //so they scatter

    void Start()
    {
        foreach (Transform point in spawnPoint)
        {
            for (int i = 0; i < numberOfNerdsToSpawn; i++)
        {
            int randomIndex = Random.Range(0, nerdPrefabs.Length);

            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = 0;

            Vector3 spawnPosition = point.position + randomOffset;

            Instantiate(nerdPrefabs[randomIndex], spawnPosition, Quaternion.identity);
        }
        }
        
        
    }

    void Update()
    {
        
    }
}
