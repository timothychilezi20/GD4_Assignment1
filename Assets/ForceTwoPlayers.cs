using UnityEngine;
using UnityEngine.InputSystem;

public class ForceTwoPlayers : MonoBehaviour
{
    void Start()
    {
        var manager = Object.FindFirstObjectByType<PlayerInputManager>();

        if (PlayerInput.all.Count < 2)
        {
            manager.JoinPlayer();
            manager.JoinPlayer();
        }
    }
}