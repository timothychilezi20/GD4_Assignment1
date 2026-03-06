using UnityEngine;

public class TeacherSpot : MonoBehaviour
{
    public string spotName;
    public TeacherType preferredTeacher;
    public float minglingRadius = 3f;

    // Visualize in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minglingRadius);
    }
}

public enum TeacherType
{
    Art,
    Math,
    Coach
}
