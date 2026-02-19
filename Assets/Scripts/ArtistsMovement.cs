using UnityEngine;

public class ArtistsMovement : MonoBehaviour
{
    public GameObject[] artistsPrefabs;
    public Transform[] spawnPoint;
    public int numberOfArtistsToSpawn = 5;
    public float spawnRadius = 5f;
    void Start()
    {
        foreach (Transform point in spawnPoint)
        {
            for (int i = 0; i < numberOfArtistsToSpawn; i++)
            {
                int randomIndex = Random.Range(0, artistsPrefabs.Length);

                Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
                randomOffset.y = 0;

                Vector3 spawnPosition = point.position + randomOffset;

                Instantiate(artistsPrefabs[randomIndex], spawnPosition, Quaternion.identity);
            }
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
