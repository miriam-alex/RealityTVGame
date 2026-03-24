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

    private readonly System.Collections.Generic.Dictionary<int, float> _nextGiveAllowedTimeByRequester = new();
    private readonly System.Collections.Generic.Dictionary<int, float> _nextStealAllowedTimeByRequester = new();

    private void Start()
    {
		_scoreManager = FindAnyObjectByType<ScoreManager>();
        _myId = GetComponentInParent<PlayerIdentity>();
        _myInventory = GetComponentInParent<PlayerInventory>();
        if (_myId == null) Debug.LogError($"[PlayerInteractable] Could not find PlayerIdentity in parent of {name}!");
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

        int requesterKey = requester != null ? requester.playerIndex : -1;
        if (_nextGiveAllowedTimeByRequester.TryGetValue(requesterKey, out float nextAllowed) && Time.time < nextAllowed)
            return;

        bool isSuccess = ExecuteInventoryTransfer(_myId, requester, "Here you go!", out Resource transferredResource);


        _nextGiveAllowedTimeByRequester[requesterKey] = Time.time + COOLDOWN;

		// You get followers for doing a good thing on camera!
        if (requester != null && requester.isSpotted && isSuccess)
        {
            DirectorManager.Instance.LogDrama(
                DramaType.GiveItem, 
                transform,           // Where it happened
                requester,      // The Thief
                _myId,          // The Victim
                ScoreManager.Instance.giveReward,                 // Score Penalty
                $"{requester.name} is a saint, giving to {_myId.name}!", 
                3f,                // High drama intensity
                transferredResource
            );
        }
    }
    
    public void AltInteract(PlayerIdentity requester) 
    {
        if (!InteractionCoordinator.Instance.CanInteract(requester, _myId)) return;

        int requesterKey = requester != null ? requester.playerIndex : -1;
        if (_nextStealAllowedTimeByRequester.TryGetValue(requesterKey, out float nextAllowed) && Time.time < nextAllowed)
            return;

        bool isSuccess = ExecuteInventoryTransfer(requester, _myId, "Stolen!", out Resource transferredResource);

        _nextStealAllowedTimeByRequester[requesterKey] = Time.time + COOLDOWN;

		// If caught, you lose followers for doing a bad thing on camera.
        if (requester != null && requester.isSpotted && isSuccess)
        {
            DirectorManager.Instance.LogDrama(
                DramaType.StealItem, 
                transform,           // Where it happened
                requester,      // The Thief
                _myId,          // The Victim
                -ScoreManager.Instance.stealPenalty,                 // Score Penalty
                $"{requester.name} caught red-handed!", 
                8.5f,                // High drama intensity
                transferredResource
            );
        }
    }
    
    private bool ExecuteInventoryTransfer(PlayerIdentity taker, PlayerIdentity giver, string label, out Resource transferredResource)
    {
        transferredResource = null;
        // 1. Retrieve the inventories from the passed identities
        PlayerInventory takerInv = taker.GetComponent<PlayerInventory>();
        PlayerInventory giverInv = giver.GetComponent<PlayerInventory>();

        bool success = false;
        if (takerInv != null && giverInv != null)
        {
            success = giverInv.TransferToPlayerInventory(takerInv, out Grabbable transferred);

            if (success && transferred != null)
            {
                // If this Grabbable is also a ResourceItem, record which resource moved.
                ResourceItem resourceItem = transferred.GetComponent<ResourceItem>();
                if (resourceItem != null && resourceItem.resource != null)
                    transferredResource = resourceItem.resource;
            }
        
            if (!success) 
            {
                label = $"{giver.name} is empty!";
            }
        }

        // 4. Visual feedback
        ChatBubble prefab = chatBubblePrefab;
        if (prefab == null)
        {
            prefab = taker != null ? taker.GetComponent<PlayerInteract>()?.chatBubble : null;
        }
        if (prefab == null)
        {
            prefab = giver != null ? giver.GetComponent<PlayerInteract>()?.chatBubble : null;
        }
        
        ChatBubble.Create(prefab, Vector3.up * 2, transform, label, 2f);
        return success;
    }
    
    public string GetInteractionPrompt(string i, string a) => $"[{i}] Give | [{a}] Steal";
    
    public PlayerIdentity GetPlayerIdentity() => _myId;
}