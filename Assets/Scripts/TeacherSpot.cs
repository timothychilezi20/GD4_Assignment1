using UnityEngine;

public class TeacherSpot : MonoBehaviour
{
    [Header("Spot Info")]
    public string spotName;

    [Header("Mingling Settings")]
    public float minglingRadius = 3f;

    [Header("Special Spots")]
    public bool isAssemblySpot = false;
    public bool isStaffRoomSpot = false;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minglingRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.25f);
    }
}