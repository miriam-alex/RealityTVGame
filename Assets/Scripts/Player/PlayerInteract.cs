using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("Settings")]
    public float interactRange = 2f;
    [SerializeField] private LayerMask pickUpLayerMask;

    [Header("Key Bindings")]
    public string p1Interact = "E";
    public string p1Give = "F";
    public string p2Interact = "O";
    public string p2Give = "P";

    private PlayerIdentity _myId;
    private PlayerInteractable _currentInteractable;

    private void Start()
    {
        _myId = GetComponent<PlayerIdentity>();
        
        SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = interactRange;
    }

    private void Update()
    {
        if (_currentInteractable == null || _myId.spotlightOn) return;

        HandleInput();
    }

    private void HandleInput()
    {
        bool isP1 = (_myId.playerIndex == 0);
        string interactKey = isP1 ? p1Interact : p2Interact;
        string giveKey = isP1 ? p1Give : p2Give;
        
        _currentInteractable.ShowVicinityMessage(interactKey, giveKey);

        bool interactPressed = Keyboard.current[GetKey(interactKey)].wasPressedThisFrame;
        bool givePressed = Keyboard.current[GetKey(giveKey)].wasPressedThisFrame;

        if (isP1 && Gamepad.current != null)
        {
            interactPressed |= Gamepad.current.buttonSouth.wasPressedThisFrame;
            givePressed |= Gamepad.current.buttonEast.wasPressedThisFrame;
        }

        if (interactPressed)
        {
            _currentInteractable.Trade(_myId);
        }
        else if (givePressed)
        {
            _currentInteractable.Give(_myId);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerInteractable interactable))
        {
            // Don't interact with yourself
            if (interactable.gameObject == gameObject) return;
            
            _currentInteractable = interactable;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerInteractable interactable))
        {
            if (_currentInteractable == interactable)
            {
                _currentInteractable = null;
            }
        }
    }

    private Key GetKey(string keyName)
    {
        if (System.Enum.TryParse(keyName, true, out Key key))
            return key;
        return Key.None;
    }
}