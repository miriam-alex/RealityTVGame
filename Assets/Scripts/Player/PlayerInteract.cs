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
       // return if no playerID
       if (_myId == null) return;


       Key iKey = GetKey(_myId.interactKey);
       Key aKey = GetKey(_myId.altInteractKey);


       bool interactPressed = false;
       bool altInteractPressed = false;


       // each player uses only their device: Player 1 = controller, Player 2 = keyboard
       // will change later
       if (_myId.playerIndex == 0)
       {
           // checks to see if controller input exists (similar to that in PlayerController)
           if (_playerInput != null)
           {
               interactPressed = _playerInput.actions["Interact"].WasPressedThisFrame();
               var stealAction = _playerInput.actions.FindAction("Steal", false);
               if (stealAction != null)
                   altInteractPressed = stealAction.WasPressedThisFrame();
           }
       }
       else
       {
           interactPressed = Keyboard.current != null && Keyboard.current[iKey].wasPressedThisFrame;
           altInteractPressed = Keyboard.current != null && Keyboard.current[aKey].wasPressedThisFrame;
       }


       // 1. If we are looking at something, handle interactions
       if (_currentInteractable != null)
       {
           // Trading or Normal Interaction (Primary)
           if (interactPressed)
           {
               _currentInteractable.Interact(_myId);
           }

           // Stealing (Alt)
           if (altInteractPressed)
           {
               _currentInteractable.AltInteract(_myId);
           }
       }
       
       // 2. Fallback: Drop item if NOT looking at anything
       else if (interactPressed)
       {
           // Simply ask the inventory to drop, don't write the logic here
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

