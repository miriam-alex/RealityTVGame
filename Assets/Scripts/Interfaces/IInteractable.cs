using UnityEngine;

public interface IInteractable
{
    void Interact(PlayerIdentity requester);
    void AltInteract(PlayerIdentity requester);
    bool IsAvailable(PlayerIdentity requester);
    string GetInteractionPrompt(string interactKey, string altKey);
    float GetHoldDuration(PlayerIdentity requester);
}