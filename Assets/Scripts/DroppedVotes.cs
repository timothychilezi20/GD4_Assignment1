using System;
using UnityEngine;

public class DroppedVotes : MonoBehaviour
{
    private int voteAmount;
    private float lifetime = 5f;

    public void Initialize(int amount, int playerNumber)
    {
        voteAmount = amount;
        Destroy(gameObject, lifetime);
    }

    public int PickUp()
    {
        Destroy(gameObject);
        return voteAmount;
    }

    internal void Initialize(int heldVotes)
    {
        throw new NotImplementedException();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.AddVotes(voteAmount);
                Destroy(gameObject);
            }
        }
    }
}