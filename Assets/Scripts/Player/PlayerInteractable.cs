using UnityEngine;
using System.Collections;

public class PlayerInteractable : MonoBehaviour, IInteractable
{
    [Header("References")]
    public ChatBubble chatBubblePrefab;
    
    private PlayerIdentity _myId;
    private ScoreManager _scoreManager;
    
    private float _lastInteractionTime;
    private const float COOLDOWN = 1.0f; 

    private void Start()
    {
        _myId = GetComponent<PlayerIdentity>();
        _scoreManager = FindAnyObjectByType<ScoreManager>();
    }

    public bool IsAvailable(PlayerIdentity requester) 
    {
        // Strict availability: Neither party can be busy, and we respect the cooldown
        if (_myId == null || requester == null) return false;
        return !_myId.IsBusy && !requester.IsBusy && (Time.time - _lastInteractionTime > COOLDOWN);
    }

    public float GetHoldDuration(PlayerIdentity requester) => 1.2f;

    // Trade
    public void Interact(PlayerIdentity requester)
    {
        // Handshake verification: Ensure both are still available before executing
        if (!IsAvailable(requester)) return;

        LockPlayers(requester);

        string label = _myId.spotlightOn ? "Public Trade (Taxed!)" : "Secret Trade!";
        TransferPoints(requester, 5, label);

        StartCoroutine(UnlockPlayers(0.5f, requester));
    }

    // Steal
    public void AltInteract(PlayerIdentity requester) 
    {
        if (!IsAvailable(requester)) return;

        LockPlayers(requester);
        
        _scoreManager.TransferPoints(gameObject, requester.gameObject);
        ChatBubble.Create(chatBubblePrefab, Vector3.up * 2, transform, "STOLEN!", 2f);

        StartCoroutine(UnlockPlayers(0.5f, requester));
    }

    private void LockPlayers(PlayerIdentity requester)
    {
        _myId.SetBusy(true, null);
        requester.SetBusy(true, null);
        _lastInteractionTime = Time.time;
    }

    private IEnumerator UnlockPlayers(float delay, PlayerIdentity requester)
    {
        yield return new WaitForSeconds(delay);
        _myId.SetBusy(false, null);
        if (requester != null) requester.SetBusy(false, null);
    }

    private void TransferPoints(PlayerIdentity requester, int amount, string label)
    {
        int giverScore = _scoreManager.GetScore(gameObject);
        int finalAmount = Mathf.Min(amount, giverScore);
        
        if (finalAmount > 0)
        {
            _scoreManager.AddScore(-finalAmount, gameObject);
            _scoreManager.AddScore(finalAmount, requester.gameObject);
        }
        
        ChatBubble.Create(chatBubblePrefab, Vector3.up * 2, transform, label, 2f);
    }

    public string GetInteractionPrompt(string i, string a) 
    {
        // This ensures the prompt displays the keys of the target (the interactable object)
        return $"Hold [{_myId.interactKey}] Trade | [{_myId.altInteractKey}] Steal";
    }
}