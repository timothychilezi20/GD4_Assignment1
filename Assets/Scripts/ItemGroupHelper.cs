using UnityEngine;

public static class ItemGroupHelper
{
    public static GroupType1 GetGroupForItem(ItemType item)
    {
        switch (item)
        {
            case ItemType.Basketball:
                return GroupType1.Athlete;

            case ItemType.Protractor:
                return GroupType1.Nerd;

            case ItemType.Paintbrush:
                return GroupType1.Artist;

            default:
                return GroupType1.Nerd;
        }
    }

    public static int GetVoteReward(ItemType item)
    {
        switch (item)
        {
            case ItemType.Basketball:
                return 10;

            case ItemType.Protractor:
                return 8;

            case ItemType.Paintbrush:
                return 8;

            default:
                return 5;
        }
    }
}