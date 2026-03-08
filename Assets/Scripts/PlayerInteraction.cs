//using UnityEngine;
//using UnityEngine.InputSystem;
//using UnityEngine.InputSystem.XR;

//public class PlayerInteraction : MonoBehaviour
//{
//    public int PlayerNumber { get; set; }
//    public float interactionRange = 3f;
//    public LayerMask interactableLayer;

//    private GameObject nearbyInteractable;

//    private PlayerController player; 

//    void Update()
//    {
//        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer);
//        nearbyInteractable = null;
//        float closest = Mathf.Infinity;

//        foreach (var hit in hits)
//        {
//            float dist = Vector3.Distance(transform.position, hit.transform.position);
//            if (dist < closest)
//            {
//                closest = dist;
//                nearbyInteractable = hit.gameObject;
//            }
//        }
//    }

//    public void OnInteract(InputAction.CallbackContext ctx)
//    {
//        if (!ctx.performed || nearbyInteractable == null) return;

//        if (nearbyInteractable.TryGetComponent<DroppedVotes>(out var votes)) votes.PickUp();
//        else if (nearbyInteractable.TryGetComponent<WorldItem>(out var item)) item.TryPickUp(GetComponent<PlayerController>());
//        else if (nearbyInteractable.TryGetComponent<NPCMovement>(out var npc)) npc.TryTrade(GetComponent<PlayerController>());
//    }

//    private void TryInteract()
//    {
//        Collider[] hits = Physics.OverlapSphere(transform.position, 2f);

//        foreach (Collider hit in hits)
//        {
//            Ballot ballot = hit.GetComponent<Ballot>();

//            if (ballot != null)
//            {
//                PickUpBallot(ballot);
//                return;
//            }
//        }
//    }

//    void PickUpBallot(Ballot ballot)
//    {
//        if (player.itemHoldPoint.childCount > 0)
//            return; // already holding something

//        ballot.PickUp(player.itemHoldPoint);
//    }


//}