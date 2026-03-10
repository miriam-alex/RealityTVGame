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
        if (_myId == null || requester == null || requester == _myId) return false;
        
        if (!InteractionCoordinator.Instance.CanInteract(requester, _myId))
        {
            return false;
        }
        
        return true;
    }

    public float GetHoldDuration(PlayerIdentity requester) => 1.2f;
    
    public void Interact(PlayerIdentity requester)
    {
        if (!InteractionCoordinator.Instance.CanInteract(requester, _myId)) return;
        bool isSuccess = ExecuteInventoryTransfer(_myId, requester, "Here you go!");
		// You get followers for doing a good thing on camera!
        if (requester.isSpotted && isSuccess)
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
    
    private bool ExecuteInventoryTransfer(PlayerIdentity taker, PlayerIdentity giver, string label)
    {
        // 1. Retrieve the inventories from the passed identities
        PlayerInventory takerInv = taker.GetComponent<PlayerInventory>();
        PlayerInventory giverInv = giver.GetComponent<PlayerInventory>();

        bool success = false;
        if (takerInv != null && giverInv != null)
        {
            success = giverInv.TransferToPlayerInventory(takerInv);
        
            if (!success) 
            {
                label = $"{giver.name} is empty!";
            }
        }

        // 4. Visual feedback
        ChatBubble.Create(chatBubblePrefab, Vector3.up * 2, transform, label, 2f);
        return success;
    }
    
    public string GetInteractionPrompt(string i, string a) => $"[{_myId.interactKey}] Give | [{_myId.altInteractKey}] Steal";
    
    public PlayerIdentity GetPlayerIdentity() => _myId;
}