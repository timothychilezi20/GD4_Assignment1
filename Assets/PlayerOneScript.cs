using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using System.Collections;

public class PlayerOneScript : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpStop = 1f;
    [SerializeField] private float rotateSpeed = 100f;
    private PlayerInput playerInput;

    //PICK UP SETTINGS 
    [SerializeField] private float interactRange = 10f;
    [SerializeField] private LayerMask interactLayer;
    private PlayerInventory inventory;  
    private Vector2 lookInput;
    private Vector2 moveInput;


    private void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        playerInput = GetComponent<PlayerInput>();
    }
    private void Update()
    {
        //Horizontal look rotates around Y (turn left/right)
        float y = lookInput.x * rotateSpeed * Time.deltaTime;
        transform.Rotate(0f, y, 0f, Space.World);

        Vector3 move3 = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed * Time.deltaTime;
        transform.position += move3; 
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump (InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        transform.position += Vector3.up * jumpStop; 
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if(!context.performed) return;  

        Ray ray = new Ray(transform.position + Vector3.up, transform.forward);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, interactRange, interactLayer))
        {
            

            if(hit.collider.TryGetComponent<Item>(out Item item))
            {
                inventory.PickUp(item);
            }

            else if(hit.collider.TryGetComponent<GroupReceiver>(out GroupReceiver group))
            {
                inventory.GiveItemToGroup(group);
            }

            else if(hit.collider.TryGetComponent<DumpingStation>(out DumpingStation station))
            {
                inventory.DumpBallots(station);
            }
        }
    }
}
