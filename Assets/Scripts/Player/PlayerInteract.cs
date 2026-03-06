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
    private Grabbable _heldObject;
    private Grabbable _nearbyGrabbable;
    private Transform _carryPoint;

    private void Start()
    {
        _myId = GetComponent<PlayerIdentity>();
        SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = interactRange;
        
        _carryPoint = transform.Find("ObjectCarryPoint");
    }

    private void Update()
    {
        // Rules for interaction:
        // 1. You cannot initiate a steal/trade in the spotlight.
        if (_currentInteractable == null && _nearbyGrabbable == null && _carryPoint == null) return;

        HandleInput();
    }

    private void HandleInput()
    {
        bool isP1 = (_myId.playerIndex == 0);
        string interactKey = isP1 ? p1Interact : p2Interact;
        string giveKey = isP1 ? p1Give : p2Give;
        
        bool interactPressed = Keyboard.current[GetKey(interactKey)].wasPressedThisFrame;
        bool givePressed = Keyboard.current[GetKey(giveKey)].wasPressedThisFrame;

        if (isP1 && Gamepad.current != null)
        {
            interactPressed |= Gamepad.current.buttonSouth.wasPressedThisFrame;
            givePressed |= Gamepad.current.buttonEast.wasPressedThisFrame;
        }
        
        // Priority order: if there's a grabbable, we go for the grabbable
        if (interactPressed && _nearbyGrabbable != null)
        {
            Debug.Log("Calling grab");
            _heldObject = _nearbyGrabbable;
            _heldObject.Grab(_carryPoint);
            _nearbyGrabbable = null;
        }
        // We can interact while we hold a grabbable, so that's priority 2
        else if (!_myId.spotlightOn && _currentInteractable)
        {
            _currentInteractable.ShowVicinityMessage(interactKey, giveKey);
            if (interactPressed)
            {
                _currentInteractable.Trade(_myId);
            }
            else if (givePressed)
            {
                _currentInteractable.Give(_myId);
            }
        }
        else if (interactPressed && _heldObject != null)
        {
            Debug.Log("Calling drop");
            _heldObject.Drop();
            _heldObject = null;
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsInLayerMask(other.gameObject, pickUpLayerMask))
        {
            if (other.TryGetComponent(out Grabbable grabbable))
            {
                _nearbyGrabbable = grabbable;
                Debug.Log("Nearby grabbable: " + grabbable.name);
            }
        }
        
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
        
        if (IsInLayerMask(other.gameObject, pickUpLayerMask))
        {
            if (other.TryGetComponent(out Grabbable grabbable))
            {
                _nearbyGrabbable = null;
                Debug.Log("No more nearby grabbable: " + grabbable.name);
            }
        }
    }

    private Key GetKey(string keyName)
    {
        if (System.Enum.TryParse(keyName, true, out Key key))
            return key;
        return Key.None;
    }
    
    private bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return (mask.value & (1 << obj.layer)) > 0;
    }
}