using UnityEngine;

public class ItemTrader : MonoBehaviour
{
    public int GetTradeValue(ItemType item)
    {
        return item switch
        {
            ItemType.Protractor => 15,
            ItemType.Basketball => 15,
            ItemType.Paintbrush => 15,
            ItemType.Apple => 10,
            ItemType.PrankKit => 5,
            ItemType.Food => 3,
            ItemType.Book => 5,
            ItemType.Pen => 1,
            _ => 0
        };
    }
}