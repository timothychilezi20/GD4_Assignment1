using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
public class PatrolRoute : MonoBehaviour
{
    public string routeName;
    public List<Transform> waypoints = new List<Transform>();

    public bool loopRoute = true;

    public bool reverseRoute = false;

    // Visualize route in editor
    private void OnDrawGizmos()
    {
        if (waypoints.Count < 2) return;

        Gizmos.color = Color.blue;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                Gizmos.DrawSphere(waypoints[i].position, 0.3f);
            }
        }

        // Draw last to first if looping
        if (loopRoute && waypoints.Count > 2)
        {
            Gizmos.DrawLine(waypoints[waypoints.Count - 1].position, waypoints[0].position);
        }
        Gizmos.DrawSphere(waypoints[waypoints.Count - 1].position, 0.3f);
    }
}

     


