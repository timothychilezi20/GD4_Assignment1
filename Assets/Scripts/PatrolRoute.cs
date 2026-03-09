using UnityEngine;
using System.Collections.Generic;

public class PatrolRoute : MonoBehaviour
{
    [Header("Route Info")]
    public string routeName;

    [Header("Waypoints")]
    public List<Transform> waypoints = new List<Transform>();

    [Header("Route Behavior")]
    public bool loopRoute = true;
    public bool reverseRoute = false;

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count == 0)
            return;

        Gizmos.color = Color.blue;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] != null)
            {
                Gizmos.DrawSphere(waypoints[i].position, 0.3f);
            }

            if (i < waypoints.Count - 1 && waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }

        if (loopRoute && waypoints.Count > 2 && waypoints[0] != null && waypoints[waypoints.Count - 1] != null)
        {
            Gizmos.DrawLine(waypoints[waypoints.Count - 1].position, waypoints[0].position);
        }
    }
}