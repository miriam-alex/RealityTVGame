using UnityEngine;
using System.Collections;

public class PlayerInteractable : MonoBehaviour, IInteractable
{
	private ScoreManager _scoreManager;
    private PlayerIdentity _myId;
    private PlayerInventory _myInventory;
    private const float COOLDOWN = 1.0f; 
    public event System.Action<float, float> OnCooldownStarted;

    [SerializeField] private float maxCooldownValue = 5.0f; // Set your desired duration
    private float currentCooldownValue = 0f;
    private bool IsOnCooldown => currentCooldownValue > 0;

    private readonly System.Collections.Generic.Dictionary<int, float> _nextGiveAllowedTimeByRequester = new();
    private readonly System.Collections.Generic.Dictionary<int, float> _nextStealAllowedTimeByRequester = new();

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip giveAudioClip;

    private void Start()
    {
		_scoreManager = FindAnyObjectByType<ScoreManager>();
        _myId = GetComponentInParent<PlayerIdentity>();
        _myInventory = GetComponentInParent<PlayerInventory>();
        if (_myId == null) Debug.LogError($"[PlayerInteractable] Could not find PlayerIdentity in parent of {name}!");

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void PlaySfx(AudioClip clip)
   {
    if (audioSource == null || clip == null) return;
    audioSource.PlayOneShot(clip);
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

        // int requesterKey = requester != null ? requester.playerIndex : -1;
        // if (_nextGiveAllowedTimeByRequester.TryGetValue(requesterKey, out float nextAllowed) && Time.time < nextAllowed)
        //     return;

        Debug.Log("Interacting: " + requester.name);
        bool isSuccess = ExecuteInventoryTransfer(_myId, requester, "Here you go!", out Resource transferredResource);
        Debug.Log($"Inventory transfer status: {isSuccess}");

        if (isSuccess)
        {
            PlaySfx(giveAudioClip);
        }


        // _nextGiveAllowedTimeByRequester[requesterKey] = Time.time + COOLDOWN;
		// You get followers for doing a good thing on camera!
        if (isSuccess && requester != null && requester.isSpotted)
        {
            DirectorManager.Instance.LogDrama(
                DramaType.GiveItem, 
                transform,           // Where it happened
                requester,      // The Thief
                _myId,          // The Victim
                ScoreManager.Instance.giveReward,                 // Score Penalty
                3f,                // High drama intensity
                transferredResource
            );
        }
    }
    public void TriggerCooldown()
    {
        currentCooldownValue = maxCooldownValue;
        OnCooldownStarted?.Invoke(currentCooldownValue, maxCooldownValue);
    }
    
    public void AltInteract(PlayerIdentity requester) 
    {
        PlayerStatus thiefStatus = requester.GetComponent<PlayerStatus>();
        if (thiefStatus != null && thiefStatus.IsOnCooldown())
        {
            Debug.Log("Tried to steal on cooldown!");
            return;
        }

        if (!InteractionCoordinator.Instance.CanInteract(requester, _myId)) return;
        
        bool isSuccess = ExecuteInventoryTransfer(requester, _myId, "Stolen!", out Resource transferredResource);

        if (isSuccess)
        {
            // 1. Backend Status: Set cooldown on both participants
            _myId.GetComponent<PlayerStatus>()?.SetStealCooldown();
            thiefStatus?.SetStealCooldown();

            // 2. Trigger UI: Look for the UI component and trigger it ONCE per player
            var requesterUI = requester.GetComponentInChildren<PlayerCooldownUI>();
            requesterUI?.TriggerCooldown(); 

            var victimUI = GetComponentInChildren<PlayerCooldownUI>();
            victimUI?.TriggerCooldown();

            if (requester != null && requester.isSpotted)
            {
                Debug.Log($"{requester.name} caught stealing on camera!");
                DirectorManager.Instance.LogDrama(
                    DramaType.StealItem,
                    transform,
                    requester,
                    _myId,
                    -ScoreManager.Instance.stealPenalty,
                    8.0f,
                    transferredResource
                );
            }
            else
            {
                Debug.Log($"{requester.name} stole but evaded the camera!");
            }

        }
    }


    
    private void Update()
    {
        if (currentCooldownValue > 0)
        {
            currentCooldownValue -= Time.deltaTime;
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
                Debug.LogError($"Transfer from {giver.name}'s inventory to {taker.name}'s inventory failed!");
                label = $"{giver.name} is empty!";
            }
        }
        else
        {
            if (takerInv == null) {Debug.LogError($"{taker.name}'s inventory is null!");}
            if (giverInv == null) {Debug.LogError($"{giver.name}'s inventory is null!");}
        }

        //ChatBubbleManager.Show("Look at this!", transform, new Vector3(0, 2, 0), 5.0f);
        return success;
    }
    
    public InteractionPromptData GetInteractionPromptData(PlayerIdentity requester)
        => InteractionPromptData.PrimaryAndSecondary("Give", "Steal");
    
    public PlayerIdentity GetPlayerIdentity() => _myId;
}