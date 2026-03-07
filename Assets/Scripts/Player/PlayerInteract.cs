using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("Settings")]
    public float interactRange = 2.5f;
    [Header("Prefabs")]
    public ChatBubble chatBubble;
    
    private float _holdTimer = 0f;
    private string _interactKey;
    private string _altInteractKey;
    
    private float _msgTimer = 0f;
    private PlayerIdentity _myId;
    private PlayerInventory _inventory;
    private IInteractable _currentInteractable;

    private void Start()
    {
        _myId = GetComponent<PlayerIdentity>();
        _interactKey = _myId.interactKey;
        _altInteractKey = _myId.altInteractKey;
        _inventory = GetComponent<PlayerInventory>();
        
        // Ensure trigger setup
        SphereCollider col = gameObject.GetComponent<SphereCollider>();
        if (col == null) col = gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = interactRange;
    }

    private void Update()
    {
        HandleInput();
        HandleVicinityDisplay();
    }
    
    private void HandleVicinityDisplay()
    {
        // 1. If I am busy, I should NOT be looking for things to interact with
        // 2. If the current object is busy, it shouldn't show a prompt
        if (_myId.IsBusy || _currentInteractable == null || !_currentInteractable.IsAvailable(_myId)) 
        {
            return;
        }

        _msgTimer -= Time.deltaTime;
        if (_msgTimer <= 0)
        {
            // Only show if the target is physically available right now
            if (_currentInteractable is MonoBehaviour target)
            {
                string prompt = _currentInteractable.GetInteractionPrompt(_interactKey, _altInteractKey);
                ChatBubble.Create(chatBubble, Vector3.up * 1.7f, target.transform, prompt, 0.25f);
            
                if (target.TryGetComponent(out SpotlightVisual visual)) 
                    visual.FlashRed(0.1f);
            }

            _msgTimer = 0.2f; 
        }
    }

    private void HandleInput()
    {
        if (_myId.IsBusy) 
        {
            _holdTimer = 0;
            return;
        }

        Key iKey = GetKey(_myId.interactKey);
        Key altKey = GetKey(_myId.altInteractKey);

        if (_currentInteractable != null)
        {
            bool iPressed = Keyboard.current[iKey].isPressed;

            if (iPressed)
            {
                // --- MUTUAL HANDSHAKE LOGIC ---
                if (IsTargetHoldingKey(_currentInteractable))
                {
                    float duration = _currentInteractable.GetHoldDuration(_myId);
                    _holdTimer += Time.deltaTime;
                
                    UpdateHoldUI(Mathf.Clamp01(_holdTimer / duration));

                    if (_holdTimer >= duration)
                    {
                        _currentInteractable.Interact(_myId);
                        _holdTimer = 0; 
                    }
                }
                else 
                {
                    // Target is not holding: Show a "Waiting" status
                    ChatBubble.Create(chatBubble, Vector3.up * 2.2f, transform, "Waiting...", 0.1f);
                    _holdTimer = 0; // Reset progress until they join in
                }
            }
            else if (Keyboard.current[iKey].wasReleasedThisFrame)
            {
                _holdTimer = 0;
            }

            // --- ALT INTERACTION ---
            if (Keyboard.current[altKey].wasPressedThisFrame)
            {
                _currentInteractable.AltInteract(_myId);
            }
        }
        // ... rest of inventory code
    }

    // Helper to check the other player's input
    private bool IsTargetHoldingKey(IInteractable interactable)
    {
        if (interactable is MonoBehaviour targetMB)
        {
            var targetInteract = targetMB.GetComponent<PlayerInteract>();
            if (targetInteract != null)
            {
                Key targetKey = GetKey(targetInteract._myId.interactKey);
                return Keyboard.current[targetKey].isPressed;
            }
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
            _currentInteractable = interactable;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable) && _currentInteractable == interactable)
        {
            _currentInteractable = null;
            _holdTimer = 0; // Reset if we walk away
        }
    }
    
    private void UpdateHoldUI(float percent)
    {
        int totalSegments = 10;
        int filledSegments = Mathf.RoundToInt(percent * totalSegments);
        string bar = new string('■', filledSegments) + new string('□', totalSegments - filledSegments);
        string colorTag = _myId.spotlightOn ? "<color=red>" : "<color=green>";
        ChatBubble.Create(chatBubble, Vector3.up * 2.2f, transform, $"{colorTag}{bar}</color>", 0.1f);
    }

    private Key GetKey(string name) => System.Enum.TryParse(name, true, out Key k) ? k : Key.None;
}