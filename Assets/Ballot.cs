using UnityEngine;

public class Ballot : MonoBehaviour
{
    [SerializeField] private int voteValue = 5;

    public int PickUp()
    {
        int amount = voteValue;
        Destroy(gameObject);
        return amount;
    }
}