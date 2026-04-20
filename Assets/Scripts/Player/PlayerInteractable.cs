using UnityEngine;
using System.Collections;

public class PlayerInteractable : MonoBehaviour, IInteractable
{
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


        // 1. Check if either player is currently stunned
        PlayerStatus requesterStatus = requester.GetComponent<PlayerStatus>();
        PlayerStatus victimStatus = _myId.GetComponent<PlayerStatus>();


        if (requesterStatus != null && requesterStatus.isStunned)
        {
            Debug.Log("Steal blocked: You are stunned!");
            return;
        }

        bool isAggravated = (victimStatus != null && victimStatus.isStunned);

        if (!InteractionCoordinator.Instance.CanInteract(requester, _myId)) return;

        int requesterKey = requester != null ? requester.playerIndex : -1;
        if (_nextStealAllowedTimeByRequester.TryGetValue(requesterKey, out float nextAllowed) && Time.time < nextAllowed)
            return;

        if (victimStatus != null && victimStatus.isStunned)
        {
            Debug.Log("You are stealing from a stunned player!");
        }
        bool isSuccess = ExecuteInventoryTransfer(requester, _myId, "Stolen!", out Resource transferredResource);

        _nextStealAllowedTimeByRequester[requesterKey] = Time.time + COOLDOWN;

        // 2. If steal is successful, apply the 5-second stun to BOTH players
        if (isSuccess)
        {
            // 2. Register the steal. This method now handles the logic of 
            // incrementing the counter AND applying the stun if the limit is exceeded.
            requesterStatus?.RegisterSuccessfulSteal();

            // 3. Define the penalty and caption based on status
            int penalty = isAggravated ? 100 : ScoreManager.Instance.stealPenalty;
            string caption = isAggravated ? $"{requester.name} robbed a defenseless player!" : $"{requester.name} caught red-handed!";
            float intensity = isAggravated ? 10f : 8.5f; // Higher intensity for aggravated
            
            // REMOVE: requesterStatus?.ApplyStun(5f); 
            // Do not call ApplyStun here anymore! RegisterSuccessfulSteal handles it.

            // Log drama only if successful
            if (requester != null && requester.isSpotted)
            {
                DirectorManager.Instance.LogDrama(
                    DramaType.StealItem, 
                    transform, 
                    requester, 
                    _myId, 
                    -penalty, 
                    caption, 
                    intensity, 
                    transferredResource
                );
            }
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
        //ChatBubbleManager.Show("Look at this!", transform, new Vector3(0, 2, 0), 5.0f);
        return success;
    }
    
    public InteractionPromptData GetInteractionPromptData(PlayerIdentity requester)
        => InteractionPromptData.PrimaryAndSecondary("Give", "Steal");
    
    public PlayerIdentity GetPlayerIdentity() => _myId;
}