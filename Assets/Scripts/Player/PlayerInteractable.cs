using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerInteractable : MonoBehaviour
{
    public ChatBubble chatBubblePrefab;
    private PlayerIdentity id;
    private bool waitingForResponse = false;
    private PlayerIdentity currentRequestorId = null;
    
    // Track active bubbles to prevent crowding
    private List<ChatBubble> activeBubbles = new List<ChatBubble>();
    
    // Global interaction lock to prevent race conditions
    private static bool globalInteractionLock = false;
    private static PlayerInteractable currentActiveInteraction = null;

    private void Start()
    {
        id = GetComponent<PlayerIdentity>();
    }

    private void Update()
    {
        // Clean up destroyed bubbles from our tracking list
        activeBubbles.RemoveAll(bubble => bubble == null);
        
        // Handle responses when this player is being asked for points
        if (waitingForResponse && currentRequestorId != null)
        {
            bool yesPressed = Input.GetKeyDown(KeyCode.Y);
            bool noPressed = Input.GetKeyDown(KeyCode.N);
            // Controller support for Player 1
            if (id.playerIndex == 0 && UnityEngine.InputSystem.Gamepad.current != null)
            {
                yesPressed |= UnityEngine.InputSystem.Gamepad.current.buttonSouth.wasPressedThisFrame; // A button
                noPressed |= UnityEngine.InputSystem.Gamepad.current.buttonEast.wasPressedThisFrame; // B button
            }
            if (yesPressed)
            {
                AcceptPointRequest();
            }
            else if (noPressed)
            {
                RejectPointRequest();
            }
        }
    }

    private void CreateBubbleAndClearPrevious(string text, float duration = 3f)
    {
        // Destroy all previous bubbles
        foreach (var bubble in activeBubbles)
        {
            if (bubble != null)
            {
                Destroy(bubble.gameObject);
            }
        }
        activeBubbles.Clear();
        
        // Create new bubble using the existing static method
        ChatBubble.Create(chatBubblePrefab, new Vector3(0f, 1.7f, 0f), transform, text, duration);
        
        // Find and track the newly created bubble
        ChatBubble[] bubbles = GetComponentsInChildren<ChatBubble>();
        if (bubbles.Length > 0)
        {
            activeBubbles.Add(bubbles[bubbles.Length - 1]); // Add the most recently created one
        }
    }

    public void Interact(PlayerIdentity interactorId)
    {
        // Check if there's already a global interaction happening
        if (globalInteractionLock && currentActiveInteraction != this)
        {
            Debug.Log($"Interaction blocked - another player interaction is in progress");
            return;
        }
        
        if (id.spotlightOn && !waitingForResponse)
        {
            Debug.Log($"Player {interactorId.playerIndex + 1} requesting 10 points from Player {id.playerIndex + 1}");
            
            globalInteractionLock = true;
            currentActiveInteraction = this;
            
            waitingForResponse = true;
            currentRequestorId = interactorId;
            
            CreateBubbleAndClearPrevious($"Player {interactorId.playerIndex + 1} wants 10 points! [Y]es or [N]o?", 6f);
            
            StartCoroutine(RequestTimeoutDelayed());
        }
        else if (waitingForResponse)
        {
            Debug.Log($"Player {id.playerIndex + 1} is already handling a request from Player {currentRequestorId?.playerIndex + 1}");
        }
    }
    
    private static void ClearGlobalLock()
    {
        globalInteractionLock = false;
        currentActiveInteraction = null;
    }

    private IEnumerator RequestTimeoutDelayed()
    {
        // Small delay to ensure the request message is displayed first
        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(RequestTimeout());
    }

    private void AcceptPointRequest()
    {
        if (waitingForResponse && currentRequestorId != null)
        {
            Debug.Log($"Player {id.playerIndex + 1} accepted point request from Player {currentRequestorId.playerIndex + 1}");
            CreateBubbleAndClearPrevious("Yes! Here you go!", 2f);
            
            var scoreManager = FindObjectOfType<ScoreManager>();
            scoreManager.AddScore(-10, id.playerIndex); // Remove 10 from giver
            scoreManager.AddScore(10, currentRequestorId.playerIndex); // Give 10 to requestor
            scoreManager.AddScore(5, currentRequestorId.playerIndex); // Bonus +5 to requestor
            
            ResetRequest();
        }
    }

    private void RejectPointRequest()
    {
        if (waitingForResponse && currentRequestorId != null)
        {
            Debug.Log($"Player {id.playerIndex + 1} rejected point request from Player {currentRequestorId.playerIndex + 1}");
            CreateBubbleAndClearPrevious("No way!", 2f);
            
            var scoreManager = FindObjectOfType<ScoreManager>();
            scoreManager.AddScore(-5, currentRequestorId.playerIndex);
            
            ResetRequest();
        }
    }

    private IEnumerator RequestTimeout()
    {
        yield return new WaitForSeconds(5f);
        
        if (waitingForResponse)
        {
            Debug.Log($"Point request from Player {currentRequestorId?.playerIndex + 1} to Player {id.playerIndex + 1} timed out");
            CreateBubbleAndClearPrevious("No response...", 2f);
            
            // Decrease requestor's score by 5 for no response
            if (currentRequestorId != null)
            {
                var scoreManager = FindObjectOfType<ScoreManager>();
                scoreManager.AddScore(-5, currentRequestorId.playerIndex);
            }
            
            ResetRequest();
        }
    }

    private void ResetRequest()
    {
        waitingForResponse = false;
        currentRequestorId = null;
        ClearGlobalLock();
    }

    public void Give(PlayerIdentity interactorId)
    {
        // Check if there's already a global interaction happening
        if (globalInteractionLock && currentActiveInteraction != this)
        {
            Debug.Log($"Steal attempt blocked - another player interaction is in progress");
            return;
        }
        
        if (id.spotlightOn)
        {
            Debug.Log($"Player {interactorId.playerIndex + 1} stealing points from Player {id.playerIndex + 1}");
            
            // Set global lock for steal action
            globalInteractionLock = true;
            currentActiveInteraction = this;
            
            CreateBubbleAndClearPrevious("MY POINTS!", 2f);
            FindObjectOfType<ScoreManager>().StealPoints(id.playerIndex, interactorId.playerIndex);
            
            // Clear lock after steal action completes
            StartCoroutine(ClearStealLock());
        }
    }
    
    private IEnumerator ClearStealLock()
    {
        // Wait for the bubble duration, then clear lock
        yield return new WaitForSeconds(2.5f);
        ClearGlobalLock();
    }
    
    
    public void ShowVicinityMessage(string interactKey = "E", string giveKey = "F")
    {
        // Don't show vicinity message if we're waiting for a response to a point request
        // or if there's a global interaction lock
        if (id.spotlightOn && !waitingForResponse && !globalInteractionLock)
        {
            CreateBubbleAndClearPrevious($"[{interactKey}] Ask Points | [{giveKey}] Steal Points", 0.25f);
            GetComponentInChildren<SpotlightVisual>().FlashRed(0.1f);
        }
    }

}
