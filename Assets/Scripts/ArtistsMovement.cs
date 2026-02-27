using UnityEngine;
using System.Collections.Generic;

public class ArtistsMovement : MonoBehaviour
{
    public GameObject[] artistsPrefabs;
    public Transform[] spawnPoint;
    public int numberOfArtistsToSpawn = 5;
    public float spawnRadius = 5f;

    private List<GameObject> spawnArtists = new List<GameObject>();


    void Start()
    {
       SpawnArtists();
    }

    void SpawnArtists()
    {
               for (int i = 0; i < numberOfArtistsToSpawn; i++)
        {
            int prefabIndex = Random.Range(0, artistsPrefabs.Length);

            GameObject artistPrefab = artistsPrefabs[prefabIndex];

            Transform spawnPointTransform = spawnPoint[Random.Range(0, spawnPoint.Length)];
            Vector3 spawnPosition = spawnPointTransform.position + Random.insideUnitSphere * spawnRadius;
            spawnPosition.y = spawnPointTransform.position.y;
            GameObject spawnedArtist = Instantiate(artistPrefab, spawnPosition, Quaternion.identity);
            spawnArtists.Add(spawnedArtist);
        }
    }
}
