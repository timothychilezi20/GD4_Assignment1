using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [Header("Items that can spawn")]
    public GameObject[] itemPrefabs;

    [Header("Spawn locations")]
    public Transform[] spawnPoints;

    [Header("Number of items to spawn")]
    public int itemsToSpawn = 5;

    void Start()
    {
        SpawnItems();
    }

    void SpawnItems()
    {
        List<Transform> availableSpawns = new List<Transform>(spawnPoints);

        for (int i = 0; i < itemsToSpawn; i++)
        {
            if (availableSpawns.Count == 0)
                return;

            // Pick random spawn location
            int spawnIndex = Random.Range(0, availableSpawns.Count);
            Transform spawnPoint = availableSpawns[spawnIndex];

            // Pick random item
            int itemIndex = Random.Range(0, itemPrefabs.Length);
            GameObject item = itemPrefabs[itemIndex];

            // Spawn item
            Instantiate(item, spawnPoint.position, Quaternion.identity);

            // Remove used spawn point so items don't stack
            availableSpawns.RemoveAt(spawnIndex);
        }
    }
    public void Respawn(ItemSpawnPoint spawnPoint)
    {
        if (spawnPoint == null || itemPrefabs == null || itemPrefabs.Length == 0)
            return;

        // Example respawn logic: instantiate a random item at the given spawn point
        int randomIndex = UnityEngine.Random.Range(0, itemPrefabs.Length);
        GameObject newItem = Instantiate(itemPrefabs[randomIndex], spawnPoint.transform.position, Quaternion.identity);
        spawnPoint.SetItem(newItem);
    }
}