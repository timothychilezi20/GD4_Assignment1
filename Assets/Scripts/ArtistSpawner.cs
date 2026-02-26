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
    [SerializeField] private float spawnRadius = 3f; // How far apart NPCs in same pack can spawn

    [Header("Formation Settings")]
    [SerializeField] private FormationType defaultFormation = FormationType.LooseCircle;  // students defulat formation is a loose circle
    [SerializeField] private float spacing = 2f;
    [SerializeField] private float formationDepth = 3f;

    [Header("Distribution Settings")]
    [SerializeField] private bool allowMutiplePacksPerSpot = false;
    [SerializeField] private float minDistanceBetweenaPacks = 5f; 

    [Header("Debug settings")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool visualizeFormation = true;
    [SerializeField] private float debugSphereRadius = 0.3f;
    [SerializeField] private Color[] packColors; //colors for the pack


    
    private List<GameObject> spawnedNPCs = new List<GameObject>();
    private Dictionary<int, List<Vector3>> packSpawnPositions = new Dictionary<int,List<Vector3>>();
    private Dictionary<int, FormationType> packFormations = new Dictionary<int, FormationType>();

    private List<UsedSpot> usedSpots = new List<UsedSpot>();

    // Class to track which spots are used and by whom

    class UsedSpot
    {
        public ArtSpawns spot;
        public int packID;
        public Vector3 centerPosition;
        public float radius;
    }


    enum FormationType //formations for students sitting
    { 
        Semicircle,
        Clump,
        Line, 
        LooseCircle
    }


    void Start()
    {
        SpawnArtsyKids();
    }

    void SpawnArtsyKids()
    {
        int remainingNPCS = totalNPCs;
        int packID = 0; //tracks different groups of artits
        usedSpots.Clear();

        //spawn artists till all 8 are spawned 
        while (remainingNPCS > 0)
        {
            int packSize = DeterminePackSize(remainingNPCS);

            ArtSpawns artsySpot = FindBestSpotForPack(packSize,packID); //find spots for packs



            if (artsySpot == null)
            {
                Debug.LogError("Cannot find suitable hangout spot for remaining students!");
                break;
            }

          
            FormationType usedFormation = defaultFormation; //store formations details 

            Vector3 basePosition = artsySpot.GetRandomSpawnPoint();

            usedSpots.Add(new UsedSpot { 
            spot = artsySpot,
            packID = packID,
            centerPosition = basePosition,
            radius = CalculateFormationRadius(packSize, usedFormation)
            
            });

            




            //spawn pack and get positions of packs
            List<Vector3> packPositions = SpawnPack(packSize, artsySpot, packID, usedFormation, basePosition);

            //store pack information for debugging 
            packSpawnPositions[packID] = packPositions;
            packFormations[packID] = usedFormation;

            if (enableDebugLogs)
            {
                Debug.Log($"=== PACK{packID} DEBUG ==="); //$ is used for string interpolation, making strings that use variables shorter
                Debug.Log($"Size:{packSize} students"); //tracks indiviual pack size
                Debug.Log($"Formation: {usedFormation}"); //gives location for pack
                Debug.Log($"Hangout Spot: {artsySpot.gameObject.name}"); 
                Debug.Log($"Base Position: {artsySpot.GetRandomSpawnPoint()}");
                Debug.Log($"Individual Positions:");

                for(int i = 0; i < packPositions.Count; i++)
                {
                    Debug.Log($" Student{i}: {packPositions[i]}");
                }

                //spacing between students 
                CheckSpacing(packPositions, packID);
            }

            //update counter of pack numbers 
            remainingNPCS -= packSize;
            packID++;

        }

        Debug.Log($"<color=green>SPAWN COMPLETE: {totalNPCs} artsy kids in {packID} packs/color>");
        Debug.Log($"Pack Formation Summary:");

        foreach(var pack in packFormations)
        {
            Debug.Log($" Pack{pack.Key}: {pack.Value} with {packSpawnPositions[pack.Key].Count} students");
        }

       
    }

    ArtSpawns FindBestSpotForPack(int packSize, int packID)
    {
        if (!allowMutiplePacksPerSpot)
        {
            //find used spots 
            foreach(ArtSpawns spot in artSpawns)
            {
                bool isUsed = false;
                foreach(UsedSpot used in usedSpots)
                {
                    if(used.spot == spot)
                    {
                        isUsed = true;
                        break;
                    }
                }

                if (!isUsed)
                {
                    return spot;
                }
            }
            return null;
        }

        else
        {
            List<ArtSpawns> suitableSpots = new List<ArtSpawns>();

            foreach(ArtSpawns spot in artSpawns)
            {
                Vector3[] possiblePositions = spot.GetAllSpawnPoints();

                foreach(Vector3 positions in possiblePositions)
                {
                    bool tooClose = false;

                    foreach(UsedSpot used in usedSpots)
                    {
                        float distance = Vector3.Distance(positions, used.centerPosition);

                        float minDistance = used.radius + CalculateFormationRadius(packSize, defaultFormation) + minDistanceBetweenaPacks;

                        if(distance < minDistance)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (!tooClose)
                    {
                        suitableSpots.Add(spot);
                        break;
                    }
                }
                
            }

            if(suitableSpots.Count > 0)
            {
                return suitableSpots[Random.Range(0, suitableSpots.Count)];
            }
            return null;
        }
    }

    float CalculateFormationRadius(int packSize, FormationType formation)
    {
        switch (formation) 
        {
            case FormationType.LooseCircle:
                return spacing * 1.2f;

            case FormationType.Clump:
                return spacing;

            case FormationType.Line:
                return (packSize * spacing) / 2f;

            case FormationType.Semicircle:
                return spacing;

            default:
             return spacing;

        }

    }

   

    void CheckSpacing(List<Vector3> positions, int packID)
    {
        for(int i = 0; i < positions.Count;i++)
        {
            for (int j = i + 1; j < positions.Count; j++)
            {
                float distance = Vector3.Distance(positions[i], positions[j]);
                if (distance < 0.5f)
                {
                    Debug.LogWarning($"Pack {packID}: Students {i} and {j} are very close! Distance: {distance}");
                }
                else if(enableDebugLogs)
                {
                    Debug.Log($"Pack {packID}: Distance between student {i} and {j}:{distance:F2}");
                }
            }
            
        }
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

    List<Vector3> SpawnPack(int packSize, ArtSpawns artSpawn, int packID, FormationType formation, Vector3 basePosition)
    {
       


        //the direction the spawnned artists face
        Vector3 forward = artSpawn.transform.forward;
        Vector3 right = artSpawn.transform.right;

        List<Vector3> spawnPositions = new List<Vector3>();

        switch (formation)
        {
            case FormationType.LooseCircle:
                spawnPositions = GenerateLooseCircle(packSize, basePosition);
                if (enableDebugLogs) Debug.Log($"Pack {packID}: Using Loose Circle Formation");
                break;
            case FormationType.Clump:
                spawnPositions = GenerateClump(packSize, basePosition);
                if (enableDebugLogs) Debug.Log($"Pack {packID}: Using Clump formation");
                break;
            case FormationType.Line:
                spawnPositions = GenerateLine(packSize, basePosition, right);
                if (enableDebugLogs) Debug.Log($"Pack {packID}:  Using Line Formation");
                break;
            case FormationType.Semicircle:
                spawnPositions = GenerateSemiCircle(packSize, basePosition, forward, right);
                if (enableDebugLogs) Debug.Log($"Pack: {packID}: Using SemiCircle formation");
                break;
        }

        //Spawn artists at calculated positions
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            GameObject artist = Instantiate(artsyNPCPrefab, spawnPositions[i], Quaternion.identity);

            Artists artistsScript = artist.GetComponent<Artists>();
            if (artistsScript != null)
            {
                artistsScript.groupID = packID;
            }

            spawnedNPCs.Add(artist);
        }

        return spawnPositions;

    }


     List<Vector3> GenerateLooseCircle(int count, Vector3 center)
        {
            List<Vector3> positions = new List<Vector3>();
            float angleStep = 360f / count;

            for (int i = 0; i < count; ++i)
            {
                float angle = i *angleStep * Mathf.Deg2Rad;

                //Vary the radius slightly for each NPC
                float variedSpacing = spacing * Random.Range(0.8f, 1.2f);

                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * variedSpacing, 0,
                    Mathf.Sin(angle) * variedSpacing
                    );

               positions.Add(center + offset); // positions of each student in their respective pack


            }

            return positions;
        }

        List<Vector3> GenerateClump( int count, Vector3 center)
        {
           List<Vector3> positions = new List<Vector3>();

            for(int i = 0; i < count; ++i)
            {
                //this generates the pack in a loose cluster
                float distance = Random.Range(spacing * 0.5f, spacing);
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * distance, 0,
                    Mathf.Sin(angle) * distance);

                positions.Add(center + offset);
            }

            return positions;
        }

        List<Vector3> GenerateLine(int count, Vector3 start, Vector3 direction)
        {
            List<Vector3> positions = new List<Vector3>();

            float totalWidth = (count - 1) * spacing;
            float startOffset = -totalWidth / 2f;

            for(int i = 0; i < count; i++)
            {
                Vector3 offset = direction *(startOffset + i * spacing);
                positions.Add(start + offset);
            }
            return positions;
        }

        List<Vector3> GenerateSemiCircle(int count, Vector3 center, Vector3 forward, Vector3 right)
        {
            List<Vector3> positions = new List<Vector3>();
            float angleStep = 180f/(count - 1); // 180 degrees for the semi circle 

            for(int i = 0; i < count; i++) 
            {
                float angle = (i * angleStep - 90f) * Mathf.Deg2Rad;

                Vector3 offset = (right * Mathf.Sin(angle) + forward * Mathf.Cos(angle)) * spacing;

                Vector3 finalPos = center + offset;

                finalPos += new Vector3(
                    Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));

            positions.Add(finalPos);


            }
            return positions;
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
