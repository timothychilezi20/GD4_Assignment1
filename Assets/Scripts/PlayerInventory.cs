//using UnityEngine;

//public class PlayerInventory : MonoBehaviour
//{
//    public Transform holdPoint;

//    private Ballot heldBallot;

//    public bool HasBallot()
//    {
//        return heldBallot != null;
//    }

//    public void PickUpBallot(Ballot ballot)
//    {
//        if (heldBallot != null) return;

//        heldBallot = ballot;
//        ballot.PickUp(holdPoint);
//    }

//    public int DepositBallot()
//    {
//        if (heldBallot == null) return 0;

//        int value = heldBallot.Deposit();
//        heldBallot = null;

//        return value;
//    }

//    public void DropBallot(Vector3 dropPosition)
//    {
//        if (heldBallot == null) return;

//        heldBallot.transform.SetParent(null);

//        Rigidbody rb = heldBallot.GetComponent<Rigidbody>();
//        if (rb != null)
//        {
//            rb.isKinematic = false;
//            rb.useGravity = true;
//        }

//        heldBallot.transform.position = dropPosition;

//        heldBallot = null;
//    }
//}