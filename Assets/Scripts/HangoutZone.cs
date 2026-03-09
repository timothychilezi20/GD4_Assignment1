using UnityEngine;
using System.Collections.Generic;

public class HangoutZone : MonoBehaviour
{
    [Header("Zone Info")]
    public string zoneName;
    public GroupType1 groupType;

    [Header("Zone Settings")]
    public float zoneRadius = 10f;

    [Header("Optional Wander Points")]
    public List<Transform> wanderPoints = new List<Transform>();

    public Vector3 GetRandomPoint()
    {
        if (wanderPoints != null && wanderPoints.Count > 0)
        {
            int index = Random.Range(0, wanderPoints.Count);
            return wanderPoints[index].position;
        }

        Vector3 randomDir = Random.insideUnitSphere * zoneRadius;
        randomDir.y = 0;

        return transform.position + randomDir;
    }

    public Vector3 GetCenterPosition()
    {
        return transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, zoneRadius);

        if (wanderPoints != null)
        {
            Gizmos.color = Color.cyan;

            foreach (var point in wanderPoints)
            {
                if (point != null)
                    Gizmos.DrawSphere(point.position, 0.3f);
            }
        }
    }
}

public enum GroupType1
{
    Athlete,
    Nerd,
    Artist
}