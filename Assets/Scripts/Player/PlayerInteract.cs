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
       HandleVicinityDisplay();
   }
  
   private void HandleVicinityDisplay()
   {
       if (_currentInteractable == null) return;


       MonoBehaviour interactableMono = _currentInteractable as MonoBehaviour;
       if (interactableMono == null) { _currentInteractable = null; return; }


       if (!_currentInteractable.IsAvailable(_myId)) return;


       _msgTimer -= Time.deltaTime;
       if (_msgTimer <= 0)
       {
           // Pass empty strings or null because GetInteractionPrompt now ignores them
           // and uses its own Internal Identity (as per our updated PlayerInteractable)
           string prompt = _currentInteractable.GetInteractionPrompt(_interactKey, _altInteractKey);
           ChatBubble.Create(chatBubble, Vector3.up * 1.7f, interactableMono.transform, prompt, 0.25f);
      
           if (interactableMono.TryGetComponent(out SpotlightVisual visual))
               visual.FlashRed(0.1f);


           _msgTimer = 0.2f;
       }
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
           
           // // 2. Logic for Trading (Requires handshake + timer)
           // if (_currentInteractable is PlayerInteractable targetPlayer)
           // {
           //     bool iPressed = Keyboard.current[iKey].isPressed;
           //     InteractionCoordinator.Instance.SetHandshake(_myId, targetPlayer.GetPlayerIdentity(), iPressed);
           //
           //     if (iPressed && InteractionCoordinator.Instance.IsTradeReady(_myId, targetPlayer.GetPlayerIdentity()))
           //     {
           //         RunHoldTimer();
           //     }
           //     else if (Keyboard.current[iKey].wasReleasedThisFrame)
           //     {
           //         _holdTimer = 0;
           //     }
           // }
           // else
           // {
           //     // 3. Logic for Generic Objects (Instant)
           //     if (Keyboard.current[iKey].wasPressedThisFrame)
           //     {
           //         _currentInteractable.Interact(_myId);
           //     }
           // }
           
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
         _currentInteractable = interactable;
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
   }
  
   // private void RunHoldTimer()
   // {
   //     float duration = _currentInteractable.GetHoldDuration(_myId);
   //     _holdTimer += Time.deltaTime;
   //     UpdateHoldUI(Mathf.Clamp01(_holdTimer / duration));
   //
   //     if (_holdTimer >= duration)
   //     {
   //         // Just trigger the master controller
   //         _currentInteractable.Interact(_myId);
   //         _holdTimer = 0;
   //     }
   // }
  
   // private void UpdateHoldUI(float percent)
   // {
   //     int totalSegments = 10;
   //     int filledSegments = Mathf.RoundToInt(percent * totalSegments);
   //     string bar = new string('■', filledSegments) + new string('□', totalSegments - filledSegments);
   //     // string colorTag = _myId.spotlightOn ? "<color=red>" : "<color=green>";
   //     ChatBubble.Create(chatBubble, Vector3.up * 2.2f, transform, $"{bar}</color>", 0.1f);
   // }
  
   public IInteractable GetCurrentTarget()
   {
       return _currentInteractable;
   }


   private Key GetKey(string name) => System.Enum.TryParse(name, true, out Key k) ? k : Key.None;
}

