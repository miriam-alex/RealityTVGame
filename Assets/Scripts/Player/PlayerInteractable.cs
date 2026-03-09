using UnityEngine;
using System.Collections;

public class PlayerInteractable : MonoBehaviour, IInteractable
{
    [Header("References")]
    public ChatBubble chatBubblePrefab;
    
	private ScoreManager _scoreManager;
    private PlayerIdentity _myId;
    private PlayerInventory _myInventory;
    private const float COOLDOWN = 1.0f; 

    private void Start()
    {
		_scoreManager = FindAnyObjectByType<ScoreManager>();
        _myId = GetComponent<PlayerIdentity>();
        _myInventory = GetComponent<PlayerInventory>();
    }

    public bool IsAvailable(PlayerIdentity requester)
    {
        // 1. Basic sanity checks
        if (_myId == null || requester == null || requester == _myId) return false;

        // 2. Check if either player is already "Busy" (locked in a trade)
        // We add a helper method to the Coordinator to check this cleanly
        if (!InteractionCoordinator.Instance.CanInteract(requester, _myId))
        {
            return false;
        }

        // 3. Optional: Are they currently looking at each other? 
        // You could add a distance check here if you want to be extra safe
        return true;
    }

    public float GetHoldDuration(PlayerIdentity requester) => 1.2f;
    
    public void Interact(PlayerIdentity requester)
    {
        if (!InteractionCoordinator.Instance.CanInteract(requester, _myId)) return;
        ExecuteInventoryTransfer(_myId, requester, "Here you go!");
		// You get followers for doing a good thing on camera!
        if (requester.isSpotted)
        {
            _scoreManager.RewardGive(requester.gameObject);
        }
    }
    
    public void AltInteract(PlayerIdentity requester) 
    {
        if (!InteractionCoordinator.Instance.CanInteract(requester, _myId)) return;
        ExecuteInventoryTransfer(requester, _myId, "Stolen!");
		// If caught, you lose followers for doing a bad thing on camera.
        if (requester.isSpotted)
        {
            _scoreManager.PenalizeSteal(requester.gameObject);
        }
    }
    
    private void ExecuteInventoryTransfer(PlayerIdentity taker, PlayerIdentity giver, string label)
    {
        // 1. Retrieve the inventories from the passed identities
        PlayerInventory takerInv = taker.GetComponent<PlayerInventory>();
        PlayerInventory giverInv = giver.GetComponent<PlayerInventory>();
    
        // 2. Perform the logic ONLY if components exist
        if (takerInv != null && giverInv != null)
        {
            // 3. The transfer is now unambiguous because the Coordinator 
            // has already validated that 'taker' and 'giver' are distinct 
            // and available.
            bool success = giverInv.TransferToPlayerInventory(takerInv);
        
            if (!success) 
            {
                label = $"{giver.name} is empty!";
            }
        }

        // 4. Visual feedback
        ChatBubble.Create(chatBubblePrefab, Vector3.up * 2, transform, label, 2f);
    }
    
    public string GetInteractionPrompt(string i, string a) => $"[{_myId.interactKey}] Give | [{_myId.altInteractKey}] Steal";
    
    public PlayerIdentity GetPlayerIdentity() => _myId;
}