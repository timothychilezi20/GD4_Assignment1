using NUnit.Framework;
using System.Collections.Generic;
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

    public Vector3[] GetAllSpawnPoints()
    {

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return new Vector3[] { transform.position };

        }

        Vector3[] positions = new Vector3[spawnPoints.Length];
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            positions[i] = spawnPoints[i].position;
        }

        return positions;

    }

    public bool IsPositionAvailable(Vector3 position, float requiredRadius, List<Vector3> usedPositions)
    {
        foreach (Vector3 used in usedPositions)
        {
            if (Vector3.Distance(position, used) < requiredRadius * 2)
            {
                return false;
            }


        }
        return true;

    }
    }

        
