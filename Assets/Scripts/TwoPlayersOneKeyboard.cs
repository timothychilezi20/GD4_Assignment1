using UnityEngine;
using UnityEngine.InputSystem; 
public class TwoPlayersOneKeyboard : MonoBehaviour
{
    [Header("Actions (drag from ypur Input Actions asset)")]
    [SerializeField] private InputActionReference player1MoveAction;
    [SerializeField] private InputActionReference player2MoveAction;
    [SerializeField] private InputActionReference jump1; 
    [SerializeField] private InputActionReference jump2;
    [SerializeField] private float jumpForce = 5f;
    //[SerializeField] public bool isGrounded;
    Rigidbody rb; 

    [Header("Players")]
    [SerializeField] private Transform p1;
    [SerializeField] private Transform p2;
    [SerializeField] private Rigidbody p1RB;
    [SerializeField] private Rigidbody p2RB;

    [SerializeField] private float moveSpeed = 5f;

    private void Start()
    {
        
    }

    private void OnEnable()
    {
        player1MoveAction.action.Enable();
        player2MoveAction.action.Enable();
        jump1.action.Enable();
        jump1.action.performed += OnJumpP1;
        jump2.action.performed += OnJumpP2;
        jump2.action.Enable();
    }

    private void OnDisable()
    {
        player1MoveAction.action.Disable();
        player2MoveAction.action.Disable();
        jump1.action.Disable();
        jump1.action.performed -= OnJumpP1;
        jump2.action.performed -= OnJumpP2; 
        jump2.action.Disable();
    }

    private void Update()
    {
       var m1 = player1MoveAction.action.ReadValue<Vector2>();
       var m2 = player2MoveAction.action.ReadValue<Vector2>();

        if (p1) p1.position += new Vector3(m1.x, 0, m1.y) * moveSpeed * Time.deltaTime;
        if (p2) p2.position += new Vector3(m2.x, 0, m2.y) * moveSpeed * Time.deltaTime;



    }

    public void OnJumpP1(InputAction.CallbackContext context)
    {
        p1RB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); 
    }

    public void OnJumpP2(InputAction.CallbackContext context)
    {
        p2RB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

}
