using UnityEngine;

public interface IInteractable
{
    void Interact(PlayerIdentity requester);
    void AltInteract(PlayerIdentity requester);
    bool IsAvailable(PlayerIdentity requester);
    InteractionPromptData GetInteractionPromptData(PlayerIdentity requester);
    float GetHoldDuration(PlayerIdentity requester);
}