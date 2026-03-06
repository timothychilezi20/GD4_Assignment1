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

            artTeacher.teacherType = TeacherType.Art;
        }

        if(mathTeacherPrefab != null && mathTeacherSpawn != null)
        {
            GameObject mathObj = Instantiate(mathTeacherPrefab, mathTeacherSpawn.position, Quaternion .identity);
            mathTeacher = mathObj.GetComponent<TeacherController>();
            mathTeacher.teacherType = TeacherType.Math;
        }
        if(coachTeacherPrefab != null && coachTeacherSpawn != null)
        {
            GameObject coachObj = Instantiate(coachTeacherPrefab, coachTeacherSpawn.position, Quaternion.identity);
                coachTeacher = coachObj.GetComponent<TeacherController>();
            coachTeacher.teacherType = TeacherType.Coach;
        }
    }


}
