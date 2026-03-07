using UnityEngine;

public interface IInteractable
{
    void Interact(PlayerIdentity requester);
    void AltInteract(PlayerIdentity requester);
    string GetInteractionPrompt(string interactKey, string altKey);
    bool IsAvailable(PlayerIdentity requester);
    float GetHoldDuration(PlayerIdentity requester);
}