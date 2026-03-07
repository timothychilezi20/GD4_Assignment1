using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.AI;

public class GroupController : MonoBehaviour
{
    [Header("Group Settings")]
    public GroupType1 groupType;
    public GameObject studentPrefab;
    public List<HangoutZone> hangoutPoints;
    public int totalStudents = 8;
    public int minPackSize = 2;
    public int maxPackSize = 4;

    [Header("Runtime Info")]
    public List<Pack> packs = new List<Pack>();

    private void Start()
    {
        SpawnPacks();
        InvokeRepeating("ManagePackBehaviors", 5f, 10f); //Change behaviors every 10 seconds
    }

    private void SpawnPacks()
    {
        int remainingStudents = totalStudents;
        int packCount = 0;

        while(remainingStudents > 0)
        {
            int packSize = Mathf.Min(Random.Range(minPackSize, maxPackSize + 1), remainingStudents);

            Pack newPack = new Pack();
            HangoutZone spawnHangout = hangoutPoints[Random.Range(0, hangoutPoints.Count)];
            
            newPack.currentHangout = spawnHangout;

            List<Vector3> spawnPositions = CalculateSpawnPositions(spawnHangout.transform.position, packSize, spawnHangout.zoneRadius * 0.3f);

            for (int i = 0; i < packSize; i++)
            {
                GameObject studentObj = Instantiate(studentPrefab, spawnPositions[i], Quaternion.identity, this.transform);

                StudentController student = studentObj.GetComponent<StudentController>();

                student.groupType = groupType;
                student.studentID = (packCount * 100) + i;
                student.currentPack = newPack;

                newPack.students.Add(student);
                remainingStudents--;
            }

            packs.Add(newPack);
            packCount++;
        }

        Debug.Log($"Spawned {packs.Count} packs for {groupType} group");
    }

    private List<Vector3> CalculateSpawnPositions(Vector3 center, int count, float radius)
    {
        List<Vector3> positions = new List<Vector3>();

        if(count <= 2)
        {
            positions.Add(center + Vector3.left * radius);
            if(count == 2) positions.Add(center + Vector3.right * radius);
        }
        else
        {
            float angleStep = 360f / count;
            for(int i = 0; i < count;i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

                positions.Add(center + offset);
            }
        }

        return positions;
    }

    private void ManagePackBehaviors()
    {
        foreach(Pack pack in packs)
        {
            Pack.PackBehavior newBehavior = (Pack.PackBehavior)Random.Range(0, 3);

            switch(newBehavior)
            {
                case Pack.PackBehavior.Wandering:
                    pack.currentBehavior = Pack.PackBehavior.Wandering;
                    break;

                case Pack.PackBehavior.Mingling:

                    pack.currentBehavior=Pack.PackBehavior.Mingling;
                    foreach(var students in pack.students)
                    {
                        students.GetComponent<NavMeshAgent>().ResetPath();
                    }
                    break;

                case Pack.PackBehavior.MovingToHangout:

                    HangoutZone newHangout = GetDifferentHangout(pack.currentHangout);

                    if (newHangout != null)
                    {
                        pack.currentBehavior = Pack.PackBehavior.MovingToHangout;
                        pack.targetHangout = newHangout;


                        for (int i = 0; i < pack.students.Count; i++)
                        {
                            Vector3 targetPos = newHangout.transform.position + (Random.insideUnitSphere * newHangout.zoneRadius * 0.5f);

                            targetPos.y = 0f;
                            pack.students[i].GetComponent<NavMeshAgent>().SetDestination(targetPos);
                        }

                        pack.currentHangout = newHangout;
                    }

                    break;   
            }
        }
    }

    private HangoutZone GetDifferentHangout (HangoutZone current) 
    {
        List<HangoutZone> otherHangouts = new List<HangoutZone>();

        foreach (HangoutZone hangout in hangoutPoints)
        {
            if (hangout != current)  // If this hangout is NOT the current one
            {
                otherHangouts.Add(hangout);  // Add it to our new list
            }
        }

        if (otherHangouts.Count > 0)
        {
            return otherHangouts[Random.Range(0, otherHangouts.Count)];
        }
        return null;
    }
}
