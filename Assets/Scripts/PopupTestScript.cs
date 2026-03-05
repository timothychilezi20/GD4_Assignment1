//using UnityEngine;
//using UnityEngine.InputSystem;

//public class PopupTestScript : MonoBehaviour
//{
//    private PlayerInput playerInput;

//    void Start()
//    {
//        // Try to get PlayerInput from this GameObject or find one
//        playerInput = GetComponent<PlayerInput>();

//        if (playerInput == null)
//        {
//            // Try to find any player input as fallback
//            playerInput = FindFirstObjectByType<PlayerInput>();
//        }

//        Debug.Log($"PopupTest: PlayerInput found = {playerInput != null}");
//    }

//    // Called by the Input System when the test action is performed
//    public void OnTestPopup(InputAction.CallbackContext context)
//    {
//        if (context.performed)
//        {
//            Debug.Log("=== MANUAL POPUP TEST (Input System) ===");
//            TriggerTestPopup();
//        }
//    }

//    // Alternative: Direct key check using Input System
//    void Update()
//    {
//        // This is for testing if the Input System method isn't working
//        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
//        {
//            Debug.Log("P key pressed via direct Keyboard check");
//            TriggerTestPopup();
//        }
//    }

//    void TriggerTestPopup()
//    {
//        // Check if ReputationPopupManager exists
//        if (ReputationPopupManager.Instance == null)
//        {
//            Debug.LogError("ReputationPopupManager.Instance is NULL!");
//            Debug.Log("   Make sure ReputationPopupManager exists in the scene");
//            return;
//        }

//        // Find Player 1
//        GameObject player = GameObject.FindGameObjectWithTag("Player1");
//        if (player == null)
//        {
//            Debug.LogError("❌ Player1 not found! Check tag");

//            // Try to find by name as fallback
//            player = GameObject.Find("Player1");
//            if (player != null)
//            {
//                Debug.Log("✓ Found Player1 by name");
//            }
//            else
//            {
//                return;
//            }
//        }

//        Debug.Log($"✓ Player1 found at position: {player.transform.position}");

//        // Show popup for Player 1
//        Debug.Log("Attempting to show popup...");
//        ReputationPopupManager.Instance.ShowPopup(
//            1,                              // playerNumber
//            NPCMovement.NPCGroup.Nerd,      // group
//            0.15f,                          // changeAmount (positive = green)
//            player.transform.position + Vector3.up * 2f  // position above player
//        );

//        Debug.Log("✓ ShowPopup called");
//    }

//    // Optional: Display instructions on screen
//    void OnGUI()
//    {
//        GUI.Box(new Rect(10, 10, 300, 60), "Popup Test Controls");
//        GUI.Label(new Rect(20, 35, 280, 20), "Press 'P' to test reputation popup");

//        // Show status
//        string status = "Status: ";
//        if (ReputationPopupManager.Instance == null)
//            status += "No Popup Manager";
//        else if (ReputationPopupManager.Instance.reputationPopupPrefab == null)
//            status += "No Prefab Assigned";
//        else if (ReputationPopupManager.Instance.worldSpaceCanvas == null)
//            status += "No Canvas Assigned";
//        else
//            status += "Ready";

//        GUI.Label(new Rect(20, 55, 280, 20), status);
//    }
//}
