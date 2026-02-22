using UnityEngine;

public class ArtSpawns : MonoBehaviour
{
   
    
        [Header("Spawn Settings")]
        public Transform[] spawnPoints; // Specific positions where NPCs can spawn at this hangout

        // Helper method to get a random spawn point from this hangout
        public Vector3 GetRandomSpawnPoint()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                // If no specific spawn points, use the hangout spot's position
                return transform.position;
            }

            int randomIndex = Random.Range(0, spawnPoints.Length);
            return spawnPoints[randomIndex].position;
        }
   
}
