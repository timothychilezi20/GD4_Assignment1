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
        if (itemPrefabs.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("ItemSpawner: Missing prefabs or spawn points!");
            return;
        }

        SpawnItems();
    }

    void SpawnItems()
    {
        List<Transform> availableSpawns = new List<Transform>(spawnPoints);

        int spawnCount = Mathf.Min(itemsToSpawn, availableSpawns.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            int spawnIndex = Random.Range(0, availableSpawns.Count);
            Transform spawnPoint = availableSpawns[spawnIndex];

            int itemIndex = Random.Range(0, itemPrefabs.Length);
            GameObject itemPrefab = itemPrefabs[itemIndex];

            Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);

            availableSpawns.RemoveAt(spawnIndex);
        }
    }

    public void Respawn(ItemSpawnPoint spawnPoint)
    {
        if (spawnPoint == null || itemPrefabs.Length == 0)
            return;

        int randomIndex = Random.Range(0, itemPrefabs.Length);

        GameObject newItem = Instantiate(
            itemPrefabs[randomIndex],
            spawnPoint.transform.position,
            Quaternion.identity
        );

        spawnPoint.SetItem(newItem);
    }
}