using UnityEngine;
using System.Collections.Generic;

public class NPCGatherManager : MonoBehaviour
{
    public static NPCGatherManager Instance { get; private set; }

    [Header("Gather Settings")]
    [SerializeField] private float gatherSearchRadius = 12f;
    [SerializeField] private int maxStudentsToGather = 5;
    [SerializeField] private float gatherDuration = 5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AttractGroupToPlayer(GroupType1 targetGroup, PlayerController player)
    {
        if (player == null) return;

        StudentController[] allStudents = FindObjectsByType<StudentController>(FindObjectsSortMode.None);

        List<StudentController> matchingStudents = new List<StudentController>();

        foreach (StudentController student in allStudents)
        {
            if (student == null) continue;
            if (student.groupType != targetGroup) continue;

            float distance = Vector3.Distance(player.transform.position, student.transform.position);
            if (distance <= gatherSearchRadius)
            {
                matchingStudents.Add(student);
            }
        }

        matchingStudents.Sort((a, b) =>
            Vector3.Distance(player.transform.position, a.transform.position)
            .CompareTo(Vector3.Distance(player.transform.position, b.transform.position))
        );

        int count = Mathf.Min(maxStudentsToGather, matchingStudents.Count);

        for (int i = 0; i < count; i++)
        {
            matchingStudents[i].StartGathering(player.transform, gatherDuration);
        }

        UIManager.Instance?.ShowPlayerMessage(
            player.GetPlayerNumber(),
            $"{targetGroup}s noticed your item!",
            Color.cyan
        );
    }
}