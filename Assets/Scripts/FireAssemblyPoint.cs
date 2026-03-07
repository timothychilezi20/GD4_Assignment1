using UnityEngine;
using System.Collections.Generic;

public class FireAssemblyPoint : MonoBehaviour
{
    [Header("Assembly Point Settings")]
    public string pointName;
    public float assemblyRadius;
    public int maxCapacity = 20;

    [Header("State")]
    public List<StudentController> studentsGathered = new List<StudentController>();
    public bool isActive = false; //active once fire alarm is rung

    private void OnDrawGizmos()
    {
        if (isActive)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.grey;

        Gizmos.DrawWireSphere(transform.position, assemblyRadius);

        // Draw capacity info
        if (Application.isPlaying)
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2,
                $"Students: {studentsGathered.Count}/{maxCapacity}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        StudentController student = other.GetComponent<StudentController>();
        if (student != null && !studentsGathered.Contains(student))
        {
            studentsGathered.Add(student);
            Debug.Log($"{student.name} arrived at {pointName}");
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
