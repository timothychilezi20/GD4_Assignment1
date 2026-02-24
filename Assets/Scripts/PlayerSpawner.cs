using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform player1Spawn;
    [SerializeField] private Transform player2Spawn;

    [Header("Colors")]
    [SerializeField] private Material player1Material;
    [SerializeField] private Material player2Material;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    void Start()
    {
        Debug.Log("=== PlayerSpawner Starting ===");
        Debug.Log($"Input Actions Asset: {(inputActions != null ? inputActions.name : "NULL")}");

        SpawnPlayer1();
        SpawnPlayer2();
    }

    void SpawnPlayer1()
    {
        GameObject player = Instantiate(playerPrefab, player1Spawn.position, player1Spawn.rotation);
        player.name = "Player1";
        player.tag = "Player1";

        // Controller
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SetPlayerNumber(1);
            controller.SetPlayerColor(Color.blue);
            controller.SetPlayerName("Player1");
        }

        // CRITICAL: Player Input setup
        PlayerInput input = player.GetComponent<PlayerInput>();
        if (input != null)
        {
            input.actions = inputActions;
            input.defaultActionMap = "Gameplay";
            input.notificationBehavior = PlayerNotifications.SendMessages;

            // Force Unity to find devices
            InputSystem.Update();

            // Log what devices are available
            Debug.Log($"Available devices: {InputSystem.devices.Count}");
            foreach (var device in InputSystem.devices)
            {
                Debug.Log($"- {device.name} ({device.deviceId})");
            }

            // Log the current control scheme after a short delay
            StartCoroutine(LogControlScheme(input, "Player1"));
        }
        else
        {
            Debug.LogError("PlayerInput component missing on prefab!");
        }

        // Visual
        ApplyMaterial(player, player1Material, Color.blue);
    }

    void SpawnPlayer2()
    {
        GameObject player = Instantiate(playerPrefab, player2Spawn.position, player2Spawn.rotation);
        player.name = "Player2";
        player.tag = "Player2";

        // Controller
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SetPlayerNumber(2);
            controller.SetPlayerColor(Color.red);
            controller.SetPlayerName("Player2");
        }

        // CRITICAL: Player Input setup
        PlayerInput input = player.GetComponent<PlayerInput>();
        if (input != null)
        {
            input.actions = inputActions;
            input.defaultActionMap = "Gameplay";
            input.notificationBehavior = PlayerNotifications.SendMessages;

            // Force Unity to find devices
            InputSystem.Update();

            // Log the current control scheme after a short delay
            StartCoroutine(LogControlScheme(input, "Player2"));
        }

        // Visual
        ApplyMaterial(player, player2Material, Color.red);
    }

    System.Collections.IEnumerator LogControlScheme(PlayerInput input, string playerName)
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log($"{playerName} control scheme: {input.currentControlScheme}");
        Debug.Log($"{playerName} devices: {string.Join(", ", input.devices)}");
    }

    void ApplyMaterial(GameObject player, Material material, Color fallback)
    {
        Renderer renderer = player.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            if (material != null)
                renderer.material = material;
            else
                renderer.material.color = fallback;
        }
    }
}