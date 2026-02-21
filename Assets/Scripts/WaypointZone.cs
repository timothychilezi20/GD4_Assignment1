using UnityEngine;

public class WaypointZone : MonoBehaviour
{
    public Transform[] waypoints;

    public Transform GetRandomWaypoint()
    {
        if (waypoints.Length == 0)
            return null;

        return waypoints[Random.Range(0, waypoints.Length)];
    }
}