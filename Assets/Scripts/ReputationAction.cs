using UnityEngine;

[CreateAssetMenu(fileName = "ReputationActions", menuName = "Reputation/Actions")]
public class ReputationActions : ScriptableObject
{
    [System.Serializable]
    public class ReputationModifier
    {
        public NPCMovement.NPCGroup group;
        public float changeAmount;
    }

    [Header("Dash Actions")]
    public ReputationModifier[] dashModifiers;

    [Header("Rare Item Actions")]
    public ReputationModifier[] rareItemModifiers;

    [Header("Food Actions")]
    public ReputationModifier[] foodModifiers;

    [Header("Interaction Actions")]
    public ReputationModifier[] interactionModifiers;

    public float GetModifier(string actionType, NPCMovement.NPCGroup group)
    {
        ReputationModifier[] modifiers = actionType switch
        {
            "dash" => dashModifiers,
            "rareItem" => rareItemModifiers,
            "food" => foodModifiers,
            "interact" => interactionModifiers,
            _ => null
        };

        if (modifiers != null)
        {
            foreach (var mod in modifiers)
            {
                if (mod.group == group)
                    return mod.changeAmount;
            }
        }
        return 0f;
    }
}