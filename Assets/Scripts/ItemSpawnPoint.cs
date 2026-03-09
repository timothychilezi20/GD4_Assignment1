using UnityEngine;

public class ItemSpawnPoint : MonoBehaviour
{
    public GameObject currentItem;

    public bool IsOccupied()
    {
        return currentItem != null;
    }

    public void SetItem(GameObject item)
    {
        currentItem = item;
    }

    public void ClearItem()
    {
        currentItem = null;
    }

}