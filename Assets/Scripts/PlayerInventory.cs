using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    [Header("References")]
    [SerializeField]private Transform holdPoint;
    [SerializeField]private Text ballotText;

    [Header("Inventory")]
    [SerializeField] private Item heldItem;
    [SerializeField] private int ballots;
    [SerializeField] private GroupType lastItemType; // Track ballot type

    private int PlayerNumber => GetComponent<PlayerController>().GetPlayerNumber();

    public int receivedballots;
    public GroupType ballotType;
    
    public int heldBallots;


    private void Start()
    {
        if (holdPoint == null) holdPoint = transform;
        if (ballotText == null)
            Debug.LogWarning("PlayerInventory: Assign ballotText!", this);
        UpdateBallotUI();
    }




    private void UpdateBallotUI()
    {
        if (ballotText != null)
            ballotText.text = $"Ballots: {ballots} ({lastItemType})";
    }

    public bool HasItem() => heldItem != null;
    public GroupType GetBallotType() => lastItemType;
    public int GetBallotCount() => ballots;



    public void PickUp(Item item)
    {
        if (heldItem != null)
        {
            Debug.Log($"Player {PlayerNumber}: Already holding item");
            return;
        }

        heldItem = item;
        lastItemType = item.groupType; //Tracks type of ballot
        item.OnPickedUp(holdPoint);
        Debug.Log($"Player {PlayerNumber}: Picked up {item.groupType}");
    }

    public void GiveItemToGroup(GroupReceiver group)
    {
        if (heldItem == null)
        {
            Debug.Log($"Player {PlayerNumber}: No item to give");
            return;
        }

        int received = group.ReceiveItem(heldItem.groupType);


        

        if (received > 0)
        {
            ballots += received;

            Debug.Log($"Player {PlayerNumber}: Gained {received} {heldItem.groupType} ballots (Total: {ballots})");

            Destroy(heldItem.gameObject);
            heldItem = null;
            UpdateBallotUI();
        }
    }

    public void DumpBallots(DumpingStation station)
    {
        if (heldBallots <= 0)
        {
            Debug.Log("No ballots to dump");
            return;
        }

        if (heldItem == null)
        {
            Debug.Log("Need item type to determine ballot type");
            return;
        }

        station.ReceiveBallots(heldBallots, heldItem.groupType);

        Debug.Log($"Dumped {heldBallots} ballots");

        heldBallots = 0;
        UpdateBallotUI();
    }
}
