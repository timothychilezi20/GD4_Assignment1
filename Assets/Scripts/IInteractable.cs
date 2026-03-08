using UnityEngine;

public interface IInteractable
{
    // Called when a player interacts
    void Interact(PlayerController player);

    // Returns the prompt string for this interactable
    string GetInteractionPrompt();

    // Checks if a given player can interact
    bool CanInteract(GameObject playerObject);
}