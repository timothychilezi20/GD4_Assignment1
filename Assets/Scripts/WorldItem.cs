using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public ItemType itemType;
    public bool isRareItem;
    public Sprite itemIcon;
    public string itemName;
    public GameObject itemVisualPrefab; 

    [TextArea(2, 3)]
    public string itemDescription;


}