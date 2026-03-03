using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInteract : MonoBehaviour
{
    [Header("Key Bindings")]
    public string interactKey = "E";

    public float interactRange = 2f;

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
                        playerInteractable.ShowVicinityMessage();
                        
                        // If we hit E to interact, then we should do the action.
                        if (Keyboard.current[GetKey(interactKey)].wasPressedThisFrame)
                        {
                            Debug.Log($"Player {playerIndex + 1} interacted with {otherPlayerIndex + 1}!");
                            playerInteractable.Interact(id);
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
