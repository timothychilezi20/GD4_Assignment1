using UnityEngine;

public class WaypointZone : MonoBehaviour
{
   public Transform[] waypoints;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        if (waypoints == null) return;

        foreach (Transform point in waypoints)
        {
            if (point != null)
            {
                Gizmos.DrawSphere(point.position, 0.5f);
            }
        }
    }
}
