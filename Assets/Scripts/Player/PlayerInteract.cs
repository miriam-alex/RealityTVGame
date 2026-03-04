using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInteract : MonoBehaviour
{
    [Header("Key Bindings")]
    public string interactKey = "E";
    public string giveKey = "F";

    public float interactRange = 2f;

    private void Start()
    {
        PlayerIdentity playerIdentity = GetComponent<PlayerIdentity>();
        if (playerIdentity != null)
        {
            if (playerIdentity.playerIndex == 0) // Player 1
            {
                interactKey = "E";
                giveKey = "F";
            }
            else if (playerIdentity.playerIndex == 1) // Player 2
            {
                interactKey = "O";
                giveKey = "P";
            }
        }
    }

    // Update is called once per frame
    
    void Update()
    {
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
            foreach (Collider collider in colliderArray)
            {
                if (collider.TryGetComponent(out PlayerInteractable playerInteractable))
                {
                    PlayerIdentity id = GetComponent<PlayerIdentity>();
                    int playerIndex = id.playerIndex;
                    int otherPlayerIndex = collider.GetComponent<PlayerIdentity>().playerIndex;
                    if (playerIndex != otherPlayerIndex)
                    {
                        // If we're in range of another player, we should see a way for them to do an action.
                        // TODO: Other player's spotlight should change color.
                        playerInteractable.ShowVicinityMessage(interactKey, giveKey);
                        
                        // If we hit E to interact, then we should do the action.
                        if (Keyboard.current[GetKey(interactKey)].wasPressedThisFrame)
                        {
                            Debug.Log($"Player {playerIndex + 1} interacted with {otherPlayerIndex + 1}!");
                            playerInteractable.Interact(id);
                            break;
                        }

                        if (Keyboard.current[GetKey(giveKey)].wasPressedThisFrame) //steal points from other player
                        {
                            Debug.Log($"Player {playerIndex + 1} gave points to {otherPlayerIndex + 1}!");
                            playerInteractable.Give(id);
                            break;
                        }
                    }
                }
            }

    }

    private PlayerInteractable GetInteractableObject()
    {
        Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
        foreach (Collider collider in colliderArray)
        {
            if (collider.TryGetComponent(out PlayerInteractable playerInteractable))
            {
                int playerIndex = GetComponent<PlayerIdentity>().playerIndex;
                int otherPlayerIndex = collider.GetComponent<PlayerIdentity>().playerIndex;
                if (playerIndex != otherPlayerIndex)
                {
                    return playerInteractable;
                }
            }
        }
        return null;
    }

    private Key GetKey(string keyName)
    {
        if (System.Enum.TryParse(keyName, ignoreCase: true, out Key key))
            return key;

        Debug.LogWarning($"Invalid key name: '{keyName}', defaulting to None");
        return Key.None;
    }
}
