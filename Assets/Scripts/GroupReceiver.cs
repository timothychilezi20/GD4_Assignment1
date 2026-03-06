using UnityEngine;

public class GroupReceiver : MonoBehaviour
{
    public GroupType groupType;
    public int packID;

    public int ReceiveItem(GroupType itemType)
    {
        if (itemType != groupType) return 0;

        int packSize = GetPackSize();
        return packSize;
    }

    private int GetPackSize()
    {
        Artists[] all = FindObjectsOfType<Artists>();
        int count = 0;  

        foreach(var artist in all)
        {
            if(artist.groupID == packID)
                count++;
        }

        Debug.Log($"Pack {packID} size: {count}");

        return count;
    }
}
