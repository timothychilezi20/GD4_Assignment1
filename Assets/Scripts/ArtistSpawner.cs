using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ArtistSpawner : MonoBehaviour
{
    [Header("NPC Prefab")]
    [SerializeField] private GameObject artsyNPCPrefab; // Drag your NPC prefab here

    [Header("Hangout Spots")]
    [SerializeField] private ArtSpawns[] artSpawns; // Array of 3 artspawn spots

    [Header("Spawn Settings")]
    [SerializeField] private int totalNPCs = 8; // Total number of artsy kids
    [SerializeField] private int minPackSize = 2; // Minimum NPCs per pack
    [SerializeField] private int maxPackSize = 4; // Maximum NPCs per pack

    [Header("Spawning Area")]
    [SerializeField] private float spawnRadius = 1f; // How far apart NPCs in same pack can spawn

    private List<GameObject> spawnedNPCs = new List<GameObject>();

    void Start()
    {
        SpawnArtsyKids();
    }

    void SpawnArtsyKids()
    {
        int remainingNPCS = totalNPCs;
        int packID = 0; //tracks different groups of artits

        //spawn artists till all 8 are spawned 
        while (remainingNPCS > 0)
        {
            int packSize = DeterminePackSize(remainingNPCS);

            //where each pack of artist is going to spawn
            ArtSpawns artsySpot = artSpawns[Random.Range(0, artSpawns.Length)];

            //spawn pack 
            SpawnPack(packSize, artsySpot, packID);

            //update counter of pack numbers 
            remainingNPCS -= packSize;
            packID++;

        }

        Debug.Log($"Spawned {totalNPCs} artsy kids in {packID} packs");
    }

    int DeterminePackSize(int remaining)
    {
        // Calculate maximum pack size possible based on remaining NPCs
        int maxPossible = Mathf.Min(maxPackSize, remaining);

        //Makes sure last pack is still above 2 

        if(remaining - minPackSize < minPackSize && remaining > minPackSize)
        {
            //if the remaining amount of the artists is less than the min pack then jut use the remaining
            return remaining;
        }

        // Randomly choose pack size between 2 and 4
        return Random.Range(minPackSize, maxPossible + 1);
    }

    void SpawnPack(int packSize, ArtSpawns artSpawn, int packID)
    {
        //get spawn positions for each pack 
        Vector3 basePosition = artSpawn.GetRandomSpawnPoint();

        for(int i = 0; i < packSize; i++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnRadius, spawnRadius), 0, Random.Range(-spawnRadius, spawnRadius));

            //Calculate spawn position 

            Vector3 spawnPosition = basePosition + randomOffset;

            //Spawn Artists prefabs 
            GameObject artist = Instantiate(
                artsyNPCPrefab, spawnPosition, Quaternion.identity);

            //keep the spawn in a list of all spawnned artists
            spawnedNPCs.Add(artist);

            Artists artistsScript = artist.GetComponent<Artists>();
            if( artistsScript != null )
            {
                artistsScript.groupID = packID;
            }
        }
    }

    //visualization
    private void OnDrawGizmosSelected()
    {
        if (artSpawns == null) return;

        Gizmos.color = Color.cyan;

        foreach(ArtSpawns artSpawn in  artSpawns)
        {
            if( artSpawn != null)
            {
                //sphere at each spawn point 
                if(artSpawn.spawnPoints != null)
                {
                    foreach(Transform spawnPoint in artSpawn.spawnPoints)
                    {
                        if(spawnPoint != null)
                        {
                            Gizmos.DrawWireSphere(spawnPoint.position, spawnRadius);
                        }
                    }
                }

                else
                {
                    Gizmos.DrawWireSphere(artSpawn.transform.position, spawnRadius);

                }
            }
        }
    }
}
