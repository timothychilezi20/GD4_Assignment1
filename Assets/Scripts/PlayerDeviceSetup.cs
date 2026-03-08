using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDeviceSetup : MonoBehaviour
{
    private PlayerInput playerInput;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput.playerIndex == 0)
        {
            playerInput.SwitchCurrentControlScheme("KeyboardPlayer1", Keyboard.current);
        }
        else if (playerInput.playerIndex == 1)
        {
            playerInput.SwitchCurrentControlScheme("KeyboardPlayer2", Keyboard.current);
        }
    }
}