using UnityEngine;
using System.Collections.Generic;

public class ArtistGroupCoordinater : MonoBehaviour
{
    [Header("Group Movement")]
    [SerializeField] private float travelCooldown = 60f;
    [SerializeField] private float regroupTime = 15f;


    private Dictionary<int, float> lastTravelTime = new Dictionary<int, float>();
    private Dictionary<int, bool> groupTraveling = new Dictionary<int, bool>();
    private Dictionary<int, ArtSpawns> groupTargets = new Dictionary<int, ArtSpawns>();

    public bool CanGroupTravel(int groupID)
    {
        if (!lastTravelTime.ContainsKey(groupID))
        {
            lastTravelTime[groupID] = -travelCooldown;
            return true;
        }

        return Time.time - lastTravelTime[groupID] >= travelCooldown;
    }

    public void GroupStartedTravel(int groupID)
    {
        lastTravelTime[groupID] = Time.time;
        groupTraveling[groupID] = true;
    }

    public void GroupArrived(int groupID)
    {
        groupTraveling[groupID] = false;
    }

    public bool IsGroupTraveling(int groupID)
    {
        return groupTraveling.ContainsKey(groupID) && groupTraveling[groupID];
    }

    public void SetGroupTarget(int groupID, ArtSpawns target)
    {
        groupTargets[groupID] = target;
    }

    public ArtSpawns GetGroupTarget(int groupID)
    {
        if(groupTargets.ContainsKey(groupID))
            return groupTargets[groupID];

        return null;
    }

}
