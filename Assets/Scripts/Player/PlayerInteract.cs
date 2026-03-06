using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 2f;
    // Key bindings for interaction (Player 1: E/F, Player 2: O/P)
    public string interactKey = "E"; // P1 ask
    public string giveKey = "F";     // P1 steal
    public string interactKeyP2 = "O"; // P2 ask
    public string giveKeyP2 = "P";     // P2 steal
    private PlayerInput playerInput;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
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
                        // Only allow action if this player is NOT in the spotlight
                        if (!id.spotlightOn)
                        {
                            // Show correct keys for each player
                            if (id.playerIndex == 1)
                                playerInteractable.ShowVicinityMessage(interactKeyP2, giveKeyP2);
                            else
                                playerInteractable.ShowVicinityMessage(interactKey, giveKey);

                            // Key and controller checks
                            bool interactPressed, givePressed;
                            if (id.playerIndex == 1)
                            {
                                interactPressed = Keyboard.current[GetKey(interactKeyP2)].wasPressedThisFrame;
                                givePressed = Keyboard.current[GetKey(giveKeyP2)].wasPressedThisFrame;
                                // Optionally add controller support for P2 here
                            }
                            else
                            {
                                interactPressed = Keyboard.current[GetKey(interactKey)].wasPressedThisFrame;
                                givePressed = Keyboard.current[GetKey(giveKey)].wasPressedThisFrame;
                                if (Gamepad.current != null)
                                {
                                    interactPressed |= Gamepad.current.buttonSouth.wasPressedThisFrame; // A button
                                    givePressed |= Gamepad.current.buttonEast.wasPressedThisFrame; // B button
                                }
                            }

                            if (interactPressed)
                            {
                                Debug.Log($"Player {playerIndex + 1} interacted with {otherPlayerIndex + 1}!");
                                playerInteractable.Interact(id);
                                break;
                            }

                            if (givePressed)
                            {
                                Debug.Log($"Player {playerIndex + 1} gave points to {otherPlayerIndex + 1}!");
                                playerInteractable.Give(id);
                                break;
                            }
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
