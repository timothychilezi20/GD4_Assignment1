using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Player Prefab")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform player1Spawn;
    [SerializeField] private Transform player2Spawn;

    [Header("Player Materials/Colors")]
    [SerializeField] private Material player1Material;
    [SerializeField] private Material player2Material;

    [Header("Input Settings")]
    [SerializeField] private InputActionAsset inputActions; // Your Gameplay.inputactions asset

    [Header("Player Names")]
    [SerializeField] private string player1Name = "Player 1";
    [SerializeField] private string player2Name = "Player 2";

    void Start()
    {
        Debug.Log("=== PlayerSpawner Starting ===");
        ValidateAssignments();
        SpawnAllPlayers();
    }

    void ValidateAssignments()
    {
        if (playerPrefab == null)
            Debug.LogError("Player Prefab is not assigned!");

        if (player1Spawn == null || player2Spawn == null)
            Debug.LogError("Spawn points are not assigned!");

        if (inputActions == null)
            Debug.LogError("Input Actions asset is not assigned!");
        else
            Debug.Log($"Input Actions Asset: {inputActions.name}");
    }

    void SpawnAllPlayers()
    {
        SpawnPlayer1();
        SpawnPlayer2();

        StartCoroutine(LogDeviceInfo());
    }

    void SpawnPlayer1()
    {
        GameObject player = Instantiate(playerPrefab, player1Spawn.position, player1Spawn.rotation);
        player.name = player1Name;
        player.tag = "Player1";

        // Configure PlayerController
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SetPlayerNumber(1);
            controller.SetPlayerName(player1Name);
            Debug.Log("Player 1 controller configured");
        }
        else
        {
            Debug.LogError("PlayerController not found on Player 1 prefab!");
        }

        // Configure PlayerInput
        SetupPlayerInput(player, 1);

        // Apply visual material
        ApplyPlayerVisuals(player, player1Material);
    }

    void SpawnPlayer2()
    {
        GameObject player = Instantiate(playerPrefab, player2Spawn.position, player2Spawn.rotation);
        player.name = player2Name;
        player.tag = "Player2";

        // Configure PlayerController
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SetPlayerNumber(2);
            controller.SetPlayerName(player2Name);
            Debug.Log("Player 2 controller configured");
        }
        else
        {
            Debug.LogError("PlayerController not found on Player 2 prefab!");
        }

        // Configure PlayerInput
        SetupPlayerInput(player, 2);

        // Apply visual material
        ApplyPlayerVisuals(player, player2Material);
    }

    void SetupPlayerInput(GameObject player, int playerNumber)
    {
        PlayerInput playerInput = player.GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError($"PlayerInput component missing on {player.name} prefab!");
            return;
        }

        // Set up the Player Input component
        playerInput.actions = inputActions;
        playerInput.defaultActionMap = "Gameplay"; // Must match your action map name
        playerInput.notificationBehavior = PlayerNotifications.SendMessages;

        // IMPORTANT: Set Default Scheme to None (let Unity auto-assign)
        // In code, we don't set defaultScheme - it remains null/None

        Debug.Log($"Player {playerNumber} PlayerInput configured");
        Debug.Log($"- Actions: {playerInput.actions?.name}");
        Debug.Log($"- Default Map: {playerInput.defaultActionMap}");

        // Log available devices
        Debug.Log($"Available devices: {InputSystem.devices.Count}");
        foreach (var device in InputSystem.devices)
        {
            Debug.Log($"  - {device.name} ({device.deviceId})");
        }
    }

    void ApplyPlayerVisuals(GameObject player, Material material)
    {
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            Debug.LogWarning($"No renderers found on {player.name}! Cannot apply color.");
            return;
        }

        foreach (Renderer renderer in renderers)
        {
            if (material != null)
            {
                renderer.material = material;
                Debug.Log($"Applied material to {player.name}");
            }
        }
    }

    IEnumerator LogDeviceInfo()
    {
        // Wait a moment for input system to fully initialize
        yield return new WaitForSeconds(0.5f);

        // Find and log info for both players
        GameObject player1 = GameObject.FindGameObjectWithTag("Player1");
        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");

        LogPlayerInputInfo(player1, "Player 1");
        LogPlayerInputInfo(player2, "Player 2");
    }

    void LogPlayerInputInfo(GameObject player, string playerName)
    {
        if (player == null)
        {
            Debug.LogError($"Could not find {playerName}!");
            return;
        }

        PlayerInput input = player.GetComponent<PlayerInput>();
        if (input != null)
        {
            Debug.Log($"=== {playerName} Input Info ===");
            Debug.Log($"Current Control Scheme: {input.currentControlScheme}");
            Debug.Log($"Devices: {string.Join(", ", input.devices)}");
            Debug.Log($"Input Enabled: {input.inputIsActive}");
        }
    }

    // Visualize spawn points in editor
    void OnDrawGizmosSelected()
    {
        if (player1Spawn != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(player1Spawn.position, 0.5f);
            Gizmos.DrawRay(player1Spawn.position, player1Spawn.forward * 2);
        }

        if (player2Spawn != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player2Spawn.position, 0.5f);
            Gizmos.DrawRay(player2Spawn.position, player2Spawn.forward * 2);
        }
    }
}