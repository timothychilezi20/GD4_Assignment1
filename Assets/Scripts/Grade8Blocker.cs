using UnityEngine;
using System.Collections;

public class Grade8Blocker : MonoBehaviour
{
    [Header("Block Settings")]
    [SerializeField] private float slowMultiplier = 0.35f;
    [SerializeField] private float slowDuration = 2f;
    [SerializeField] private float cooldownPerPlayer = 3f;

    [Header("Feedback")]
    [SerializeField] private string blockMessage = "Grade 8s are blocking you!";
    [SerializeField] private Color messageColor = Color.red;
    [SerializeField] private ParticleSystem blockEffect;

    private bool player1OnCooldown = false;
    private bool player2OnCooldown = false;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        int playerNumber = player.GetPlayerNumber();

        if (playerNumber == 1 && player1OnCooldown) return;
        if (playerNumber == 2 && player2OnCooldown) return;

        StartCoroutine(ApplyBlock(player));
    }

    private IEnumerator ApplyBlock(PlayerController player)
    {
        int playerNumber = player.GetPlayerNumber();

        if (playerNumber == 1)
            player1OnCooldown = true;
        else if (playerNumber == 2)
            player2OnCooldown = true;

        // Visual feedback
        if (blockEffect != null)
            blockEffect.Play();

        UIManager.Instance?.ShowPlayerMessage(
            playerNumber,
            blockMessage,
            messageColor
        );

        // Apply slow
        player.ApplyMovementSlow(slowMultiplier, slowDuration);

        yield return new WaitForSeconds(cooldownPerPlayer);

        if (playerNumber == 1)
            player1OnCooldown = false;
        else if (playerNumber == 2)
            player2OnCooldown = false;
    }
}