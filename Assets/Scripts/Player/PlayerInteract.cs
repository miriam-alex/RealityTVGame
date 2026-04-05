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

       bool isControllerPlayer = _myId.playerIndex == 0;
       string primaryKeyLabel = isControllerPlayer ? controllerPrimaryLabel : _interactKey;
       string secondaryKeyLabel = isControllerPlayer ? controllerSecondaryLabel : _altInteractKey;

       promptUI.Show(
           primaryKeyLabel,
           data.PrimaryAction,
           secondaryKeyLabel,
           data.SecondaryAction,
           data.HasSecondary);
   }
   
    private void HandleInput()
    {
        if (_myId == null || _playerInput == null) return;

        // USE ACTIONS FOR EVERYONE
        // This allows P1, P2, P3 etc. to all use their own controllers/keys
        bool interactPressed = _playerInput.actions["Interact"].WasPressedThisFrame();
        
        // Using FindAction for Steal in case it's not mapped in every Action Map
        var stealAction = _playerInput.actions.FindAction("Steal", false);
        bool altInteractPressed = (stealAction != null) && stealAction.WasPressedThisFrame();

        if (_currentInteractable != null)
        {
            if (interactPressed) _currentInteractable.Interact(_myId);
            if (altInteractPressed) _currentInteractable.AltInteract(_myId);
        }
        else if (interactPressed)
        {
            _inventory.TryDrop();
        }
    }


   private void OnTriggerEnter(Collider other)
   {
       if (other.TryGetComponent(out IInteractable interactable)) 
       {
         _nearbyInteractables[other] = interactable;

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
       _nearbyInteractables.Remove(other);
   }

   public IInteractable GetCurrentTarget()
   {
       return _currentInteractable;
   }


   private Key GetKey(string name) => System.Enum.TryParse(name, true, out Key k) ? k : Key.None;
}

