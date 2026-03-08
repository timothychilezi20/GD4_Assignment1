using UnityEngine;

public class PlayerCameraSetup : MonoBehaviour
{
    [Header("Player Settings")]
    public Camera playerCamera;
    public int playerNumber = 1; // 1 or 2

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        if (playerCamera != null)
        {
            if (playerNumber == 1)
            {
                // Top half for Player 1
                playerCamera.rect = new Rect(0, 0.5f, 1, 0.5f);
            }
            else if (playerNumber == 2)
            {
                // Bottom half for Player 2
                playerCamera.rect = new Rect(0, 0, 1, 0.5f);
            }
        }
    }
}