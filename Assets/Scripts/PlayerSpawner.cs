using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Player Prefabs")]
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;  

    [Header("Spawn Points")]
    [SerializeField] private Transform player1Spawn;
    [SerializeField] private Transform player2Spawn;

    [Header("Player Settings")]
    [SerializeField] private Material player1Material;
    [SerializeField] private Material player2Material;

    [Header("Input Settings")]
    [SerializeField] private InputActionAsset inputActions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnPlayers(); 
    }

    void SpawnPlayers()
    {
        //Player 1
        GameObject player1 = Instantiate(player1Prefab, player1Spawn.position, player1Spawn.rotation);

        player1.name = "Player1";
    player1.tag = "Player1";

        PlayerController p1Controller = player1.GetComponent<PlayerController>();
        if (p1Controller != null)
        {
            p1Controller.SetPlayerNumber(1);
            p1Controller.SetPlayerColor(Color.blue);
            p1Controller.SetPlayerName("Player1"); 
        }

        PlayerInput p1Input = player1.GetComponent<PlayerInput>();
        if (p1Input != null)
        {
            p1Input.actions = inputActions; // Assign your input actions asset
            p1Input.defaultActionMap = "Player";
            p1Input.notificationBehavior = PlayerNotifications.SendMessages;
        }

        ApplyPlayerMaterial(player1, player1Material);

        //Player 2
        GameObject player2 = Instantiate(player2Prefab, player2Spawn.position, player2Spawn.rotation);

        player2.name = "Player2";
        player2.tag = "Player2";

        PlayerController p2Controller = player1.GetComponent<PlayerController>();
        if (p2Controller != null)
        {
            p2Controller.SetPlayerNumber(2);
            p2Controller.SetPlayerColor(Color.blue);
            p2Controller.SetPlayerName("Player2");
        }

        PlayerInput p2Input = player2.GetComponent<PlayerInput>();
        if (p2Input != null)
        {
            p2Input.actions = inputActions; // Same input actions asset
            p2Input.defaultActionMap = "Player";
            p2Input.notificationBehavior = PlayerNotifications.SendMessages;
        }

        ApplyPlayerMaterial(player2, player1Material);
    }

     void ApplyPlayerMaterial(GameObject player, Material material)
    {
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = material;
        }
    }
}
