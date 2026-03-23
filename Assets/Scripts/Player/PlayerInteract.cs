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
   private PlayerInput _playerInput;
   private IInteractable _currentInteractable;
    private Transform _currentPromptTarget;


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
       HandleInput();
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
         if (_currentPromptTarget != null && _currentPromptTarget != other.transform)
         {
             ChatBubble.Clear(_currentPromptTarget);
         }

         _currentInteractable = interactable;
         _currentPromptTarget = other.transform;

         if (chatBubble != null && _myId != null && interactable.IsAvailable(_myId))
         {
             string prompt = interactable.GetInteractionPrompt(_interactKey, _altInteractKey);

             // Add a drop hint for grabbables when you have something to drop.
             if (interactable is Grabbable && _inventory != null && _inventory.HasItems)
             {
                 prompt = $"{prompt}\n[{_interactKey}] Drop";
             }

             ChatBubble.Create(chatBubble, Vector3.up * 2f, other.transform, prompt, 9999f);
         }

         if (_currentInteractable is PlayerInteractable targetPlayer) 
         {
               GetComponent<PlayerHaptics>()?.Pulse(0.3f, 0.6f);
               CameramanNPC cam = FindObjectOfType<CameramanNPC>();
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
       if (other.TryGetComponent(out IInteractable interactable) && _currentInteractable == interactable)
       {
           _currentInteractable = null;
           _holdTimer = 0;
       }

       if (_currentPromptTarget == other.transform)
       {
           ChatBubble.Clear(_currentPromptTarget);
           _currentPromptTarget = null;
       }
   }

   public IInteractable GetCurrentTarget()
   {
       return _currentInteractable;
   }


   private Key GetKey(string name) => System.Enum.TryParse(name, true, out Key k) ? k : Key.None;
}

