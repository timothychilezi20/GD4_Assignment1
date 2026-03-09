using UnityEngine;
using System.Collections.Generic;
public class TeacherGroupController : MonoBehaviour
{
    [Header("Teacher Prefabs")]
    public GameObject artTeacherPrefab;
    public GameObject mathTeacherPrefab;
    public GameObject coachTeacherPrefab;

    [Header("Spawn Points")]
    public Transform artTeacherSpawn;
    public Transform mathTeacherSpawn;
    public Transform coachTeacherSpawn;

    [Header("Teacher Routes ")]
    public List<PatrolRoute> artPatrolRoutes;
    public List<PatrolRoute> mathPatrolRoutes;
    public List<PatrolRoute> coachPatrolRoutes;

    [Header("Teacher Spots")]
    public List<TeacherSpot> artTeacherSpots;
    public List<TeacherSpot> mathTeacherSpots;
    public List<TeacherSpot> coachTeacherSpots;

    [Header("Runtime Teachers")]
    public TeacherController artTeacher;
    public TeacherController mathTeacher;
    public TeacherController coachTeacher;



    private void Start()
    {
        SpawnAllTeachers();
    }

    private void SpawnAllTeachers()
    {
        if (artTeacherPrefab != null && artTeacherSpawn != null)
        {
            GameObject artObj = Instantiate(artTeacherPrefab, artTeacherSpawn.position, Quaternion.identity);
            artTeacher = artObj.GetComponent<TeacherController>();

            artTeacher.teacherType = (TeacherController.TeacherType)TeacherType.Art;

            artTeacher.availableRoutes = artPatrolRoutes;
            artTeacher.assignedSpots = artTeacherSpots;
        }

        if (mathTeacherPrefab != null && mathTeacherSpawn != null)
        {
            GameObject mathObj = Instantiate(mathTeacherPrefab, mathTeacherSpawn.position, Quaternion.identity);
            mathTeacher = mathObj.GetComponent<TeacherController>();
            mathTeacher.teacherType = (TeacherController.TeacherType)TeacherType.Math;

            mathTeacher.availableRoutes = mathPatrolRoutes;
            mathTeacher.assignedSpots = mathTeacherSpots;
        }
        if (coachTeacherPrefab != null && coachTeacherSpawn != null)
        {
            GameObject coachObj = Instantiate(coachTeacherPrefab, coachTeacherSpawn.position, Quaternion.identity);
            coachTeacher = coachObj.GetComponent<TeacherController>();
            coachTeacher.teacherType = (TeacherController.TeacherType)TeacherType.Coach;

            coachTeacher.availableRoutes = coachPatrolRoutes;
            coachTeacher.assignedSpots = coachTeacherSpots;
        }
    }


}
