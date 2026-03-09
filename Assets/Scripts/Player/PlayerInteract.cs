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
        if (_myId.IsBusy || _currentInteractable == null) return;

        MonoBehaviour interactableMono = _currentInteractable as MonoBehaviour;
        if (interactableMono == null) { _currentInteractable = null; return; }

        if (!_currentInteractable.IsAvailable(_myId)) return;

        _msgTimer -= Time.deltaTime;
        if (_msgTimer <= 0)
        {
            // Pass empty strings or null because GetInteractionPrompt now ignores them 
            // and uses its own Internal Identity (as per our updated PlayerInteractable)
            string prompt = _currentInteractable.GetInteractionPrompt("", "");
            ChatBubble.Create(chatBubble, Vector3.up * 1.7f, interactableMono.transform, prompt, 0.25f);
        
            if (interactableMono.TryGetComponent(out SpotlightVisual visual)) 
                visual.FlashRed(0.1f);

            _msgTimer = 0.2f; 
        }
    }

    private void HandleInput()
    {
        if (_myId.IsBusy) { _holdTimer = 0; return; }

        Key iKey = GetKey(_myId.interactKey);
        Key aKey = GetKey(_myId.altInteractKey);

        if (_currentInteractable != null)
        {
            // 1. Logic for Stealing (Instant, no handshake required)
            if (Keyboard.current[aKey].wasPressedThisFrame)
            {
                _currentInteractable.AltInteract(_myId);
            }

            // 2. Logic for Trading (Requires handshake + timer)
            if (_currentInteractable is PlayerInteractable targetPlayer)
            {
                bool iPressed = Keyboard.current[iKey].isPressed;
                InteractionCoordinator.Instance.SetHandshake(_myId, targetPlayer.GetPlayerIdentity(), iPressed);

                if (iPressed && InteractionCoordinator.Instance.IsTradeReady(_myId, targetPlayer.GetPlayerIdentity()))
                {
                    RunHoldTimer();
                }
                else if (Keyboard.current[iKey].wasReleasedThisFrame)
                {
                    _holdTimer = 0;
                }
            }
            else
            {
                // 3. Logic for Generic Objects (Instant)
                if (Keyboard.current[iKey].wasPressedThisFrame)
                {
                    _currentInteractable.Interact(_myId);
                }
            }
        }
    }
    private void RunHoldTimer()
    {
        float duration = _currentInteractable.GetHoldDuration(_myId);
        _holdTimer += Time.deltaTime;
        UpdateHoldUI(Mathf.Clamp01(_holdTimer / duration));

        if (_holdTimer >= duration)
        {
            // Just trigger the master controller
            _currentInteractable.Interact(_myId);
            _holdTimer = 0;
        }
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
            _holdTimer = 0;
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
    
    public IInteractable GetCurrentTarget()
    {
        return _currentInteractable;
    }

    private Key GetKey(string name) => System.Enum.TryParse(name, true, out Key k) ? k : Key.None;
}