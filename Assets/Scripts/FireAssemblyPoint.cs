using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class FireAssemblyPoint : MonoBehaviour
{
    [Header("Assembly Point Settings")]
    public string pointName;
    public float assemblyRadius = 5f;
    public int maxCapacity = 20;

    [Header("State")]
    public List<StudentController> studentsGathered = new List<StudentController>();
    public bool isActive = false;

    private void Reset()
    {
        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = assemblyRadius;
    }

    private void OnValidate()
    {
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
            col.radius = assemblyRadius;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isActive ? Color.red : Color.gray;
        Gizmos.DrawWireSphere(transform.position, assemblyRadius);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 2f,
            $"Students: {studentsGathered.Count}/{maxCapacity}"
        );
    }
#endif

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        StudentController student = other.GetComponent<StudentController>();
        if (student != null && !studentsGathered.Contains(student))
        {
            if (studentsGathered.Count < maxCapacity)
            {
                studentsGathered.Add(student);
                Debug.Log($"{student.name} arrived at {pointName}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        StudentController student = other.GetComponent<StudentController>();
        if (student != null)
        {
            studentsGathered.Remove(student);
        }
    }

    public void Activate()
    {
        isActive = true;
        studentsGathered.Clear();
    }

    public void Deactivate()
    {
        isActive = false;
        studentsGathered.Clear();
    }
}