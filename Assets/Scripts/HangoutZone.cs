using UnityEngine;
using System.Collections.Generic;

public class HangoutZone : MonoBehaviour
{
    public string zoneName;
    public GroupType1 groupType;
    public float zoneRadius = 10f;
    public List<Transform> wanderPoints;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, zoneRadius);
    }

}

public enum GroupType1
{
    Athlete,
    Nerd,
    Artist
}
