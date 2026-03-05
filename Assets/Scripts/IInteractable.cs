using UnityEngine;

public interface IInteractable
{
    void OnInteract(GameObject interactor);
    string GetInteractionPrompt();
    bool CanInteract(GameObject interactor);
    Transform GetTransform();
}