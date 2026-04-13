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
    public string controllerDropLabel = "X";
   private string _interactKey;
   private string _altInteractKey;
  
   private PlayerIdentity _myId;
   private PlayerInventory _inventory;
   private PlayerInput _playerInput;
   private IInteractable _currentInteractable;

    private readonly Dictionary<Collider, IInteractable> _nearbyInteractables = new Dictionary<Collider, IInteractable>();
    private IInteractable _lastPromptInteractable;


   private void Start()
   {
       _myId = GetComponent<PlayerIdentity>();
       _interactKey = _myId.interactKey;
       _altInteractKey = _myId.altInteractKey;
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

       _currentInteractable = best;

       if (_currentInteractable == _lastPromptInteractable)
       {
           return;
       }

       _lastPromptInteractable = _currentInteractable;

       if (promptUI == null)
       {
           return;
       }

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

        // Get Inputs
        bool interactPressed = _playerInput.actions["Interact"].WasPressedThisFrame(); // A
        bool altInteractPressed = _playerInput.actions.FindAction("Steal")?.WasPressedThisFrame() ?? false; // B
        bool dropPickPressed = _playerInput.actions.FindAction("Drop")?.WasPressedThisFrame() ?? false; // Y

        if (_currentInteractable != null)
        {
            // 1. Logic for PLAYER Interaction (A and B)
            if (_currentInteractable is PlayerInteractable)
            {
                if (interactPressed) _currentInteractable.Interact(_myId);    // A to Give
                if (altInteractPressed) _currentInteractable.AltInteract(_myId); // B to Steal
            }
            // 2. Logic for WORLD Interaction (Y)
            else 
            {
                // If it's a regular item (Grabbable), use Y to pick up
                if (dropPickPressed) _currentInteractable.Interact(_myId);
            }
        }
        else
        {
            // 3. Logic for DROPPING (Y)
            // If not looking at anything, Y drops the current item
            if (dropPickPressed)
            {
                _inventory.TryDrop();
            }
        }
    }

   private void OnTriggerEnter(Collider other)
   {
       if (other.TryGetComponent(out IInteractable interactable)) 
       {
         _nearbyInteractables[other] = interactable;

		 if (interactable is Grabbable grabbableObj)
		 {
             Debug.Log("should be calling show outline");
			grabbableObj.ShowOutline();
		 }

         if (interactable is PlayerInteractable && _myId != null && interactable.IsAvailable(_myId)) 
         {
             GetComponent<PlayerHaptics>()?.Pulse(0.3f, 0.6f);
             CameramanNPC cam = Object.FindAnyObjectByType<CameramanNPC>();
             if (cam != null)
             {
                 cam.SetDramaState(DirectorPersonality.Aggressive, other.transform, 5.0f);
                 Debug.Log($"Camera tracking {other.name}");
             }
         }
       }
   }


   private void OnTriggerExit(Collider other)
   {
       if (other.TryGetComponent(out IInteractable interactable))
       {
           if (interactable is Grabbable grabbableObj)
           {
               Debug.Log("should be calling hide outline");
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

