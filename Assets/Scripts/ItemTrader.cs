using UnityEngine;

public class ItemTrader : MonoBehaviour
{
    public int GetTradeValue(ItemType item)
    {
        return item switch
        {
            ItemType.RubiksCube => 15,
            ItemType.Football => 15,
            ItemType.PaintBrush => 15,
            ItemType.MathSet => 10,
            ItemType.CricketBat => 5,
            ItemType.Book => 3,
            ItemType.PaintTin => 5,
            _ => 0
        };
    }
}