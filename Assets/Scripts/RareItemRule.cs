using UnityEngine;

public class RareItemRule : MonoBehaviour
{
    [Header("Rare Item Rule")]
    [SerializeField] private bool isRare = true;
    [SerializeField] private NPCMovement.NPCGroup targetGroup = NPCMovement.NPCGroup.Athlete;
    [SerializeField] private float reputationChange = 0.2f;

    public bool IsRare => isRare;
    public NPCMovement.NPCGroup TargetGroup => targetGroup;
    public float ReputationChange => reputationChange;
}