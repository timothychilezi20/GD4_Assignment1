using UnityEngine;
using System.Collections.Generic;

public class WaypointZone : MonoBehaviour
{
    [SerializeField] private List<Transform> waypoints = new List<Transform>();

    public Transform GetRandomWaypoint()
    {
        if (waypoints.Count == 0)
            return null;

        int randomIndex = Random.Range(0, waypoints.Count);
        return waypoints[randomIndex];
    }

    public Transform GetWaypoint(int index)
    {
        if (index >= 0 && index < waypoints.Count)
            return waypoints[index];

        return null;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] != null)
            {
                Gizmos.DrawSphere(waypoints[i].position, 0.3f);

                if (i < waypoints.Count - 1 && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
        }
    }
}