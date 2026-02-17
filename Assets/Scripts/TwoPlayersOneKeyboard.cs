using UnityEngine;
using UnityEngine.InputSystem; 
public class TwoPlayersOneKeyboard : MonoBehaviour
{
    [Header("Actions (drag from ypur Input Actions asset)")]
    [SerializeField] private InputActionReference player1MoveAction;
    [SerializeField] private InputActionReference player2MoveAction;

    [Header("Players")]
    [SerializeField] private Transform p1;
    [SerializeField] private Transform p2;

    [SerializeField] private float moveSpeed = 5f;

    private void OnEnable()
    {
        player1MoveAction.action.Enable();
        player2MoveAction.action.Enable();
    }

    private void OnDisable()
    {
        player1MoveAction.action.Disable();
        player2MoveAction.action.Disable();
    }

    private void Update()
    {
       var m1 = player1MoveAction.action.ReadValue<Vector2>();
       var m2 = player2MoveAction.action.ReadValue<Vector2>();

        if (p1) p1.position += new Vector3(m1.x, 0, m1.y) * moveSpeed * Time.deltaTime;
        if (p2) p2.position += new Vector3(m2.x, 0, m2.y) * moveSpeed * Time.deltaTime;
    }
}
