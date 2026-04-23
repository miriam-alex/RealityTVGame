using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


public class PlayerInteract : MonoBehaviour
{
   [Header("Settings")]
   public float interactRange = 2.5f;
    [Header("Prefabs")]

    [Header("UI")]
    public InteractionPromptUI promptUI;
    public string controllerPrimaryLabel = "A";
    public string controllerSecondaryLabel = "B";
    public string controllerDropLabel = "";
   private PlayerIdentity _myId;
   private PlayerInventory _inventory;
   private PlayerInput _playerInput;
   private IInteractable _currentInteractable;

    private readonly Dictionary<Collider, IInteractable> _nearbyInteractables = new Dictionary<Collider, IInteractable>();
    private IInteractable _lastPromptInteractable;


   private void Start()
   {
       _myId = GetComponent<PlayerIdentity>();
       _inventory = GetComponent<PlayerInventory>();
       _playerInput = GetComponent<PlayerInput>();
      
       SphereCollider col = gameObject.GetComponent<SphereCollider>();
       if (col == null) col = gameObject.AddComponent<SphereCollider>();
       col.isTrigger = true;
       col.radius = interactRange;
   }


   private void Update()
   {
       UpdateCurrentTargetAndPrompt();
       HandleInput();
   }

    private void UpdateCurrentTargetAndPrompt()
    {
        if (_myId == null)
        {
            if (promptUI != null) promptUI.Hide();
            
            // Ensure we clear the outline if the ID becomes null
            if (_currentInteractable is Grabbable g) g.HideOutline();
            
            _currentInteractable = null;
            _lastPromptInteractable = null;
            return;
        }

        IInteractable best = null;
        float bestDistSq = float.PositiveInfinity;
        Vector3 myPos = transform.position;

        foreach (var kvp in _nearbyInteractables)
        {
            IInteractable candidate = kvp.Value;
            if (candidate == null) continue;
            if (!candidate.IsAvailable(_myId)) continue;

            Component candidateComponent = candidate as Component;
            if (candidateComponent == null) continue;

            float distSq = (candidateComponent.transform.position - myPos).sqrMagnitude;
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best = candidate;
            }
        }

        // Logic to update Outlines when the target changes
        if (_currentInteractable != best)
        {
            // Hide outline for old target
            // The 'if (oldGrabbable)' check handles the MissingReferenceException
            if (_currentInteractable is Grabbable oldGrabbable && oldGrabbable != null) 
            {
                oldGrabbable.HideOutline();
            }

            _currentInteractable = best;

            // Show outline for new target
            if (_currentInteractable is Grabbable newGrabbable && newGrabbable != null)
            {
                newGrabbable.ShowOutline();
            }
        }

        if (_currentInteractable == _lastPromptInteractable) return;

        _lastPromptInteractable = _currentInteractable;

        if (promptUI == null) return;

        if (_currentInteractable == null)
        {
            promptUI.Hide();
            return;
        }

       InteractionPromptData data = _currentInteractable.GetInteractionPromptData(_myId);

       string primaryKeyLabel = controllerPrimaryLabel;
       string secondaryKeyLabel = controllerSecondaryLabel;

       //If using the UI Player Interaction Prompt Panel 
       //promptUI.Show(
       //    primaryKeyLabel,
       //    data.PrimaryAction,
       //    secondaryKeyLabel,
       //    data.SecondaryAction,
       //    data.HasSecondary);
   }
   
    private void HandleInput()
    {
        if (_myId == null || _playerInput == null) return;

        bool interactPressed = _playerInput.actions["Interact"].WasPressedThisFrame();
        bool altInteractPressed = _playerInput.actions.FindAction("Steal")?.WasPressedThisFrame() ?? false;
        bool dropPickPressed = _playerInput.actions.FindAction("Drop")?.WasPressedThisFrame() ?? false;
        bool yodelPressed = _playerInput.actions.FindAction("Yodel")?.WasPressedThisFrame() ?? false;

        // summons spotlight to player position when yodel is pressed
        if (yodelPressed)
        {
            SpotlightDirector.Instance?.SummonToPosition(transform.position);
        }

        if (_currentInteractable != null)
        {
            // 1. Handle Players (Stealing/Giving)
            if (_currentInteractable is PlayerInteractable)
            {
                if (interactPressed) _currentInteractable.Interact(_myId);   
                if (altInteractPressed) _currentInteractable.AltInteract(_myId);
            }
            // 2. Handle EVERYTHING ELSE (Pickups, etc.)
            // This MUST be a separate check that triggers on interactPressed OR dropPickPressed
            else
            {
                if (interactPressed || dropPickPressed)
                {
                    _currentInteractable.Interact(_myId);
                }
            }
        }
        else if (dropPickPressed)
        {
            _inventory.TryDrop();
        }
    }

   private void OnTriggerEnter(Collider other)
   {
       if (other.TryGetComponent(out IInteractable interactable)) 
       {
         _nearbyInteractables[other] = interactable;

		 if (interactable is Grabbable grabbableObj)
		 {
			grabbableObj.ShowOutline();
		 }
       }
   }


   private void OnTriggerExit(Collider other)
   {
       if (other.TryGetComponent(out IInteractable interactable))
       {
           if (interactable is Grabbable grabbableObj)
           {
               grabbableObj.HideOutline();
           }
       }
       _nearbyInteractables.Remove(other);
   }

   public IInteractable GetCurrentTarget()
   {
       return _currentInteractable;
   }


   private Key GetKey(string name) => System.Enum.TryParse(name, true, out Key k) ? k : Key.None;
}
