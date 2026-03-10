using UnityEngine;

public static class ItemGroupHelper
{
    public static GroupType1 GetGroupForItem(ItemType item)
    {
        switch (item)
        {
            case ItemType.Football:
                return GroupType1.Athlete;

            case ItemType.CricketBat:
                return GroupType1.Athlete;

            case ItemType.MathSet:
                return GroupType1.Nerd;

            case ItemType.RubiksCube:
                return GroupType1.Nerd;

            case ItemType.PaintBrush:
                return GroupType1.Artist;

            case ItemType.PaintTin:
                return GroupType1.Artist;

            default:
                return GroupType1.Nerd;
        }
    }

    public static int GetVoteReward(ItemType item)
    {
        switch (item)
        {
            case ItemType.Football:
                return 10;

            case ItemType.CricketBat:
                return 5;

            case ItemType.MathSet:
                return 8;

            case ItemType.RubiksCube:
                return 15;

            case ItemType.PaintTin:
                return 10;

            case ItemType.PaintBrush:
                return 8;

            default:
                return 5;
        }
    }
}